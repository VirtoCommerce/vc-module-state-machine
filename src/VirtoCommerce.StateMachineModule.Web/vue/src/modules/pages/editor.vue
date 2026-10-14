<template>
  <VcBlade
    :title="$t('STATEMACHINE.PAGES.EDITOR.TITLE')"
    :subtitle="draft.model.value.entity.name"
    :toolbar-items="bladeToolbar"
    width="70%"
  >
    <div class="sm-editor">
      <div
        v-show="mode === 'visual'"
        :ref="(el) => (workspace = el as HTMLElement)"
        class="sm-workspace"
        @contextmenu.prevent
        @pointerdown="closeMenu"
      >
        <svg
          class="sm-workspace__lines"
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
                class="sm-arrow"
                d="M0 0 L10 5 L0 10 z"
              />
            </marker>
          </defs>
          <g
            v-for="transition in graph.transitions"
            :key="transition.id"
            class="sm-transition"
            :class="{ 'sm-transition--hot': isHot(transition) }"
            :data-trigger="transition.trigger"
            @mouseenter="hovered = transition"
            @mouseleave="hovered = undefined"
            @contextmenu.prevent.stop="transitionMenu($event, transition)"
            @dblclick.stop="canEdit && editTransition(transition)"
          >
            <path
              class="sm-transition__hit"
              :d="pathOf(transition).d"
            />
            <path
              class="sm-transition__path"
              :d="pathOf(transition).d"
              marker-end="url(#sm-arrow)"
            />
            <text
              class="sm-transition__label"
              :x="pathOf(transition).label.x"
              :y="pathOf(transition).label.y + 4"
            >
              {{ transition.trigger }}
            </text>
          </g>
          <path
            v-if="tempLine"
            class="sm-transition__path sm-transition__path--temp"
            :d="tempLine"
          />
        </svg>

        <div
          v-for="state in graph.states"
          :key="state.id"
          class="sm-state"
          :class="{ 'sm-state--hot': isHot(state) }"
          :data-state="state.name"
          :style="{
            left: state.position.x + 'px',
            top: state.position.y + 'px',
            width: STATE_WIDTH + 'px',
            height: STATE_HEIGHT + 'px',
          }"
          @mouseenter="hovered = state"
          @mouseleave="hovered = undefined"
          @contextmenu.prevent.stop="stateMenu($event, state)"
        >
          <div
            class="sm-state__header"
            :class="{ 'sm-state__header--success': state.isSuccess, 'sm-state__header--failed': state.isFailed }"
            @pointerdown="startDrag($event, state)"
            @dblclick="canEdit && editState(state)"
          >
            {{ state.name }}
          </div>
          <div
            class="sm-state__description"
            @pointerdown="startDrag($event, state)"
            @dblclick="canEdit && editState(state)"
          >
            {{ state.description || $t("STATEMACHINE.PAGES.EDITOR.NO_DESCRIPTION") }}
          </div>
          <!-- the AngularJS three-position toggle: a final state succeeds, fails or neither -->
          <div
            v-if="state.isFinal"
            class="sm-outcome"
          >
            <span>{{ $t("STATEMACHINE.PAGES.EDITOR.SUCCESS") }}</span>
            <div
              class="sm-outcome__toggle"
              role="radiogroup"
            >
              <button
                v-for="option in outcomes"
                :key="option.id"
                type="button"
                role="radio"
                class="sm-outcome__option"
                :class="`sm-outcome__option--${option.id}`"
                :aria-checked="outcomeOf(state) === option.id"
                :aria-label="$t(option.label)"
                :title="$t(option.label)"
                :disabled="!canEdit"
                @click="setOutcome(state, option.id)"
              >
                <VcIcon
                  v-if="option.icon"
                  :icon="option.icon"
                  size="xs"
                />
              </button>
            </div>
            <span>{{ $t("STATEMACHINE.PAGES.EDITOR.FAIL") }}</span>
          </div>
          <div
            v-if="canEdit"
            class="sm-state__connector"
            @pointerdown.stop="startTransition($event, state)"
          />
        </div>

        <ul
          v-if="menu"
          class="sm-menu"
          role="menu"
          :style="{ left: menu.x + 'px', top: menu.y + 'px' }"
          @pointerdown.stop
        >
          <li
            v-for="item in menu.items"
            :key="item.label"
            role="none"
          >
            <button
              type="button"
              role="menuitem"
              class="sm-menu__item"
              @click="run(item.action)"
            >
              <VcIcon
                :icon="item.icon"
                size="s"
              />
              {{ item.label }}
            </button>
          </li>
        </ul>
      </div>

      <div
        v-if="mode === 'json'"
        class="sm-json"
      >
        <textarea
          v-model="jsonText"
          class="sm-json__editor"
          :class="{ 'sm-json__editor--invalid': jsonInvalid }"
          spellcheck="false"
          :readonly="!canEdit"
          @input="onJsonInput"
        />
        <VcHint
          v-if="jsonInvalid"
          class="sm-json__error"
          >{{ $t("STATEMACHINE.PAGES.EDITOR.INVALID_JSON") }}</VcHint
        >
      </div>
    </div>

    <FormDialog
      v-if="dialog"
      :title="dialog.title"
      :fields="dialog.fields"
      :values="dialog.values"
      :rows="dialog.rows"
      :error="dialog.error?.()"
      @ok="confirmDialog"
      @cancel="cancelDialog"
    />
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcHint, VcIcon } from "@vc-shell/framework/ui";
import { computed, nextTick, onMounted, onUnmounted, reactive, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import FormDialog, { type DialogField } from "../components/FormDialog.vue";
import type { Draft } from "../draft";
import {
  STATE_HEIGHT,
  STATE_WIDTH,
  addTransition,
  bounds,
  curve,
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

// The AngularJS visual editor: states are boxes you drag, transitions are curves you draw from a state's connector
// to another state (or to empty space, to add a state there); right-click (long-press) a state or a transition for
// its menu, double-click to edit it. Every change goes straight into the definition open in the details blade.
defineBlade({
  url: "/state-machine-editor",
  name: "StateMachineEditor",
  permissions: ["statemachine:read"],
  // it edits the details blade's draft (options.draft), which a reload doesn't bring back
  routable: false,
});

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<{ draft: () => Draft }>();
const { hasAccess } = usePermissions();
const draft = ((): Draft => {
  const opened = options.value?.draft?.();
  if (!opened) throw new Error("[statemachine] the editor opens from a definition's details blade");
  return opened;
})();
const canEdit = computed(() => hasAccess("statemachine:update"));
const canLocalize = computed(() => hasAccess("statemachine:localize"));

const graph = reactive<Graph>({ states: [], transitions: [] });
const workspace = shallowRef<HTMLElement>();
const mode = ref<"visual" | "json">("visual");
const hovered = ref<GraphState | GraphTransition>();
const tempLine = ref<string>();
let written: string | undefined;

// ---------------- graph <-> definition ----------------

function rebuild() {
  const statesGraph = draft.model.value.entity.statesGraph;
  written = statesGraph;
  let parsed: ReturnType<typeof parseGraph>;
  try {
    parsed = parseGraph(statesGraph);
  } catch {
    return; // not JSON: the JSON mode shows it
  }
  // states saved without positions are laid out, not written back until something changes
  if (!parsed.positioned) layoutGraph(parsed, workspace.value?.clientWidth || 800);
  graph.states = parsed.states;
  graph.transitions = parsed.transitions;
}

function commit() {
  updateStateFlags(graph);
  written = serializeGraph(graph);
  draft.model.value.entity.statesGraph = written;
}

// a reset or reload in the details blade
watch(
  () => draft.model.value.entity.statesGraph,
  (value) => value !== written && mode.value === "visual" && rebuild(),
);

const size = computed(() => {
  const b = bounds(graph.states, 100);
  return { width: b.maxX, height: b.maxY };
});

// each curve once per graph change, not on each of the four template reads per transition
const curveOf = (transition: GraphTransition) => transitionPath(transition.from.position, transition.to.position);
const paths = computed(() => new Map(graph.transitions.map((x) => [x, curveOf(x)])));
const pathOf = (transition: GraphTransition) => paths.value.get(transition) ?? curveOf(transition);

const isHot = (x: GraphState | GraphTransition) => {
  const h = hovered.value;
  if (!h) return false;
  if ("trigger" in x) return h === x || x.from === h || x.to === h;
  return h === x || ("trigger" in h && (h.from === x || h.to === x));
};

// ---------------- pointer: drag, draw ----------------

function point(e: PointerEvent | MouseEvent): Point {
  const el = workspace.value!;
  const rect = el.getBoundingClientRect();
  return { x: e.clientX - rect.left + el.scrollLeft, y: e.clientY - rect.top + el.scrollTop };
}

let stopPointer: (() => void) | undefined;
function track(move: (e: PointerEvent) => void, up: (e: PointerEvent) => void) {
  const onUp = (e: PointerEvent) => {
    stopPointer?.();
    up(e);
  };
  window.addEventListener("pointermove", move);
  window.addEventListener("pointerup", onUp);
  stopPointer = () => {
    window.removeEventListener("pointermove", move);
    window.removeEventListener("pointerup", onUp);
    stopPointer = undefined;
  };
}
onUnmounted(() => stopPointer?.());

function startDrag(e: PointerEvent, state: GraphState) {
  if (!canEdit.value || e.button !== 0) return;
  e.preventDefault();
  let last = point(e);
  let moved = false;
  track(
    (m) => {
      const p = point(m);
      state.position = { x: Math.max(0, state.position.x + p.x - last.x), y: Math.max(0, state.position.y + p.y - last.y) };
      moved ||= p.x !== last.x || p.y !== last.y;
      last = p;
    },
    () => moved && commit(),
  );
}

function startTransition(e: PointerEvent, from: GraphState) {
  if (e.button !== 0) return;
  e.preventDefault();
  closeMenu();
  const startX = from.position.x + STATE_WIDTH / 2;
  const startY = from.position.y + STATE_HEIGHT;
  track(
    (m) => {
      const p = point(m);
      tempLine.value = curve(startX, startY, p.x, p.y).d;
    },
    (u) => {
      const p = point(u);
      const target = graph.states.find(
        (s) => p.x >= s.position.x && p.x <= s.position.x + STATE_WIDTH && p.y >= s.position.y && p.y <= s.position.y + STATE_HEIGHT,
      );
      if (target) {
        if (target === from) tempLine.value = undefined;
        else transitionDialog(undefined, (data) => commitWith(() => addTransition(graph, from, target, data)));
      } else if (tempLine.value) {
        // dropped on empty space: offer a new state there
        openMenu(p, [
          {
            label: t("STATEMACHINE.PAGES.EDITOR.STATE_MENU.ADD_NEW"),
            icon: "lucide-plus",
            action: () =>
              stateDialog(undefined, { x: Math.max(0, p.x - STATE_WIDTH / 2), y: p.y }, (state) =>
                transitionDialog(
                  undefined,
                  (data) => commitWith(() => addTransition(graph, from, state, data)),
                  () => removeState(graph, state),
                ),
              ),
          },
        ]);
      }
    },
  );
}

function commitWith(change: () => unknown) {
  change();
  tempLine.value = undefined;
  commit();
}

// ---------------- context menus ----------------

interface MenuItem {
  label: string;
  icon: string;
  action: () => void;
}
const menu = ref<{ x: number; y: number; items: MenuItem[] }>();

function openMenu(at: Point, items: MenuItem[]) {
  menu.value = items.length ? { ...at, items } : undefined;
  if (!menu.value) tempLine.value = undefined;
}

function closeMenu() {
  menu.value = undefined;
  if (!dialog.value) tempLine.value = undefined;
}

function run(action: () => void) {
  menu.value = undefined;
  action();
}

// the state and the transition menu: edit, attributes, localization, delete, plus the item's own entries
function itemMenu(
  e: MouseEvent,
  prefix: "STATE_MENU" | "TRANSITION_MENU",
  name: string,
  edit: () => void,
  remove: () => void,
  extra: (MenuItem & { show: boolean })[] = [],
) {
  openMenu(
    point(e),
    [
      { show: canEdit.value, label: "EDIT", icon: "lucide-pencil", action: edit },
      { show: canEdit.value, label: "ATTRIBUTES", icon: "lucide-settings-2", action: () => attributesDialog(name) },
      { show: canLocalize.value, label: "LOCALIZATION", icon: "lucide-globe", action: () => localizationDialog(name) },
      { show: canEdit.value, label: "DELETE", icon: "lucide-trash-2", action: () => commitWith(remove) },
      ...extra,
    ]
      .filter((x) => x.show)
      .map((x) => ({ ...x, label: t(`STATEMACHINE.PAGES.EDITOR.${prefix}.${x.label}`) })),
  );
}

function stateMenu(e: MouseEvent, state: GraphState) {
  itemMenu(e, "STATE_MENU", state.name, () => editState(state), () => removeState(graph, state));
}

// ponytail: legacy also offered "Edit condition" here when another module registered a condition tree for the
// entity type; none does, so it is left out (add a condition blade when one registers).
function transitionMenu(e: MouseEvent, transition: GraphTransition) {
  itemMenu(e, "TRANSITION_MENU", transition.trigger, () => editTransition(transition), () => removeTransition(graph, transition), [
    { show: canEdit.value, label: "INSERT_STATE", icon: "lucide-plus", action: () => insertState(transition) },
  ]);
}

function insertState(transition: GraphTransition) {
  const { from, to } = transition;
  const at = { x: (from.position.x + to.position.x) / 2, y: (from.position.y + to.position.y) / 2 };
  stateDialog(undefined, at, (state) =>
    transitionDialog(
      undefined,
      (data) =>
        commitWith(() => {
          transition.to = state;
          addTransition(graph, state, to, data);
        }),
      () => removeState(graph, state),
    ),
  );
}

// ---------------- dialogs ----------------

interface Dialog {
  title: string;
  fields?: DialogField[];
  values?: Record<string, string>;
  rows?: { key: string; value?: string }[];
  error?: () => string | undefined;
  ok: () => void;
  cancel?: () => void;
}
const dialog = ref<Dialog>();

function confirmDialog() {
  const current = dialog.value!;
  dialog.value = undefined;
  current.ok();
}

function cancelDialog() {
  const current = dialog.value;
  dialog.value = undefined;
  tempLine.value = undefined;
  current?.cancel?.();
}

const nameField = (): DialogField[] => [
  { name: "name", label: t("STATEMACHINE.MODALS.NAME"), required: true },
  { name: "description", label: t("STATEMACHINE.MODALS.DESCRIPTION"), multiline: true },
];

/** Adds a state at `at`, or edits `existing`; the name is the state's id, so it has to be unique. */
function stateDialog(existing: GraphState | undefined, at: Point | undefined, done?: (state: GraphState) => void) {
  const values = reactive({ name: existing?.name ?? "", description: existing?.description ?? "" });
  dialog.value = {
    title: t(existing ? "STATEMACHINE.MODALS.STATE_EDIT" : "STATEMACHINE.MODALS.STATE_ADD"),
    fields: nameField(),
    values,
    error: () =>
      graph.states.some((s) => s !== existing && s.name === values.name.trim()) ? t("STATEMACHINE.MODALS.STATE_EXISTS") : undefined,
    ok: () => {
      const name = values.name.trim();
      if (existing) {
        renameTexts(existing.name, name);
        Object.assign(existing, { id: name, name, description: values.description });
        commit();
        return;
      }
      const state: GraphState = {
        id: name,
        name,
        description: values.description,
        isInitial: true,
        isFinal: true,
        isSuccess: false,
        isFailed: false,
        position: at!,
        transitions: [],
      };
      graph.states.push(state);
      // the state proxy, so that what the callback links up is what the graph holds
      const added = graph.states[graph.states.length - 1];
      if (done) done(added);
      else commit();
    },
  };
}

const editState = (state: GraphState) => stateDialog(state, undefined);

/** `cancel` undoes what led here: the state just added for the transition goes again. */
function transitionDialog(
  existing: GraphTransition | undefined,
  done: (data: { trigger: string; description: string }) => void,
  cancel?: () => void,
) {
  const values = reactive({ trigger: existing?.trigger ?? "", description: existing?.description ?? "" });
  dialog.value = {
    title: t(existing ? "STATEMACHINE.MODALS.TRANSITION_EDIT" : "STATEMACHINE.MODALS.TRANSITION_NEW"),
    fields: [
      { name: "trigger", label: t("STATEMACHINE.MODALS.TRIGGER"), required: true },
      { name: "description", label: t("STATEMACHINE.MODALS.DESCRIPTION"), multiline: true },
    ],
    values,
    ok: () => done({ trigger: values.trigger.trim(), description: values.description.trim() }),
    cancel,
  };
}

function editTransition(transition: GraphTransition) {
  transitionDialog(transition, (data) => {
    renameTexts(transition.trigger, data.trigger);
    Object.assign(transition, data);
    commit();
  });
}

// Localizations and attributes belong to a state or transition by its name / trigger.
function renameTexts(from: string, to: string) {
  if (from === to) return;
  const { localizations, attributes } = draft.model.value;
  [...localizations, ...attributes].filter((x) => x.item === from).forEach((x) => (x.item = to));
}

/** One input per language (localization) or attribute key; empty inputs leave a stored value as it is. */
function textsDialog(kind: "localization" | "attributes", item: string) {
  const isLocalization = kind === "localization";
  const keys = (isLocalization ? draft.settings.value.languages : draft.settings.value.attributeKeys) ?? [];
  const list = isLocalization ? draft.model.value.localizations : draft.model.value.attributes;
  const keyOf = (x: (typeof list)[number]) => ("locale" in x ? x.locale : (x as { attributeKey?: string }).attributeKey);
  const rows = reactive(keys.map((key) => ({ key, value: list.find((x) => x.item === item && keyOf(x) === key)?.value })));
  dialog.value = {
    title: t(isLocalization ? "STATEMACHINE.MODALS.LOCALIZATION" : "STATEMACHINE.MODALS.ATTRIBUTES", { item }),
    rows,
    ok: () => {
      const definitionId = draft.model.value.entity.id;
      for (const row of rows.filter((r) => r.value)) {
        const found = list.find((x) => x.item === item && keyOf(x) === row.key);
        if (found) found.value = row.value;
        else list.push(isLocalization ? { definitionId, item, locale: row.key, value: row.value } : { definitionId, item, attributeKey: row.key, value: row.value });
      }
    },
  };
}
const localizationDialog = (item: string) => textsDialog("localization", item);
const attributesDialog = (item: string) => textsDialog("attributes", item);

// ---------------- success / fail ----------------

const outcomes = [
  { id: "success", label: "STATEMACHINE.PAGES.EDITOR.SUCCESS", icon: "lucide-check" },
  { id: "none", label: "STATEMACHINE.PAGES.EDITOR.NEUTRAL", icon: "" },
  { id: "failed", label: "STATEMACHINE.PAGES.EDITOR.FAIL", icon: "lucide-ban" },
] as const;
const outcomeOf = (s: GraphState) => (s.isSuccess ? "success" : s.isFailed ? "failed" : "none");
function setOutcome(state: GraphState, outcome: (typeof outcomes)[number]["id"]) {
  state.isSuccess = outcome === "success";
  state.isFailed = outcome === "failed";
  commit();
}

// ---------------- JSON mode ----------------

const jsonText = ref("");
const jsonInvalid = ref(false);

function onJsonInput() {
  try {
    const parsed = JSON.parse(jsonText.value);
    jsonInvalid.value = false;
    written = JSON.stringify(parsed);
    draft.model.value.entity.statesGraph = written;
  } catch {
    jsonInvalid.value = true;
  }
}

function toJson() {
  closeMenu();
  jsonText.value = JSON.stringify(JSON.parse(draft.model.value.entity.statesGraph || "[]"), null, 2);
  jsonInvalid.value = false;
  mode.value = "json";
}

async function toVisual() {
  mode.value = "visual";
  await nextTick();
  rebuild();
}

function addState() {
  const last = graph.states[graph.states.length - 1];
  stateDialog(undefined, last ? { x: last.position.x, y: last.position.y + 200 } : { x: 100, y: 50 });
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "addState",
    title: t("STATEMACHINE.PAGES.EDITOR.TOOLBAR.ADD_STATE"),
    icon: "lucide-plus",
    permissions: "statemachine:update",
    disabled: computed(() => mode.value !== "visual"),
    clickHandler: addState,
  },
  {
    id: "visualMode",
    title: t("STATEMACHINE.PAGES.EDITOR.TOOLBAR.VISUAL_MODE"),
    icon: "lucide-workflow",
    disabled: computed(() => mode.value === "visual"),
    clickHandler: toVisual,
  },
  {
    id: "jsonMode",
    title: t("STATEMACHINE.PAGES.EDITOR.TOOLBAR.JSON_MODE"),
    icon: "lucide-code",
    disabled: computed(() => mode.value === "json"),
    clickHandler: toJson,
  },
]);

const onKey = (e: KeyboardEvent) => e.key === "Escape" && closeMenu();
onMounted(async () => {
  window.addEventListener("keydown", onKey);
  await nextTick();
  rebuild();
});
onUnmounted(() => window.removeEventListener("keydown", onKey));
</script>

<style lang="scss">
.sm-editor {
  position: relative;
  height: 100%;
  min-height: 400px;
}

.sm-workspace {
  position: absolute;
  inset: 0;
  overflow: auto;
  background-color: var(--color-neutral-50);
  background-image: radial-gradient(var(--color-neutral-200) 1px, transparent 1px);
  background-size: 20px 20px;
  touch-action: none;
  user-select: none;

  &__lines {
    position: absolute;
    top: 0;
    left: 0;
    min-width: 100%;
    min-height: 100%;
  }
}

.sm-arrow {
  fill: var(--color-neutral-500);
}

.sm-transition {
  cursor: context-menu;

  &__hit {
    fill: none;
    stroke: transparent;
    stroke-width: 12;
  }

  &__path {
    fill: none;
    stroke: var(--color-neutral-500);
    stroke-width: 1.5;
    pointer-events: none;

    &--temp {
      stroke-dasharray: 6 4;
      stroke: var(--color-primary-500);
    }
  }

  &__label {
    font-size: 12px;
    text-anchor: middle;
    fill: var(--color-neutral-800);
    paint-order: stroke;
    stroke: var(--color-neutral-50);
    stroke-width: 4px;
  }

  &--hot &__path {
    stroke: var(--color-primary-500);
    stroke-width: 2.5;
  }

  &--hot &__label {
    fill: var(--color-primary-700);
    font-weight: 600;
  }
}

.sm-state {
  position: absolute;
  display: flex;
  flex-direction: column;
  border: 1px solid var(--color-neutral-300);
  border-radius: 6px;
  background: var(--color-neutral-0);
  box-shadow: 0 1px 3px var(--color-neutral-200);
  font-size: 12px;

  &--hot {
    border-color: var(--color-primary-500);
    box-shadow: 0 2px 8px var(--color-neutral-300);
  }

  &__header {
    padding: 5px 8px;
    border-radius: 5px 5px 0 0;
    background: var(--color-secondary-500);
    color: var(--color-additional-50);
    font-weight: 600;
    text-align: center;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    cursor: move;

    &--success {
      background: var(--color-success-500);
    }

    &--failed {
      background: var(--color-danger-600);
    }
  }

  &__description {
    flex: 1;
    min-height: 0;
    padding: 4px 8px;
    overflow: hidden;
    color: var(--color-neutral-600);
    text-align: center;
    cursor: move;
  }

  &__connector {
    position: absolute;
    left: 50%;
    bottom: -7px;
    width: 14px;
    height: 14px;
    margin-left: -7px;
    border: 2px solid var(--color-neutral-0);
    border-radius: 50%;
    background: var(--color-neutral-500);
    cursor: crosshair;

    &:hover {
      background: var(--color-primary-500);
    }
  }
}

.sm-outcome {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 0 4px 9px;
  font-size: 10px;
  color: var(--color-neutral-600);

  &__toggle {
    display: flex;
    border: 1px solid var(--color-neutral-300);
    border-radius: 10px;
    overflow: hidden;
  }

  &__option {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 16px;
    padding: 0;
    border: 0;
    background: var(--color-neutral-0);
    color: var(--color-neutral-400);
    cursor: pointer;

    &:disabled {
      cursor: not-allowed;
      opacity: 0.6;
    }

    &[aria-checked="true"] {
      background: var(--color-neutral-300);
      color: var(--color-neutral-900);
    }

    &--success[aria-checked="true"] {
      background: var(--color-success-500);
      color: var(--color-additional-50);
    }

    &--failed[aria-checked="true"] {
      background: var(--color-danger-600);
      color: var(--color-additional-50);
    }
  }
}

.sm-menu {
  position: absolute;
  z-index: 10;
  min-width: 180px;
  margin: 0;
  padding: 4px 0;
  list-style: none;
  border: 1px solid var(--color-neutral-200);
  border-radius: 6px;
  background: var(--color-neutral-0);
  box-shadow: 0 4px 12px var(--color-neutral-300);

  &__item {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;
    padding: 7px 12px;
    border: 0;
    background: none;
    color: var(--color-neutral-800);
    font-size: 13px;
    text-align: left;
    cursor: pointer;

    &:hover {
      background: var(--color-primary-50);
    }
  }
}

.sm-json {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 12px;

  &__editor {
    flex: 1;
    width: 100%;
    padding: 10px;
    resize: none;
    border: 1px solid var(--color-neutral-300);
    border-radius: 6px;
    background: var(--color-neutral-0);
    color: var(--color-neutral-900);
    font-family: ui-monospace, Consolas, monospace;
    font-size: 13px;

    &--invalid {
      border-color: var(--color-danger-500);
    }
  }
}
</style>
