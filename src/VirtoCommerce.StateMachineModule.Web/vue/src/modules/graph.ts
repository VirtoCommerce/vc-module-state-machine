// The state machine graph the visual editor draws, and its JSON form (StateMachineDefinition.statesGraph):
// the AngularJS stateMachine*Service helpers (state, transition, layout, snapshot) as plain functions.
import { generateId } from "@vc-shell/framework";

export const STATE_WIDTH = 150;
export const STATE_HEIGHT = 100;

export interface Point {
  x: number;
  y: number;
}

export interface GraphState {
  id: string;
  name: string;
  description: string;
  isInitial: boolean;
  isFinal: boolean;
  isSuccess: boolean;
  isFailed: boolean;
  position: Point;
  transitions: GraphTransition[];
}

export interface GraphTransition {
  id: string;
  trigger: string;
  description: string;
  from: GraphState;
  to: GraphState;
  condition?: unknown;
}

export interface Graph {
  states: GraphState[];
  transitions: GraphTransition[];
}

interface StoredState {
  id?: string;
  name?: string;
  description?: string;
  isInitial?: boolean;
  isFinal?: boolean;
  isSuccess?: boolean;
  isFailed?: boolean;
  position?: Partial<Point>;
  transitions?: { id?: string; trigger?: string; description?: string; toState?: string | { id?: string; name?: string }; condition?: unknown }[];
}

export const newTransitionId = () => "transition_" + generateId();

/** Reads statesGraph; `positioned` is false when some state has no valid position (it needs a layout). */
export function parseGraph(json?: string): Graph & { positioned: boolean } {
  const data = (json ? JSON.parse(json) : []) as StoredState[];
  const list = Array.isArray(data) ? data : [];
  const byId = new Map<string, GraphState>();
  let positioned = true;

  const states = list.map((s) => {
    const valid = typeof s.position?.x === "number" && typeof s.position?.y === "number" && s.position.x >= 0 && s.position.y >= 0;
    if (!valid) positioned = false;
    const state: GraphState = {
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

export function addTransition(graph: Graph, from: GraphState, to: GraphState, data: { trigger: string; description: string }) {
  const transition: GraphTransition = { id: newTransitionId(), ...data, from, to };
  graph.transitions.push(transition);
  from.transitions.push(transition);
  updateStateFlags(graph);
  return transition;
}

/** The transition's curve (bottom middle of its source to top middle of its target) and the label point on it. */
export function transitionPath(from: Point, to: Point) {
  const startX = from.x + STATE_WIDTH / 2;
  const startY = from.y + STATE_HEIGHT;
  const endX = to.x + STATE_WIDTH / 2;
  const endY = to.y;
  return curve(startX, startY, endX, endY);
}

export function curve(startX: number, startY: number, endX: number, endY: number) {
  const distance = Math.hypot(endX - startX, endY - startY);
  const intensity = Math.min(Math.max(distance * 0.2, 30), 100);
  const cp1y = startY + intensity;
  const cp2y = endY - intensity;
  // cubic Bezier at t = 0.5
  const label = { x: (startX + endX) / 2, y: 0.125 * startY + 0.375 * cp1y + 0.375 * cp2y + 0.125 * endY };
  return { d: `M ${startX} ${startY} C ${startX} ${cp1y}, ${endX} ${cp2y}, ${endX} ${endY}`, label };
}

/** Breadth-first depth from the initial states (1); states no initial state reaches stay at 0. */
function levels(graph: Graph) {
  const level = new Map<GraphState, number>();
  const queue = graph.states.filter((s) => s.isInitial).map((s) => ({ state: s, depth: 1 }));
  while (queue.length) {
    const { state, depth } = queue.shift()!;
    if (level.has(state)) continue;
    level.set(state, depth);
    state.transitions.forEach((t) => queue.push({ state: t.to, depth: depth + 1 }));
  }
  return level;
}

/**
 * Lays the states out in rows by depth, each row ordered under its parents and centred in `width`.
 * ponytail: the AngularJS layout also widened the spacing while transitions crossed; add that back if real graphs
 * come out tangled.
 */
export function layoutGraph(graph: Graph, width: number) {
  const level = levels(graph);
  const rows = new Map<number, GraphState[]>();
  graph.states.forEach((s) => rows.set(level.get(s) ?? 0, [...(rows.get(level.get(s) ?? 0) ?? []), s]));
  const gap = 200;
  [...rows.keys()]
    .sort((a, b) => a - b)
    .forEach((depth, row) => {
      const parentX = (s: GraphState) => {
        const parents = graph.transitions.filter((t) => t.to === s && t.from !== s).map((t) => t.from.position.x);
        return parents.length ? parents.reduce((a, b) => a + b, 0) / parents.length : 0;
      };
      const states = rows.get(depth)!.sort((a, b) => parentX(a) - parentX(b));
      const rowWidth = states.length * STATE_WIDTH + (states.length - 1) * gap;
      const left = Math.max(50, (width - rowWidth) / 2);
      states.forEach((s, i) => (s.position = { x: left + i * (STATE_WIDTH + gap), y: 50 + row * (STATE_HEIGHT + 150) }));
    });
}

/**
 * Moves the graph so that its top-left state sits at (margin, margin), as the AngularJS editor did before saving.
 * Only the positions change: fields the editor doesn't know (typed in the JSON mode) are kept.
 */
export function normalizePositions(json: string, margin = 50) {
  const graph = parseGraph(json);
  if (!graph.states.length || !graph.positioned) return json;
  const minX = Math.min(...graph.states.map((s) => s.position.x));
  const minY = Math.min(...graph.states.map((s) => s.position.y));
  const stored = JSON.parse(json) as StoredState[];
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

const escape = (text: string) => text.replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" })[c]!);
const clip = (text: string, max: number) => (text.length > max ? text.slice(0, max - 1) + "…" : text);

function headColor(s: GraphState) {
  if (s.isSuccess) return "#449463";
  if (s.isFailed) return "#ED2020";
  return "#5983A8";
}

/**
 * The graph as an SVG image (the definition's statesCapture preview). It is an image stored with the definition, so it
 * has its own fixed light palette rather than the theme's tokens.
 * ponytail: drawn from the data, not a screenshot of the editor (the AngularJS html2canvas); multi-line descriptions are
 * cut to one line.
 */
export function graphSvg(json?: string) {
  const graph = parseGraph(json);
  if (!graph.states.length) return undefined;
  if (!graph.positioned) layoutGraph(graph, 800);
  const b = bounds(graph.states, 20);
  const width = b.maxX - b.minX;
  const height = b.maxY - b.minY;
  const paths = graph.transitions
    .map((t) => {
      const { d, label } = transitionPath(t.from.position, t.to.position);
      return `<path d="${d}" fill="none" stroke="#737373" stroke-width="1.5" marker-end="url(#a)"/><text x="${label.x}" y="${label.y + 4}" text-anchor="middle" font-size="12" fill="#262626" paint-order="stroke" stroke="#fff" stroke-width="4">${escape(clip(t.trigger, 24))}</text>`;
    })
    .join("");
  const nodes = graph.states
    .map((s) => {
      const { x, y } = s.position;
      const head = headColor(s);
      return `<rect x="${x}" y="${y}" width="${STATE_WIDTH}" height="${STATE_HEIGHT}" rx="6" fill="#fff" stroke="#A3A3A3"/><path d="M ${x} ${y + 6} a 6 6 0 0 1 6 -6 h ${STATE_WIDTH - 12} a 6 6 0 0 1 6 6 v 26 h -${STATE_WIDTH} z" fill="${head}"/><text x="${x + STATE_WIDTH / 2}" y="${y + 21}" text-anchor="middle" font-size="13" font-weight="600" fill="#fff">${escape(clip(s.name, 20))}</text><text x="${x + STATE_WIDTH / 2}" y="${y + 62}" text-anchor="middle" font-size="11" fill="#525252">${escape(clip(s.description, 24))}</text>`;
    })
    .join("");
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="${b.minX} ${b.minY} ${width} ${height}" font-family="Arial, sans-serif"><defs><marker id="a" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M0 0 L10 5 L0 10 z" fill="#737373"/></marker></defs><rect x="${b.minX}" y="${b.minY}" width="${width}" height="${height}" fill="#fff"/>${paths}${nodes}</svg>`;
}

export const svgDataUrl = (svg: string) => "data:image/svg+xml;charset=utf-8," + encodeURIComponent(svg);

/** The graph as a 380px wide PNG data URL, the size the AngularJS editor stored. */
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
  context.imageSmoothingQuality = "high";
  context.drawImage(image, 0, 0, canvas.width, canvas.height);
  return canvas.toDataURL("image/png");
}
