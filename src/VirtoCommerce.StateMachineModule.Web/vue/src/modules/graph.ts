// The state machine graph of the visual editor and its JSON form (StateMachineDefinition.statesGraph): the legacy
// stateMachineStateService / TransitionService / LayoutService / SnapshotService as plain functions.
import type { ConditionNode } from "./types";

export const STATE_WIDTH = 150;
export const STATE_HEIGHT = 100;

export type Point = { x: number; y: number };

export type GraphState = {
  id: string;
  name: string;
  description: string;
  isInitial: boolean;
  isFinal: boolean;
  isSuccess: boolean;
  isFailed: boolean;
  position: Point;
  transitions: GraphTransition[];
};

export type GraphTransition = {
  id: string;
  trigger: string;
  description: string;
  from: GraphState;
  to: GraphState;
  condition?: ConditionNode;
};

export type Graph = { states: GraphState[]; transitions: GraphTransition[] };

type StoredState = {
  id?: string;
  name?: string;
  description?: string;
  isInitial?: boolean;
  isFinal?: boolean;
  isSuccess?: boolean;
  isFailed?: boolean;
  position?: Partial<Point>;
  transitions?: {
    id?: string;
    trigger?: string;
    description?: string;
    toState?: string | { id?: string; name?: string };
    condition?: ConditionNode;
  }[];
};

// same form as the legacy ids, which the stored graphs already hold
export const newTransitionId = () => "transition_" + Math.random().toString(36).slice(2, 11);

/** Reads statesGraph; `positioned` is false when a state has no valid position, so the graph needs a layout. */
export function parseGraph(json?: string): Graph & { positioned: boolean } {
  const data = json ? (JSON.parse(json) as unknown) : [];
  const list = (Array.isArray(data) ? data : []) as StoredState[];
  const byId = new Map<string, GraphState>();
  let positioned = true;

  const states = list.map((s) => {
    const valid =
      typeof s.position?.x === "number" && typeof s.position?.y === "number" && s.position.x >= 0 && s.position.y >= 0;
    if (!valid) positioned = false;
    const state: GraphState = {
      // legacy graphs may have no id: the name is the id
      id: s.id || s.name || "",
      name: s.name ?? "",
      description: s.description ?? "",
      isInitial: !!s.isInitial,
      isFinal: !!s.isFinal,
      isSuccess: !!s.isSuccess,
      isFailed: !!s.isFailed,
      position: valid ? { x: s.position!.x!, y: s.position!.y! } : { x: 0, y: 0 },
      transitions: [],
    };
    byId.set(state.id, state);
    return state;
  });

  const transitions: GraphTransition[] = [];
  list.forEach((s) => {
    const from = byId.get(s.id || s.name || "");
    for (const t of s.transitions ?? []) {
      const toId = typeof t.toState === "string" ? t.toState : t.toState?.id || t.toState?.name;
      const to = toId ? byId.get(toId) : undefined;
      if (!from || !to) continue;
      const transition: GraphTransition = {
        id: t.id || newTransitionId(),
        trigger: t.trigger ?? "",
        description: t.description ?? "",
        from,
        to,
        condition: t.condition,
      };
      transitions.push(transition);
      from.transitions.push(transition);
    }
  });

  const graph = { states, transitions, positioned };
  updateStateFlags(graph);
  return graph;
}

/** The graph as statesGraph JSON, in the legacy editor's format. */
export function serializeGraph(graph: Graph): string {
  return JSON.stringify(
    graph.states.map((s) => ({
      id: s.id,
      name: s.name,
      description: s.description || "",
      isInitial: s.isInitial,
      isFinal: s.isFinal,
      isSuccess: s.isSuccess,
      isFailed: s.isFailed,
      position: s.position,
      transitions: s.transitions.map((t) => ({
        id: t.id,
        trigger: t.trigger,
        description: t.description || "",
        toState: t.to.id,
        condition: t.condition,
      })),
    })),
  );
}

/** A state with no incoming transition is initial, one with no outgoing is final; only final states succeed or fail. */
export function updateStateFlags(graph: Graph) {
  for (const state of graph.states) {
    state.isInitial = !graph.transitions.some((t) => t.to === state);
    state.isFinal = state.transitions.length === 0;
    if (!state.isFinal) {
      state.isSuccess = false;
      state.isFailed = false;
    }
  }
}

export function addTransition(graph: Graph, from: GraphState, to: GraphState, data: { trigger: string; description: string }) {
  const transition: GraphTransition = { id: newTransitionId(), ...data, from, to };
  graph.transitions.push(transition);
  from.transitions.push(transition);
  updateStateFlags(graph);
  return transition;
}

export function removeTransition(graph: Graph, transition: GraphTransition) {
  graph.transitions.splice(graph.transitions.indexOf(transition), 1);
  transition.from.transitions.splice(transition.from.transitions.indexOf(transition), 1);
  updateStateFlags(graph);
}

export function removeState(graph: Graph, state: GraphState) {
  for (const t of graph.transitions.filter((x) => x.from === state || x.to === state)) removeTransition(graph, t);
  graph.states.splice(graph.states.indexOf(state), 1);
  updateStateFlags(graph);
}

/** Adds a state; the first state of a graph is its initial one. */
export function addState(graph: Graph, data: { name: string; description: string }, position: Point) {
  const state: GraphState = {
    id: data.name,
    name: data.name,
    description: data.description,
    isInitial: graph.states.length === 0,
    isFinal: true,
    isSuccess: false,
    isFailed: false,
    position,
    transitions: [],
  };
  graph.states.push(state);
  updateStateFlags(graph);
  return state;
}

/** The legacy "Insert state here": the transition now ends at the new state, which gets a new transition to the old target. */
export function insertState(
  graph: Graph,
  transition: GraphTransition,
  state: { name: string; description: string },
  next: { trigger: string; description: string },
) {
  const target = transition.to;
  const position = {
    x: (transition.from.position.x + target.position.x) / 2,
    y: (transition.from.position.y + target.position.y) / 2,
  };
  const inserted = addState(graph, state, position);
  transition.to = inserted;
  addTransition(graph, inserted, target, next);
  return inserted;
}

/** The transition's curve (bottom middle of its source to top middle of its target) and its label point. */
export function transitionPath(from: Point, to: Point) {
  return curve(from.x + STATE_WIDTH / 2, from.y + STATE_HEIGHT, to.x + STATE_WIDTH / 2, to.y);
}

export function curve(startX: number, startY: number, endX: number, endY: number) {
  const distance = Math.hypot(endX - startX, endY - startY);
  const intensity = Math.min(Math.max(distance * 0.2, 30), 100);
  const cp1y = startY + intensity;
  const cp2y = endY - intensity;
  // the cubic Bezier at t = 0.5
  const label = { x: (startX + endX) / 2, y: 0.125 * startY + 0.375 * cp1y + 0.375 * cp2y + 0.125 * endY };
  return { d: `M ${startX} ${startY} C ${startX} ${cp1y}, ${endX} ${cp2y}, ${endX} ${endY}`, label };
}

/** Longest distance from an initial state (1); on a cycle, breadth-first depth instead (legacy calculateStateLevels). */
function levels(graph: Graph) {
  const level = new Map<GraphState, number>(graph.states.map((s) => [s, s.isInitial ? 1 : 0]));
  let changed = true;
  let iterations = 0;
  while (changed && iterations < graph.states.length * 2) {
    changed = false;
    iterations++;
    for (const state of graph.states) {
      const incoming = graph.transitions.filter((t) => t.to === state).map((t) => level.get(t.from)!);
      if (incoming.length && Math.max(...incoming) + 1 > level.get(state)!) {
        level.set(state, Math.max(...incoming) + 1);
        changed = true;
      }
    }
  }
  if (changed) {
    graph.states.forEach((s) => level.set(s, s.isInitial ? 1 : 0));
    const queue = graph.states.filter((s) => s.isInitial).map((s) => ({ state: s, depth: 1 }));
    const visited = new Set<GraphState>();
    while (queue.length) {
      const { state, depth } = queue.shift()!;
      if (visited.has(state)) continue;
      visited.add(state);
      level.set(state, Math.max(level.get(state)!, depth));
      state.transitions.forEach((t) => queue.push({ state: t.to, depth: depth + 1 }));
    }
  }
  return level;
}

const parentsOf = (graph: Graph, state: GraphState) =>
  graph.transitions.filter((t) => t.to === state && t.from !== state).map((t) => t.from);
const averageX = (states: GraphState[]) =>
  states.length ? states.reduce((sum, s) => sum + s.position.x, 0) / states.length : 0;

/** Lays the states out in rows by level, each under its parents and centred in `width` (legacy normalizeStateLayout). */
export function layoutGraph(graph: Graph, width: number) {
  const level = levels(graph);
  const rows = new Map<number, GraphState[]>();
  graph.states.forEach((s) => rows.set(level.get(s)!, [...(rows.get(level.get(s)!) ?? []), s]));
  const sorted = [...rows.keys()].sort((a, b) => a - b);
  let spacing = 200;

  const first = rows.get(1) ?? [];
  const firstSpacing = Math.max(spacing, (width - first.length * STATE_WIDTH) / (first.length + 1));
  first.forEach((s, i) => (s.position = { x: firstSpacing + i * (STATE_WIDTH + firstSpacing), y: 50 }));

  // the legacy layout widened the spacing while that reduced the crossings, at most 3 times
  let previous = Infinity;
  for (let attempt = 0; attempt < 3; attempt++) {
    sorted.forEach((key, row) => {
      if (key === 1) return;
      const y = 50 + row * (STATE_HEIGHT + 150);
      const groups = new Map<string, GraphState[]>();
      [...rows.get(key)!]
        .sort((a, b) => averageX(parentsOf(graph, a)) - averageX(parentsOf(graph, b)))
        .forEach((s) => {
          const parentKey = parentsOf(graph, s).map((p) => p.id).sort().join(",");
          groups.set(parentKey, [...(groups.get(parentKey) ?? []), s]);
        });
      let left = 50;
      for (const group of groups.values()) {
        const groupWidth = group.length * STATE_WIDTH + (group.length - 1) * spacing;
        const parentX = group.reduce((sum, s) => sum + averageX(parentsOf(graph, s)), 0) / group.length;
        const start = Math.max(left, parentX - groupWidth / 2);
        group.forEach((s, i) => (s.position = { x: start + i * (STATE_WIDTH + spacing), y }));
        left = start + groupWidth + spacing;
      }
    });
    const crossings = countCrossings(graph);
    if (crossings === 0 || crossings >= previous) break;
    previous = crossings;
    spacing += 50;
  }

  // no overlaps within a row
  for (const row of rows.values()) {
    row.sort((a, b) => a.position.x - b.position.x);
    for (let i = 1; i < row.length; i++) {
      const overlap = STATE_WIDTH + spacing - (row[i].position.x - row[i - 1].position.x);
      if (overlap > 0) row[i].position.x += overlap;
    }
  }

  // centred when it fits
  const minX = Math.min(...graph.states.map((s) => s.position.x));
  const maxX = Math.max(...graph.states.map((s) => s.position.x + STATE_WIDTH));
  if (graph.states.length && maxX - minX < width) {
    const offset = (width - (maxX - minX)) / 2 - minX;
    graph.states.forEach((s) => (s.position.x += offset));
  }
}

function segmentsCross(a: number[], b: number[]) {
  const [x1, y1, x2, y2] = a;
  const [x3, y3, x4, y4] = b;
  const denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
  if (denom === 0) return false;
  const ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
  const ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;
  return ua > 0 && ua < 1 && ub > 0 && ub < 1;
}

/** Transitions (as straight lines) crossing each other or a state they don't touch. */
function countCrossings(graph: Graph) {
  const lines = graph.transitions.map((t) => ({
    t,
    line: [
      t.from.position.x + STATE_WIDTH / 2,
      t.from.position.y + STATE_HEIGHT,
      t.to.position.x + STATE_WIDTH / 2,
      t.to.position.y,
    ],
  }));
  let count = 0;
  lines.forEach((a, i) => {
    lines.slice(i + 1).forEach((b) => segmentsCross(a.line, b.line) && count++);
    graph.states
      .filter((s) => s !== a.t.from && s !== a.t.to)
      .forEach((s) => {
        const { x, y } = s.position;
        const edges = [
          [x, y, x + STATE_WIDTH, y],
          [x + STATE_WIDTH, y, x + STATE_WIDTH, y + STATE_HEIGHT],
          [x + STATE_WIDTH, y + STATE_HEIGHT, x, y + STATE_HEIGHT],
          [x, y + STATE_HEIGHT, x, y],
        ];
        if (edges.some((e) => segmentsCross(a.line, e))) count++;
      });
  });
  return count;
}

/**
 * Moves the graph so its top-left state sits at (margin, margin), as the legacy editor did before saving. Only the
 * positions change: fields the editor doesn't know (typed in the JSON mode) are kept.
 */
export function normalizePositions(json: string | undefined, margin = 50) {
  if (!json) return json;
  let stored: StoredState[];
  try {
    stored = JSON.parse(json);
  } catch {
    return json;
  }
  if (!Array.isArray(stored) || !stored.length) return json;
  if (stored.some((s) => typeof s.position?.x !== "number" || typeof s.position?.y !== "number")) return json;
  const minX = Math.min(...stored.map((s) => s.position!.x!));
  const minY = Math.min(...stored.map((s) => s.position!.y!));
  if (minX === margin && minY === margin) return json;
  stored.forEach((s) => (s.position = { ...s.position, x: s.position!.x! - minX + margin, y: s.position!.y! - minY + margin }));
  return JSON.stringify(stored);
}

export function bounds(states: GraphState[], margin: number) {
  if (!states.length) return { minX: 0, minY: 0, maxX: 0, maxY: 0 };
  return {
    minX: Math.max(0, Math.min(...states.map((s) => s.position.x)) - margin),
    minY: Math.max(0, Math.min(...states.map((s) => s.position.y)) - margin),
    maxX: Math.max(...states.map((s) => s.position.x + STATE_WIDTH)) + margin,
    maxY: Math.max(...states.map((s) => s.position.y + STATE_HEIGHT)) + margin,
  };
}

const escapeXml = (text: string) =>
  text.replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" })[c]!);
const clip = (text: string, max: number) => (text.length > max ? text.slice(0, max - 1) + "…" : text);

/**
 * The graph as an SVG image: the preview stored with the definition (statesCapture). It is a saved image, so it uses a
 * fixed light palette, not the theme's tokens. Drawn from the data instead of the legacy html2canvas screenshot, so no
 * third-party script is loaded.
 */
export function graphSvg(json?: string) {
  let graph: ReturnType<typeof parseGraph>;
  try {
    graph = parseGraph(json);
  } catch {
    return undefined;
  }
  if (!graph.states.length) return undefined;
  if (!graph.positioned) layoutGraph(graph, 800);
  const b = bounds(graph.states, 20);
  const width = b.maxX - b.minX;
  const height = b.maxY - b.minY;
  const paths = graph.transitions
    .map((t) => {
      const { d, label } = transitionPath(t.from.position, t.to.position);
      return (
        `<path d="${d}" fill="none" stroke="#737373" stroke-width="1.5" marker-end="url(#a)"/>` +
        `<text x="${label.x}" y="${label.y + 4}" text-anchor="middle" font-size="12" fill="#262626" paint-order="stroke" stroke="#fff" stroke-width="4">${escapeXml(clip(t.trigger, 24))}</text>`
      );
    })
    .join("");
  const nodes = graph.states
    .map((s) => {
      const { x, y } = s.position;
      const head = s.isSuccess ? "#57ab79" : s.isFailed ? "#ff4a4a" : "#319ed4";
      return (
        `<rect x="${x}" y="${y}" width="${STATE_WIDTH}" height="${STATE_HEIGHT}" rx="6" fill="#fff" stroke="#a3a3a3"/>` +
        `<path d="M ${x} ${y + 6} a 6 6 0 0 1 6 -6 h ${STATE_WIDTH - 12} a 6 6 0 0 1 6 6 v 26 h -${STATE_WIDTH} z" fill="${head}"/>` +
        `<text x="${x + STATE_WIDTH / 2}" y="${y + 21}" text-anchor="middle" font-size="13" font-weight="600" fill="#fff">${escapeXml(clip(s.name, 20))}</text>` +
        `<text x="${x + STATE_WIDTH / 2}" y="${y + 62}" text-anchor="middle" font-size="11" fill="#525252">${escapeXml(clip(s.description, 24))}</text>`
      );
    })
    .join("");
  return (
    `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="${b.minX} ${b.minY} ${width} ${height}" font-family="Arial, sans-serif">` +
    `<defs><marker id="a" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M0 0 L10 5 L0 10 z" fill="#737373"/></marker></defs>` +
    `<rect x="${b.minX}" y="${b.minY}" width="${width}" height="${height}" fill="#fff"/>${paths}${nodes}</svg>`
  );
}

export const svgDataUrl = (svg: string) => "data:image/svg+xml;charset=utf-8," + encodeURIComponent(svg);

/** The graph as a 380px wide PNG data URL, the size the legacy editor stored. */
export async function graphSnapshot(json?: string): Promise<string | undefined> {
  const svg = graphSvg(json);
  if (!svg) return undefined;
  const image = new Image();
  image.src = svgDataUrl(svg);
  await image.decode();
  const canvas = document.createElement("canvas");
  canvas.width = 380;
  canvas.height = Math.round((380 * image.naturalHeight) / image.naturalWidth);
  const context = canvas.getContext("2d")!;
  context.imageSmoothingEnabled = true;
  context.imageSmoothingQuality = "high";
  context.drawImage(image, 0, 0, canvas.width, canvas.height);
  return canvas.toDataURL("image/png");
}
