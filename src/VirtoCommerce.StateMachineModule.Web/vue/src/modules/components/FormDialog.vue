<template>
  <VcPopup
    :model-value="true"
    :title="title"
    @close="$emit('cancel')"
  >
    <template #content>
      <form
        class="sm-dialog"
        @submit.prevent="valid && $emit('ok')"
      >
        <!-- one value per language / attribute key -->
        <template v-if="rows">
          <VcInput
            v-for="row in rows"
            :key="row.key"
            v-model="row.value"
            :label="row.key"
          />
        </template>
        <template
          v-for="field in fields ?? []"
          v-else
          :key="field.name"
        >
          <VcTextarea
            v-if="field.multiline"
            v-model="values![field.name]"
            :label="field.label"
          />
          <VcInput
            v-else
            v-model="values![field.name]"
            :name="field.name"
            :label="field.label"
            :required="field.required"
            :error="!!error"
            :error-message="error"
            autofocus
          />
        </template>
        <button
          type="submit"
          hidden
        />
      </form>
    </template>
    <template #footer>
      <VcButton
        variant="secondary"
        @click="$emit('cancel')"
        >{{ $t("STATEMACHINE.MODALS.CANCEL") }}</VcButton
      >
      <VcButton
        :disabled="!valid"
        @click="$emit('ok')"
        >{{ $t("STATEMACHINE.MODALS.OK") }}</VcButton
      >
    </template>
  </VcPopup>
</template>

<script setup lang="ts">
// The AngularJS modal-form: a few text fields (state, transition) or one input per row (localization, attributes).
import { VcButton, VcInput, VcPopup, VcTextarea } from "@vc-shell/framework/ui";
import { computed } from "vue";

export interface DialogField {
  name: string;
  label: string;
  required?: boolean;
  multiline?: boolean;
}

const props = defineProps<{
  title: string;
  fields?: DialogField[];
  values?: Record<string, string>;
  rows?: { key: string; value?: string }[];
  error?: string;
}>();
defineEmits<{ (e: "ok"): void; (e: "cancel"): void }>();

const valid = computed(
  () => !props.error && (props.fields ?? []).every((f) => !f.required || !!props.values?.[f.name]?.trim()),
);
</script>

<style scoped>
.sm-dialog {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-width: min(360px, 80vw);
}
</style>
