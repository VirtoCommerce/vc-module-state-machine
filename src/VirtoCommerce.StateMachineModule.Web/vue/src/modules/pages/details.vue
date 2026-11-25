<template>
  <VcBlade
    :title="param ? (original.name ?? '') : $t('STATE_MACHINE.DETAILS.TITLE_NEW')"
    :subtitle="param ? $t('STATE_MACHINE.DETAILS.SUBTITLE') : undefined"
    :toolbar-items="toolbar"
    :loading="loading"
    :modified="form.isModified.value"
    width="50%"
  >
    <VcContainer>
      <VcForm class="sm-details">
        <VcInput
          v-model="model.entity.name"
          :label="$t('STATE_MACHINE.DETAILS.NAME')"
          :placeholder="$t('STATE_MACHINE.DETAILS.NAME_PLACEHOLDER')"
          :disabled="readonly"
          maxlength="512"
          name="name"
          rules="required"
          required
        />
        <!-- fixed once the definition exists (legacy: editable only on a new one) -->
        <VcSelect
          v-model="model.entity.entityType"
          :label="$t('STATE_MACHINE.DETAILS.ENTITY_TYPE')"
          :placeholder="$t('STATE_MACHINE.DETAILS.ENTITY_TYPE_PLACEHOLDER')"
          :options="entityTypeOptions"
          :clearable="false"
          :disabled="!!param || readonly"
          name="entityType"
          rules="required"
          required
        />
        <VcSwitch
          :model-value="!!model.entity.isActive"
          :label="$t('STATE_MACHINE.DETAILS.IS_ACTIVE')"
          :disabled="readonly"
          @update:model-value="onActiveChanged"
        />
        <div class="sm-details__states">
          <VcLabel>{{ $t("STATE_MACHINE.DETAILS.STATES") }}</VcLabel>
          <div>
            <VcButton
              icon="lucide-workflow"
              variant="secondary"
              @click="openEditor"
            >
              {{ $t("STATE_MACHINE.DETAILS.OPEN_EDITOR") }}
            </VcButton>
          </div>
          <button
            v-if="preview"
            type="button"
            class="sm-details__preview"
            :title="$t('STATE_MACHINE.DETAILS.OPEN_EDITOR')"
            @click="openEditor"
          >
            <img
              :src="preview"
              :alt="$t('STATE_MACHINE.DETAILS.PREVIEW')"
            />
          </button>
          <VcHint v-else>{{ $t("STATE_MACHINE.DETAILS.NO_STATES") }}</VcHint>
        </div>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcButton, VcContainer, VcForm, VcHint, VcInput, VcLabel, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, clone, errorText } from "../api";
import { entityTypeName, loadEntityTypes, usedEntityTypes } from "../entityTypes";
import { graphSnapshot, graphSvg, normalizePositions, svgDataUrl } from "../graph";
import { exportDefinition } from "../transfer";
import type { DefinitionModel, EditorOptions } from "../types";
import type { StateMachineDefinition, StateMachineEntityType } from "../../api_client/virtocommerce.statemachine";

// legacy state-machine-detail
defineBlade({ url: "/state-machine", name: "StateMachineDetails", permissions: ["statemachine:read"] });

const { t } = useI18n({ useScope: "global" });
const { param: bladeParam, callParent, openBlade, closeChildren } = useBlade();
// a new definition, once saved, stays in this blade as the stored one. Replacing the blade instead races with the
// shell, which restores the param-less "new" blade from the URL.
const savedId = ref<string>();
const param = computed(() => bladeParam.value ?? savedId.value);
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();

const model = ref<DefinitionModel>({ entity: {}, localizations: [], attributes: [] });
// the saved definition: the title shows its name, not the one being typed
const original = ref<StateMachineDefinition>({});
// statesGraph as loaded: a changed graph gets a new preview image on save
let savedGraph: string | undefined;
// the other active definition of the type, to be disabled on save (legacy anotherActiveStateMachine)
const deactivate = ref<StateMachineDefinition>();
const loading = ref(false);
const readonly = computed(() => !hasAccess("statemachine:update"));
const form = useBladeForm({ data: model, closeConfirmMessage: computed(() => t("STATE_MACHINE.DETAILS.CLOSE_CONFIRM")) });

const entityTypes = ref<StateMachineEntityType[]>([]);
const usedTypes = ref<string[]>([]);
loadEntityTypes().then((x) => (entityTypes.value = x));
usedEntityTypes()
  .then((x) => (usedTypes.value = x))
  .catch(() => undefined);
// registered types, plus types existing definitions use and the definition's own: no module registers its types on
// the server yet, and a definition of an unregistered type must still show and save it
const entityTypeOptions = computed(() => {
  const ids = [...entityTypes.value.map((x) => x.id!), ...usedTypes.value, model.value.entity.entityType].filter(
    (x): x is string => !!x,
  );
  const unique = ids.filter((id, i) => ids.findIndex((x) => x.toLowerCase() === id.toLowerCase()) === i);
  return unique.map((id) => ({ id, title: entityTypeName(entityTypes.value, id)! }));
});

// the server rejects a definition without states (StateMachineValidator), so Save waits for the first one
const hasStates = computed(() => {
  try {
    const states = JSON.parse(model.value.entity.statesGraph || "[]");
    return Array.isArray(states) && states.length > 0;
  } catch {
    return false;
  }
});

// the graph as it is now (also while the editor changes it); the stored capture until the graph is drawn
const preview = computed(() => {
  const svg = graphSvg(model.value.entity.statesGraph);
  return svg ? svgDataUrl(svg) : model.value.entity.statesCapture;
});

async function load() {
  loading.value = true;
  try {
    if (param.value) {
      const [entity, localizations, attributes] = await Promise.all([
        api.stateMachine.getStateMachineById(param.value),
        api.stateMachine.searchLocalization({ definitionId: param.value, take: 10000 }),
        api.stateMachine.searchAttribute({ definitionId: param.value, take: 10000 }),
      ]);
      // definitions saved before the graph was stored keep only their states (legacy initializeBlade)
      if (!entity.statesGraph && entity.states?.length) entity.statesGraph = JSON.stringify(entity.states, null, 2);
      model.value = { entity, localizations: localizations.results ?? [], attributes: attributes.results ?? [] };
    } else {
      model.value = { entity: { isActive: false }, localizations: [], attributes: [] };
    }
    original.value = clone(model.value.entity);
    savedGraph = model.value.entity.statesGraph;
    deactivate.value = undefined;
  } catch (e) {
    showError(errorText(e));
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

// legacy onIsActiveChanged: one active state machine per entity type
async function onActiveChanged(value?: boolean) {
  model.value.entity.isActive = !!value;
  deactivate.value = undefined;
  if (!value || !model.value.entity.entityType) return;
  const res = await api.stateMachine.search({ objectTypes: [model.value.entity.entityType], isActive: true, take: 10 });
  const other = res.results?.find((x) => x.isActive && x.id !== model.value.entity.id);
  if (!other) return;
  if (await showConfirmation(t("STATE_MACHINE.DETAILS.ACTIVE_CONFIRM", { name: other.name ?? "" }))) {
    deactivate.value = other;
  } else {
    model.value.entity.isActive = false;
  }
}

async function save() {
  loading.value = true;
  try {
    const { entity, localizations, attributes } = model.value;
    const statesGraph = normalizePositions(entity.statesGraph);
    const definition: StateMachineDefinition = {
      ...entity,
      statesGraph,
      states: JSON.parse(statesGraph || "[]"),
      version: entity.version || "0",
      statesCapture:
        statesGraph !== savedGraph || !entity.statesCapture ? await graphSnapshot(statesGraph) : entity.statesCapture,
    };
    const saved = await api.stateMachine.createNewDefinition({ definition });
    const definitionId = saved.id ?? entity.id;
    // texts of a new definition get its id now (legacy saved them without one)
    const withId = <T extends { definitionId?: string }>(list: T[]) => list.map((x) => ({ ...x, definitionId }));
    await Promise.all([
      // the localization endpoint needs its own permission; without it the texts stay as they are
      localizations.length &&
        hasAccess("statemachine:localize") &&
        api.stateMachine.updateLocalization({ localizations: withId(localizations) }),
      attributes.length && api.stateMachine.updateAttribute({ attributes: withId(attributes) }),
      deactivate.value && api.stateMachine.createNewDefinition({ definition: { ...deactivate.value, isActive: false } }),
    ]);
    form.setBaseline();
    callParent("reload");
    if (!param.value && definitionId) {
      savedId.value = definitionId;
      callParent("select", definitionId);
    }
    // the editor edits the model that load() replaces
    await closeChildren();
    await load();
  } catch (e) {
    showError(errorText(e));
  } finally {
    loading.value = false;
  }
}

function openEditor() {
  const options: EditorOptions = { model: model.value };
  openBlade({ name: "StateMachineVisualEditor", options });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("STATE_MACHINE.TOOLBAR.SAVE"),
    icon: "lucide-save",
    // the definition is saved through the create endpoint, new or not
    permissions: param.value ? "statemachine:update" : "statemachine:create",
    disabled:
      loading.value || !form.canSave.value || !model.value.entity.name || !model.value.entity.entityType || !hasStates.value,
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("STATE_MACHINE.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    permissions: "statemachine:update",
    disabled: !form.isModified.value,
    async clickHandler() {
      // the editor shows the model being reset, so it is closed first
      await closeChildren();
      await load();
    },
  },
  {
    id: "export",
    title: t("STATE_MACHINE.TOOLBAR.EXPORT"),
    icon: "lucide-upload",
    // a new definition has nothing saved to export yet
    isVisible: !!param.value,
    async clickHandler() {
      try {
        await exportDefinition(model.value);
      } catch (e) {
        showError(errorText(e));
      }
    },
  },
]);

onMounted(load);
</script>

<style lang="scss" scoped>
.sm-details {
  display: flex;
  flex-direction: column;
  gap: 16px;

  &__states {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  &__preview {
    max-width: 380px;
    padding: 0;
    border: 1px solid var(--neutrals-200);
    border-radius: 6px;
    background: #fff;
    cursor: pointer;

    &:focus-visible {
      outline: 2px solid var(--primary-500);
      outline-offset: 2px;
    }

    img {
      display: block;
      width: 100%;
    }
  }
}
</style>
