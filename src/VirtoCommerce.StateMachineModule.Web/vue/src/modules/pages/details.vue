<template>
  <VcBlade
    :title="title"
    :subtitle="model.entity.entityType"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    :modified="form.isModified.value"
    width="50%"
  >
    <VcContainer>
      <VcForm class="sm-form">
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="name"
          rules="required"
          :model-value="model.entity.name"
        >
          <VcInput
            v-model="model.entity.name"
            :label="$t('STATEMACHINE.PAGES.DETAILS.FIELDS.NAME')"
            :placeholder="$t('STATEMACHINE.PAGES.DETAILS.FIELDS.NAME_PLACEHOLDER')"
            required
            :disabled="!canEdit"
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <!-- Legacy offered the entity types other modules registered on the client; no installed module registers
             one, so the type is typed in. It is fixed once the definition exists. -->
        <Field
          v-slot="{ errorMessage, handleChange, errors }"
          name="entityType"
          rules="required"
          :model-value="model.entity.entityType"
        >
          <VcInput
            v-model="model.entity.entityType"
            :label="$t('STATEMACHINE.PAGES.DETAILS.FIELDS.ENTITY_TYPE')"
            :placeholder="$t('STATEMACHINE.PAGES.DETAILS.FIELDS.ENTITY_TYPE_PLACEHOLDER')"
            :hint="isNew ? $t('STATEMACHINE.PAGES.DETAILS.FIELDS.ENTITY_TYPE_HINT') : undefined"
            required
            :disabled="!isNew"
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="handleChange"
          />
        </Field>
        <VcSwitch
          :model-value="model.entity.isActive"
          :label="$t('STATEMACHINE.PAGES.DETAILS.FIELDS.IS_ACTIVE')"
          :disabled="!canEdit"
          @update:model-value="onActiveChanged"
        />
        <div class="sm-states">
          <VcLabel>{{ $t("STATEMACHINE.PAGES.DETAILS.FIELDS.STATES") }}</VcLabel>
          <div>
            <VcButton
              icon="lucide-workflow"
              variant="secondary"
              @click="openEditor"
              >{{ $t("STATEMACHINE.PAGES.DETAILS.OPEN_EDITOR") }}</VcButton
            >
          </div>
          <button
            v-if="preview"
            type="button"
            class="sm-states__preview"
            tabindex="-1"
            aria-hidden="true"
            @click="openEditor"
          >
            <img
              :src="preview"
              alt=""
            />
          </button>
          <VcHint v-else>{{ $t("STATEMACHINE.PAGES.DETAILS.FIELDS.NO_STATES") }}</VcHint>
        </div>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  notification,
  useApiClient,
  useAsync,
  useBlade,
  useBladeForm,
  useLoading,
  usePermissions,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcButton, VcContainer, VcForm, VcHint, VcInput, VcLabel, VcSwitch } from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import {
  StateMachineClient,
  type StateMachineDefinition,
  type StateMachineSettings,
} from "../../api_client/virtocommerce.statemachine";
import type { Draft, DraftModel } from "../draft";
import { graphSnapshot, graphSvg, normalizePositions, svgDataUrl } from "../graph";
import { exportDefinition, saveTexts } from "../transfer";

defineBlade({
  url: "/state-machine",
  name: "StateMachineDetails",
  permissions: ["statemachine:read"],
});

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, exposeToChildren } = useBlade();
const { hasAccess } = usePermissions();
const { showConfirmation } = usePopup();
const { getApiClient } = useApiClient(StateMachineClient);

// a new definition keeps this blade after its first save; the id lives here
const definitionId = ref(param.value);
const isNew = computed(() => !model.value.entity.createdDate);
const canEdit = computed(() => hasAccess("statemachine:update"));

const model = ref<DraftModel>({ entity: { isActive: false }, localizations: [], attributes: [] });
const settings = ref<StateMachineSettings>({});
// the other active definition of the same entity type, switched off with this one's save
const otherActive = ref<StateMachineDefinition>();
let savedTexts = "";
let savedGraph: string | undefined;

const form = useBladeForm({
  data: model,
  closeConfirmMessage: computed(() => t("STATEMACHINE.ALERTS.CLOSE_CONFIRMATION")),
});

const title = computed(() => model.value.entity.name || (isNew.value ? t("STATEMACHINE.PAGES.DETAILS.NEW_TITLE") : ""));
const preview = computed(() => {
  try {
    const svg = graphSvg(model.value.entity.statesGraph);
    return svg ? svgDataUrl(svg) : model.value.entity.statesCapture;
  } catch {
    return model.value.entity.statesCapture;
  }
});

const texts = (m: DraftModel) => JSON.stringify([m.localizations, m.attributes]);

const { loading: loadLoading, action: load } = useAsync(async () => {
  const client = await getApiClient();
  // the settings load alongside the definition
  const loadingSettings = client.getSettings().catch(() => ({}));
  if (definitionId.value) {
    const id = definitionId.value;
    const [entity, localizations, attributes] = await Promise.all([
      client.getStateMachineById(id),
      client.searchLocalization({ definitionId: id, take: 10000 }),
      client.searchAttribute({ definitionId: id, take: 10000 }),
    ]);
    // definitions made before the editor existed have only their states
    entity.statesGraph ||= JSON.stringify(entity.states ?? [], null, 2);
    model.value = { entity, localizations: localizations.results ?? [], attributes: attributes.results ?? [] };
  }
  settings.value = await loadingSettings;
  otherActive.value = undefined;
  savedTexts = texts(model.value);
  savedGraph = model.value.entity.statesGraph;
  form.setBaseline();
});

const { loading: saveLoading, action: save } = useAsync(async () => {
  const client = await getApiClient();
  const entity = { ...model.value.entity };
  const graphChanged = entity.statesGraph !== savedGraph;
  entity.statesGraph ||= "[]";
  // as legacy, only a graph the editor changed is moved to the margin; an untouched one is saved as stored
  if (graphChanged) entity.statesGraph = normalizePositions(entity.statesGraph);
  entity.states = JSON.parse(entity.statesGraph);
  entity.version ||= "0";
  if (graphChanged || !entity.statesCapture) entity.statesCapture = (await graphSnapshot(entity.statesGraph)) ?? entity.statesCapture;

  // the definition first (a new one gets its id), then its texts and the other active definition together, as legacy
  const saved = await client.createNewDefinition({ definition: entity });
  definitionId.value = saved.id;
  await Promise.all([
    texts(model.value) !== savedTexts && saveTexts(client, saved.id, model.value),
    otherActive.value && client.createNewDefinition({ definition: otherActive.value }),
  ]);

  notification.success(t("STATEMACHINE.ALERTS.SAVED"), { timeout: 2500 });
  await load();
  await callParent("reload").catch(() => undefined);
  await callParent("select", saved.id).catch(() => undefined);
});

const loading = useLoading(loadLoading, saveLoading);

// Only one definition of an entity type is active: switching this one on offers to switch the other off on save.
async function onActiveChanged(value?: boolean) {
  model.value.entity.isActive = !!value;
  otherActive.value = undefined;
  if (!value || !model.value.entity.entityType) return;
  const client = await getApiClient();
  // at most this definition and the other active one
  const sameType = await client.search({ objectTypes: [model.value.entity.entityType], isActive: true, take: 2 });
  const other = sameType.results?.find((x) => x.isActive && x.id !== model.value.entity.id);
  if (!other) return;
  if (await showConfirmation(t("STATEMACHINE.ALERTS.DEACTIVATE_OTHER"))) otherActive.value = { ...other, isActive: false };
  else model.value.entity.isActive = false;
}

// a getter keeps the refs as they are (blade options are made reactive, which would unwrap them)
const openEditor = () => openBlade({ name: "StateMachineEditor", options: { draft: (): Draft => ({ model, settings }) } });

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("STATEMACHINE.PAGES.DETAILS.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "statemachine:create",
    disabled: computed(() => !form.canSave.value || saveLoading.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("STATEMACHINE.PAGES.DETAILS.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => {
      form.revert();
      otherActive.value = undefined;
    },
  },
  {
    id: "export",
    title: t("STATEMACHINE.PAGES.DETAILS.TOOLBAR.EXPORT"),
    icon: "lucide-download",
    clickHandler: () => exportDefinition(model.value),
  },
]);

onMounted(load);
exposeToChildren({ reload: () => load() });
</script>

<style lang="scss">
.sm-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px 22px 36px;
}

.sm-states {
  display: flex;
  flex-direction: column;
  gap: 8px;

  &__preview {
    align-self: flex-start;
    max-width: 100%;
    padding: 0;
    border: 1px solid var(--color-neutral-200);
    border-radius: 6px;
    overflow: hidden;
    cursor: pointer;
    background: none;

    img {
      display: block;
      max-width: 380px;
      width: 100%;
    }
  }
}
</style>
