<template>
  <VcBlade
    :title="$t('STATE_MACHINE.CONDITION.TITLE')"
    :subtitle="options ? $t('STATE_MACHINE.CONDITION.SUBTITLE', { trigger: options.trigger }) : undefined"
    :toolbar-items="toolbar"
    :loading="loading"
    :modified="form.isModified.value"
    width="50%"
  >
    <VcContainer>
      <section
        v-if="tree.node"
        class="sm-conditions"
        :aria-label="$t('STATE_MACHINE.CONDITION.CONDITIONS')"
      >
        <h3 class="sm-conditions__title">{{ $t("STATE_MACHINE.CONDITION.CONDITIONS") }}</h3>
        <ConditionNodeEditor
          :node="tree.node"
          root
          :readonly="readonly"
          :permission-options="permissionOptions"
          :account-type-options="accountTypeOptions"
        />
      </section>
      <VcHint v-else-if="!loading">{{ $t("STATE_MACHINE.CONDITION.NO_PROTOTYPE") }}</VcHint>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePermissions, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcHint } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, clone } from "../api";
import ConditionNodeEditor from "../components/ConditionNodeEditor.vue";
import { loadAccountTypes, loadPermissions } from "../conditions";
import type { ConditionNode, ConditionOptions } from "../types";

// legacy state-machine-condition
defineBlade({ url: "/state-machine-condition", name: "StateMachineCondition", permissions: ["statemachine:update"] });

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<ConditionOptions>();
const { hasAccess } = usePermissions();

// wrapped, so the form tracks changes deep in the tree, and "no prototype" is a valid state
const tree = ref<{ node?: ConditionNode }>({});
const loading = ref(false);
const readonly = computed(() => !hasAccess("statemachine:update"));
const form = useBladeForm({ data: tree, closeConfirmMessage: computed(() => t("STATE_MACHINE.CONDITION.CLOSE_CONFIRM")) });

const toOptions = (values: string[]) => values.map((x) => ({ id: x, title: x }));
const permissionOptions = ref<{ id: string; title: string }[]>([]);
const accountTypeOptions = ref<{ id: string; title: string }[]>([]);

async function load() {
  // opened from a URL (no transition to edit): nothing to show
  if (!options.value) {
    closeSelf();
    return;
  }
  loading.value = true;
  try {
    const [node, permissions, accountTypes] = await Promise.all([
      // the transition's own condition, else a fresh one from the type's prototype (legacy getConditionTreeCallback)
      options.value.condition
        ? Promise.resolve(clone(options.value.condition) as ConditionNode)
        : api.stateMachine
            .getConditionTreePrototype(options.value.entityType)
            .then((x) => x as ConditionNode)
            .catch(() => undefined),
      loadPermissions(),
      loadAccountTypes(),
    ]);
    tree.value = { node };
    permissionOptions.value = toOptions(permissions);
    accountTypeOptions.value = toOptions(accountTypes);
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function apply() {
  await callParent("applyCondition", { transitionId: options.value!.transitionId, condition: clone(tree.value.node) });
  form.setBaseline();
  closeSelf();
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "apply",
    title: t("STATE_MACHINE.TOOLBAR.APPLY"),
    icon: "lucide-check",
    permissions: "statemachine:update",
    disabled: !form.isModified.value || !tree.value.node,
    clickHandler: apply,
  },
  {
    id: "reset",
    title: t("STATE_MACHINE.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    permissions: "statemachine:update",
    disabled: !form.isModified.value,
    clickHandler: () => form.revert(),
  },
]);

onMounted(load);
</script>

<style lang="scss" scoped>
.sm-conditions {
  display: flex;
  flex-direction: column;
  gap: 12px;

  &__title {
    margin: 0;
    color: var(--neutrals-800);
    font-size: 14px;
    font-weight: 600;
  }
}
</style>
