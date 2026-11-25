<template>
  <VcBlade
    :title="$t('STATE_MACHINE.EDITOR.TITLE')"
    :subtitle="model?.entity.name"
    :toolbar-items="toolbar"
    width="70%"
  >
    <div
      v-if="mode === 'json'"
      class="sm-json"
    >
      <textarea
        v-model="jsonText"
        :class="['sm-json__text', { 'sm-json__text--invalid': jsonInvalid }]"
        :readonly="!canEdit"
        spellcheck="false"
        :aria-label="$t('STATE_MACHINE.TOOLBAR.JSON_MODE')"
        :aria-invalid="jsonInvalid"
        @input="onJsonInput"
      ></textarea>
      <VcHint
        v-if="jsonInvalid"
        class="sm-json__error"
        >{{ $t("STATE_MACHINE.EDITOR.INVALID_JSON") }}</VcHint
      >
    </div>
    <div
      v-else
      ref="canvasEl"
      class="sm-canvas"
      role="application"
      :aria-label="$t('STATE_MACHINE.EDITOR.CANVAS')"
      @contextmenu.prevent
    >
      <div
        ref="innerEl"
        class="sm-canvas__inner"
        :style="{ width: `${size.width}px`, height: `${size.height}px` }"
      >
        <svg
          class="sm-canvas__lines"
          :width="size.width"
          :height="size.height"
        >
          <defs>
            <marker
              id="sm-arrow"
              viewBox="0 0 10 10"
              refX="9"
              refY="5"
              markerWidth="7"
              markerHeight="7"
              orient="auto"
            >
              <path
                d="M0 0 L10 5 L0 10 z"
                class="sm-canvas__arrow"
              />
            </marker>
          </defs>
          <g
            v-for="t in graph.transitions"
            :key="t.id"
            :class="['sm-transition', { 'sm-transition--hovered': isHovered(t) }]"
            @mouseenter="hoverTransition = t"
            @mouseleave="hoverTransition = undefined"
            @contextmenu.prevent.stop="transitionMenu($event, t)"
            @dblclick.stop="editTransition(t)"
          >
            <!-- a wide transparent stroke, so the curve is easy to hit -->
            <path
              :d="transitionPath(t.from.position, t.to.position).d"
              class="sm-transition__hit"
            />
            <path
              :d="transitionPath(t.from.position, t.to.position).d"
              class="sm-transition__line"
              marker-end="url(#sm-arrow)"
            />
            <text
              :x="transitionPath(t.from.position, t.to.position).label.x"
              :y="transitionPath(t.from.position, t.to.position).label.y + 4"
              class="sm-transition__label"
            >
              {{ t.trigger }}{{ t.condition ? " ⚑" : "" }}
              <title v-if="t.condition">{{ $t("STATE_MACHINE.EDITOR.HAS_CONDITION") }}</title>
            </text>
          </g>
          <path
            v-if="connecting"
            :d="connecting.path"
            class="sm-transition__line sm-transition__line--temp"
          />
        </svg>

        <!-- legacy state-node -->
        <div
          v-for="s in graph.states"
          :key="s.id"
          :class="['sm-state', { 'sm-state--hovered': isHovered(s), 'sm-state--editable': canEdit }]"
          :style="{ left: `${s.position.x}px`, top: `${s.position.y}px` }"
          tabindex="0"
          :aria-label="s.name"
          @pointerdown="startDrag($event, s)"
          @dblclick.stop="editState(s)"
          @contextmenu.prevent.stop="stateMenu($event, s)"
          @keydown.enter.prevent="editState(s)"
          @keydown.shift.f10.prevent="stateMenu($event, s)"
          @mouseenter="hoverState = s"
          @mouseleave="hoverState = undefined"
        >
          <div :class="['sm-state__header', { 'sm-state__header--success': s.isSuccess, 'sm-state__header--failed': s.isFailed }]">
            {{ s.name }}
          </div>
          <div class="sm-state__description">{{ s.description || $t("STATE_MACHINE.EDITOR.NO_DESCRIPTION") }}</div>
          <ThreeWayToggle
            v-if="s.isFinal"
            class="sm-state__toggle"
            :model-value="s.isSuccess ? 'left' : s.isFailed ? 'right' : 'center'"
            :disabled="!canEdit"
            @update:model-value="(p) => setOutcome(s, p)"
          />
          <!-- drag from here to another state (or to empty space) to add a transition -->
          <div
            v-if="canEdit"
            class="sm-state__connector"
            @pointerdown.stop="startConnect($event, s)"
          ></div>
        </div>

        <ContextMenu
          v-if="menu"
          :items="menu.items"
          :x="menu.x"
          :y="menu.y"
          @close="closeMenu"
        />
      </div>
    </div>

    <FormDialog
      v-if="dialog"
      :key="dialog.key"
      :title="dialog.title"
      :fields="dialog.fields"
      :initial="dialog.initial"
      :validate="dialog.validate"
      @ok="dialog.resolve"
      @cancel="dialog.resolve(undefined)"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcHint } from "@vc-shell/framework/ui";
import { computed, onBeforeUnmount, onMounted, reactive, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api, clone } from "../api";
import ContextMenu, { type MenuItem } from "../components/ContextMenu.vue";
import FormDialog, { type DialogField } from "../components/FormDialog.vue";
import ThreeWayToggle, { type TogglePosition } from "../components/ThreeWayToggle.vue";
import { hasConditionTree, loadEntityTypes } from "../entityTypes";
import {
  STATE_HEIGHT,
  STATE_WIDTH,
  addState,
  addTransition,
  bounds,
  curve,
  insertState,
  layoutGraph,
  parseGraph,
  removeState,
  removeTransition,
  serializeGraph,
  transitionPath,
  updateStateFlags,
  type Graph,
  type GraphState,
  type GraphTransition,
  type Point,
} from "../graph";
import { applyItemValues, itemValues, renameItem } from "../texts";
import type { ConditionNode, ConditionOptions, EditorOptions } from "../types";
import type { StateMachineEntityType, StateMachineSettings } from "../../api_client/virtocommerce.statemachine";

// legacy state-machine-visual-editor (with its state-node, state-transitions, context-menu and modal-form components)
defineBlade({ url: "/state-machine-editor", name: "StateMachineVisualEditor", permissions: ["statemachine:read"] });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeSelf, exposeToChildren } = useBlade<EditorOptions>();
const { hasAccess } = usePermissions();

// the details blade's model: changes here are its unsaved changes, its Save persists them
const model = computed(() => options.value?.model);
const canEdit = computed(() => hasAccess("statemachine:update"));
const canLocalize = computed(() => hasAccess("statemachine:localize"));

const graph = reactive<Graph>({ states: [], transitions: [] });
const mode = ref<"visual" | "json">("visual");
const jsonText = ref("");
const jsonInvalid = ref(false);

const settings = ref<StateMachineSettings>({});
const entityTypes = ref<StateMachineEntityType[]>([]);
const canvasEl = ref<HTMLElement>();
const innerEl = ref<HTMLElement>();
const viewport = reactive({ width: 800, height: 600 });

/** Builds the graph from the model's statesGraph; a graph without positions is laid out to fit the canvas. */
function readGraph() {
  let parsed: ReturnType<typeof parseGraph>;
  try {
    parsed = parseGraph(model.value?.entity.statesGraph);
  } catch {
    parsed = { states: [], transitions: [], positioned: true };
  }
  // laid out for display only: the model changes when the user edits something
  if (!parsed.positioned) layoutGraph(parsed, viewport.width);
  graph.states = parsed.states;
  graph.transitions = parsed.transitions;
}

/** Writes the graph back to the model (legacy updateCurrentEntity). */
function commit() {
  if (model.value) model.value.entity.statesGraph = serializeGraph(graph);
}

// the canvas grows with the graph, and is at least as large as the blade
const size = computed(() => {
  const b = bounds(graph.states, 0);
  return { width: Math.max(viewport.width, b.maxX + 200), height: Math.max(viewport.height, b.maxY + 200) };
});

let resizeObserver: ResizeObserver | undefined;
watch(canvasEl, (el) => {
  resizeObserver?.disconnect();
  if (!el) return;
  resizeObserver = new ResizeObserver(() => {
    viewport.width = el.clientWidth;
    viewport.height = el.clientHeight;
  });
  resizeObserver.observe(el);
});

/** Pointer position on the canvas (scroll included). */
function pointOf(e: { clientX: number; clientY: number }): Point {
  const rect = innerEl.value!.getBoundingClientRect();
  return { x: e.clientX - rect.left, y: e.clientY - rect.top };
}

const stateAt = (p: Point) =>
  graph.states.find(
    (s) => p.x >= s.position.x && p.x <= s.position.x + STATE_WIDTH && p.y >= s.position.y && p.y <= s.position.y + STATE_HEIGHT,
  );

// hover highlights a state's transitions, or a transition's two states (legacy onStateHover / onTransitionHover)
const hoverState = shallowRef<GraphState>();
const hoverTransition = shallowRef<GraphTransition>();
function isHovered(item: GraphState | GraphTransition) {
  if ("trigger" in item) return hoverTransition.value === item || item.from === hoverState.value || item.to === hoverState.value;
  return hoverState.value === item || hoverTransition.value?.from === item || hoverTransition.value?.to === item;
}

// dialogs: one at a time, awaited by the action that opened it
type Dialog = {
  key: number;
  title: string;
  fields: DialogField[];
  initial: Record<string, string>;
  validate?: (values: Record<string, string>) => Record<string, string> | undefined;
  resolve: (values?: Record<string, string>) => void;
};
const dialog = shallowRef<Dialog>();
let dialogKey = 0;
function ask(d: Omit<Dialog, "key" | "resolve">) {
  return new Promise<Record<string, string> | undefined>((resolve) => {
    dialog.value = {
      ...d,
      key: ++dialogKey,
      resolve: (values) => {
        dialog.value = undefined;
        resolve(values);
      },
    };
  });
}

/** State name and description; a name is the state's id, so it must be unique. */
function askState(existing?: GraphState) {
  return ask({
    title: t(existing ? "STATE_MACHINE.DIALOGS.STATE_EDIT" : "STATE_MACHINE.DIALOGS.STATE_ADD"),
    fields: [
      { key: "name", label: t("STATE_MACHINE.DIALOGS.NAME"), required: true },
      { key: "description", label: t("STATE_MACHINE.DIALOGS.DESCRIPTION"), multiline: true },
    ],
    initial: { name: existing?.name ?? "", description: existing?.description ?? "" },
    validate: ({ name }) =>
      graph.states.some((s) => s !== existing && s.id === name)
        ? { name: t("STATE_MACHINE.EDITOR.STATE_EXISTS", { name }) }
        : undefined,
  }) as Promise<{ name: string; description: string } | undefined>;
}

function askTransition(existing?: GraphTransition) {
  return ask({
    title: t(existing ? "STATE_MACHINE.DIALOGS.TRANSITION_EDIT" : "STATE_MACHINE.DIALOGS.TRANSITION_NEW"),
    fields: [
      { key: "trigger", label: t("STATE_MACHINE.DIALOGS.TRIGGER"), required: true },
      { key: "description", label: t("STATE_MACHINE.DIALOGS.DESCRIPTION"), multiline: true },
    ],
    initial: { trigger: existing?.trigger ?? "", description: existing?.description ?? "" },
  }) as Promise<{ trigger: string; description: string } | undefined>;
}

/** The localizations (by language) or attributes (by key) of a state or a trigger. */
async function editTexts(kind: "localizations" | "attributes", item: string) {
  if (!model.value) return;
  const keys = (kind === "localizations" ? settings.value.languages : settings.value.attributeKeys) ?? [];
  const values = await ask({
    title: t(kind === "localizations" ? "STATE_MACHINE.DIALOGS.LOCALIZATION" : "STATE_MACHINE.DIALOGS.ATTRIBUTES", { item }),
    fields: keys.map((key) => ({ key, label: key })),
    initial: itemValues(model.value, kind, item, keys),
  });
  if (values) applyItemValues(model.value, kind, item, values);
}

async function newState(position: Point) {
  const data = await askState();
  if (!data) return undefined;
  const state = addState(graph, data, position);
  commit();
  return state;
}

/** Below the last state, as legacy did. */
function addStateFromToolbar() {
  const last = graph.states[graph.states.length - 1];
  return newState(last ? { x: last.position.x, y: last.position.y + 100 + STATE_HEIGHT } : { x: 100, y: 50 });
}

async function editState(state: GraphState) {
  if (!canEdit.value) return;
  const data = await askState(state);
  if (!data || !model.value) return;
  renameItem(model.value, state.name, data.name);
  state.id = data.name;
  state.name = data.name;
  state.description = data.description;
  commit();
}

async function editTransition(transition: GraphTransition) {
  if (!canEdit.value) return;
  const data = await askTransition(transition);
  if (!data || !model.value) return;
  renameItem(model.value, transition.trigger, data.trigger);
  transition.trigger = data.trigger;
  transition.description = data.description;
  commit();
}

function setOutcome(state: GraphState, position: TogglePosition) {
  if (!canEdit.value) return;
  state.isSuccess = position === "left";
  state.isFailed = position === "right";
  commit();
}

// conditions: for types that registered a condition tree, and for transitions that already have one
const conditionsEnabled = computed(() => hasConditionTree(entityTypes.value, model.value?.entity.entityType));
function editCondition(transition: GraphTransition) {
  const conditionOptions: ConditionOptions = {
    entityType: model.value?.entity.entityType ?? "",
    transitionId: transition.id,
    trigger: transition.trigger,
    condition: clone(transition.condition),
  };
  openBlade({ name: "StateMachineCondition", options: conditionOptions });
}

exposeToChildren({
  applyCondition({ transitionId, condition }: { transitionId: string; condition?: ConditionNode }) {
    const transition = graph.transitions.find((x) => x.id === transitionId);
    if (!transition) return;
    transition.condition = condition;
    commit();
  },
});

// context menus (legacy right-click menus of state-node and state-transitions)
const menu = shallowRef<{ items: MenuItem[]; x: number; y: number }>();
function openMenu(e: MouseEvent | KeyboardEvent, items: (MenuItem & { allowed: boolean })[], at?: Point) {
  const allowed = items.filter((x) => x.allowed);
  if (!allowed.length) return;
  const point = at ?? ("clientX" in e ? pointOf(e) : { x: 0, y: 0 });
  menu.value = { items: allowed, ...point };
}
function closeMenu() {
  menu.value = undefined;
}

function stateMenu(e: MouseEvent | KeyboardEvent, state: GraphState) {
  // from the keyboard: at the state itself
  const at = "clientX" in e && e.clientX ? undefined : { x: state.position.x + 20, y: state.position.y + 40 };
  openMenu(
    e,
    [
      { id: "edit", label: t("STATE_MACHINE.MENU_STATE.EDIT_STATE"), icon: "lucide-pencil", allowed: canEdit.value, action: () => editState(state) },
      {
        id: "attributes",
        label: t("STATE_MACHINE.MENU_STATE.EDIT_ATTRIBUTES"),
        icon: "lucide-settings",
        allowed: canEdit.value,
        action: () => editTexts("attributes", state.name),
      },
      {
        id: "localization",
        label: t("STATE_MACHINE.MENU_STATE.EDIT_LOCALIZATION"),
        icon: "lucide-globe",
        allowed: canLocalize.value,
        action: () => editTexts("localizations", state.name),
      },
      {
        id: "delete",
        label: t("STATE_MACHINE.MENU_STATE.DELETE_STATE"),
        icon: "lucide-trash-2",
        allowed: canEdit.value,
        action: () => {
          removeState(graph, state);
          commit();
        },
      },
    ],
    at,
  );
}

function transitionMenu(e: MouseEvent, transition: GraphTransition) {
  openMenu(e, [
    {
      id: "edit",
      label: t("STATE_MACHINE.MENU_TRANSITION.EDIT_TRANSITION"),
      icon: "lucide-pencil",
      allowed: canEdit.value,
      action: () => editTransition(transition),
    },
    {
      id: "attributes",
      label: t("STATE_MACHINE.MENU_TRANSITION.EDIT_ATTRIBUTES"),
      icon: "lucide-settings",
      allowed: canEdit.value,
      action: () => editTexts("attributes", transition.trigger),
    },
    {
      id: "localization",
      label: t("STATE_MACHINE.MENU_TRANSITION.EDIT_LOCALIZATION"),
      icon: "lucide-globe",
      allowed: canLocalize.value,
      action: () => editTexts("localizations", transition.trigger),
    },
    {
      id: "condition",
      label: t("STATE_MACHINE.MENU_TRANSITION.EDIT_CONDITION"),
      icon: "lucide-filter",
      allowed: canEdit.value && (conditionsEnabled.value || !!transition.condition),
      action: () => editCondition(transition),
    },
    {
      id: "delete",
      label: t("STATE_MACHINE.MENU_TRANSITION.DELETE_TRANSITION"),
      icon: "lucide-trash-2",
      allowed: canEdit.value,
      action: () => {
        removeTransition(graph, transition);
        commit();
      },
    },
    {
      id: "insert",
      label: t("STATE_MACHINE.MENU_TRANSITION.INSERT_STATE"),
      icon: "lucide-plus",
      allowed: canEdit.value,
      action: async () => {
        const state = await askState();
        if (!state) return;
        const next = await askTransition();
        if (!next) return;
        insertState(graph, transition, state, next);
        commit();
      },
    },
  ]);
}

// dragging a state (legacy startDrag)
function startDrag(e: PointerEvent, state: GraphState) {
  if (!canEdit.value || e.button !== 0) return;
  let last = { x: e.clientX, y: e.clientY };
  let moved = false;
  const onMove = (ev: PointerEvent) => {
    state.position = {
      x: Math.max(0, state.position.x + ev.clientX - last.x),
      y: Math.max(0, state.position.y + ev.clientY - last.y),
    };
    last = { x: ev.clientX, y: ev.clientY };
    moved = true;
  };
  const onUp = () => {
    window.removeEventListener("pointermove", onMove);
    window.removeEventListener("pointerup", onUp);
    if (moved) commit();
  };
  window.addEventListener("pointermove", onMove);
  window.addEventListener("pointerup", onUp);
}

// connecting two states (legacy startTransition): drop on a state, or on empty space for "Add new State"
const connecting = shallowRef<{ from: GraphState; path: string }>();
function startConnect(e: PointerEvent, from: GraphState) {
  if (!canEdit.value || e.button !== 0) return;
  e.preventDefault();
  const draw = (p: Point) =>
    (connecting.value = { from, path: curve(from.position.x + STATE_WIDTH / 2, from.position.y + STATE_HEIGHT, p.x, p.y).d });
  draw(pointOf(e));
  const onMove = (ev: PointerEvent) => draw(pointOf(ev));
  const onUp = async (ev: PointerEvent) => {
    window.removeEventListener("pointermove", onMove);
    window.removeEventListener("pointerup", onUp);
    const point = pointOf(ev);
    const target = stateAt(point);
    if (target) {
      connecting.value = undefined;
      if (target !== from) await connect(from, target);
      return;
    }
    openMenu(ev, [
      {
        id: "add",
        label: t("STATE_MACHINE.MENU_STATE.ADD_NEW_STATE"),
        icon: "lucide-plus",
        allowed: true,
        action: async () => {
          connecting.value = undefined;
          const state = await newState({ x: Math.max(0, point.x - STATE_WIDTH / 2), y: point.y });
          if (state) await connect(from, state);
        },
      },
    ]);
  };
  window.addEventListener("pointermove", onMove);
  window.addEventListener("pointerup", onUp);
}

// the line drawn while connecting goes away with its menu
watch(menu, (value) => {
  if (!value && connecting.value) setTimeout(() => (connecting.value = undefined));
});

async function connect(from: GraphState, to: GraphState) {
  const data = await askTransition();
  connecting.value = undefined;
  if (!data) return;
  addTransition(graph, from, to, data);
  commit();
}

// JSON mode: the statesGraph as text; only valid JSON reaches the model
function toJsonMode() {
  closeMenu();
  const graphJson = model.value?.entity.statesGraph;
  try {
    jsonText.value = JSON.stringify(JSON.parse(graphJson || "[]"), null, 2);
  } catch {
    jsonText.value = graphJson ?? "";
  }
  jsonInvalid.value = false;
  mode.value = "json";
}

function onJsonInput() {
  try {
    const parsed = JSON.parse(jsonText.value);
    if (!Array.isArray(parsed)) throw new Error("not an array");
    jsonInvalid.value = false;
    if (model.value) model.value.entity.statesGraph = JSON.stringify(parsed);
  } catch {
    jsonInvalid.value = true;
  }
}

function toVisualMode() {
  mode.value = "visual";
  jsonInvalid.value = false;
  readGraph();
  updateStateFlags(graph);
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "addState",
    title: t("STATE_MACHINE.TOOLBAR.ADD_STATE"),
    icon: "lucide-plus",
    permissions: "statemachine:update",
    disabled: mode.value !== "visual",
    clickHandler: addStateFromToolbar,
  },
  {
    id: "visualMode",
    title: t("STATE_MACHINE.TOOLBAR.VISUAL_MODE"),
    icon: "lucide-workflow",
    permissions: "statemachine:update",
    isVisible: mode.value === "json",
    clickHandler: toVisualMode,
  },
  {
    id: "jsonMode",
    title: t("STATE_MACHINE.TOOLBAR.JSON_MODE"),
    icon: "lucide-code",
    permissions: "statemachine:update",
    isVisible: mode.value === "visual",
    clickHandler: toJsonMode,
  },
]);

onMounted(async () => {
  // opened from a URL (no details blade to edit): nothing to show
  if (!model.value) {
    closeSelf();
    return;
  }
  readGraph();
  const [loadedSettings, types] = await Promise.all([api.stateMachine.getSettings().catch(() => ({})), loadEntityTypes()]);
  settings.value = loadedSettings;
  entityTypes.value = types;
});

onBeforeUnmount(() => resizeObserver?.disconnect());
</script>

<style lang="scss" scoped>
.sm-canvas {
  position: relative;
  height: 100%;
  overflow: auto;
  background-color: var(--neutrals-50);
  background-image: radial-gradient(var(--neutrals-200) 1px, transparent 1px);
  background-size: 20px 20px;

  &__inner {
    position: relative;
  }

  &__lines {
    position: absolute;
    inset: 0;
  }

  &__arrow {
    fill: var(--neutrals-500);
  }
}

.sm-transition {
  cursor: context-menu;

  &__hit {
    fill: none;
    stroke: transparent;
    stroke-width: 12px;
  }

  &__line {
    fill: none;
    stroke: var(--neutrals-500);
    stroke-width: 1.5px;
    pointer-events: none;

    &--temp {
      stroke-dasharray: 6 4;
    }
  }

  &__label {
    fill: var(--neutrals-800);
    font-size: 12px;
    text-anchor: middle;
    paint-order: stroke;
    stroke: var(--neutrals-50);
    stroke-width: 4px;
  }

  &--hovered &__line {
    stroke: var(--primary-500);
    stroke-width: 2.5px;
  }
}

.sm-state {
  position: absolute;
  display: flex;
  flex-direction: column;
  width: 150px;
  height: 100px;
  border: 1px solid var(--neutrals-300);
  border-radius: 6px;
  background: var(--additional-50);
  box-shadow: 2px 2px 5px rgb(0 0 0 / 20%);
  user-select: none;

  &--editable {
    cursor: move;
  }

  &--hovered,
  &:focus-visible {
    outline: none;
    border: 2px solid var(--primary-500);
    box-shadow: 5px 5px 6px rgb(0 0 0 / 20%);
  }

  &__header {
    overflow: hidden;
    padding: 6px 8px;
    border-radius: 5px 5px 0 0;
    background: var(--primary-500);
    color: #fff;
    font-weight: 600;
    font-size: 13px;
    text-align: center;
    text-overflow: ellipsis;
    white-space: nowrap;

    &--success {
      background: var(--success-500);
    }

    &--failed {
      background: var(--danger-500);
    }
  }

  &__description {
    flex: 1;
    overflow: hidden;
    padding: 4px 8px;
    color: var(--neutrals-600);
    font-size: 11px;
    text-align: center;
  }

  &__toggle {
    padding-bottom: 4px;
  }

  &__connector {
    position: absolute;
    bottom: -7px;
    left: 50%;
    width: 12px;
    height: 12px;
    margin-left: -6px;
    border: 2px solid var(--additional-50);
    border-radius: 50%;
    background: var(--neutrals-500);
    cursor: crosshair;

    &:hover {
      background: var(--neutrals-700);
    }
  }
}

.sm-json {
  display: flex;
  flex-direction: column;
  gap: 4px;
  height: 100%;
  padding: 10px;

  &__text {
    flex: 1;
    width: 100%;
    padding: 10px;
    border: 1px solid var(--neutrals-300);
    border-radius: 4px;
    background: var(--additional-50);
    color: var(--neutrals-800);
    font-family: monospace;
    font-size: 14px;
    resize: none;

    &--invalid {
      border-color: var(--danger-500);
    }

    &:focus-visible {
      outline: 2px solid var(--primary-500);
    }
  }

  &__error {
    color: var(--danger-500);
  }
}
</style>
