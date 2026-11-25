<template>
  <!-- legacy modal-form: the state / transition dialogs and the localization / attribute lists of the visual editor -->
  <VcPopup
    :model-value="true"
    :title="title"
    @close="emit('cancel')"
  >
    <template #content>
      <form
        ref="formEl"
        class="sm-dialog"
        @submit.prevent="ok"
      >
        <template
          v-for="field in fields"
          :key="field.key"
        >
          <VcTextarea
            v-if="field.multiline"
            v-model="values[field.key]"
            :label="field.label"
            :name="field.key"
            maxlength="4000"
          />
          <VcInput
            v-else
            v-model="values[field.key]"
            :label="field.label"
            :name="field.key"
            :required="field.required"
            :error="!!errors[field.key]"
            :error-message="errors[field.key]"
            maxlength="512"
          />
        </template>
      </form>
    </template>
    <template #footer>
      <div class="sm-dialog__buttons">
        <VcButton
          variant="secondary"
          @click="emit('cancel')"
        >
          {{ $t("STATE_MACHINE.DIALOGS.CANCEL") }}
        </VcButton>
        <VcButton @click="ok">{{ $t("STATE_MACHINE.DIALOGS.OK") }}</VcButton>
      </div>
    </template>
  </VcPopup>
</template>

<script setup lang="ts">
import { VcButton, VcInput, VcPopup, VcTextarea } from "@vc-shell/framework/ui";
import { nextTick, onMounted, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";

export type DialogField = { key: string; label: string; required?: boolean; multiline?: boolean };

const props = defineProps<{
  title: string;
  fields: DialogField[];
  initial: Record<string, string>;
  /** an error for a field the dialog can't check itself (e.g. a duplicate name); keeps the dialog open */
  validate?: (values: Record<string, string>) => Record<string, string> | undefined;
}>();
const emit = defineEmits<{ ok: [values: Record<string, string>]; cancel: [] }>();

const { t } = useI18n({ useScope: "global" });
const values = reactive<Record<string, string>>({ ...props.initial });
const errors = reactive<Record<string, string>>({});
const formEl = ref<HTMLFormElement>();

function ok() {
  const trimmed = Object.fromEntries(props.fields.map((f) => [f.key, (values[f.key] ?? "").trim()]));
  props.fields.forEach((f) => (errors[f.key] = f.required && !trimmed[f.key] ? t("STATE_MACHINE.DIALOGS.REQUIRED") : ""));
  Object.assign(errors, props.validate?.(trimmed) ?? {});
  if (Object.values(errors).some(Boolean)) return;
  emit("ok", trimmed);
}

// as legacy: the first field gets the focus
onMounted(async () => {
  await nextTick();
  formEl.value?.querySelector<HTMLElement>("input, textarea")?.focus();
});
</script>

<style lang="scss" scoped>
// the popup's content and footer are flex rows; grow, or the form shrinks to its fields
.sm-dialog {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  gap: 12px;

  > * {
    width: 100%;
  }

  &__buttons {
    flex: 1 1 auto;
    display: flex;
    justify-content: flex-end;
    gap: 8px;
  }
}
</style>
