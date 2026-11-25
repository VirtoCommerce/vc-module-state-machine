<template>
  <!-- legacy va-dynamic-expression-tree: one node of a transition's condition tree -->
  <div :class="['sm-condition', { 'sm-condition--block': !root && isBlock(node) }]">
    <!-- the root (the prototype) only holds its blocks -->
    <template v-if="!root">
      <div
        v-if="typeof node.all === 'boolean'"
        class="sm-condition__line"
      >
        {{ $t("STATE_MACHINE.CONDITION.IF") }}
        <VcSelect
          class="sm-condition__inline"
          size="small"
          :model-value="node.all ? 'all' : 'any'"
          :options="[
            { id: 'any', title: $t('STATE_MACHINE.CONDITION.ANY') },
            { id: 'all', title: $t('STATE_MACHINE.CONDITION.ALL') },
          ]"
          :clearable="false"
          :disabled="readonly"
          :aria-label="$t('STATE_MACHINE.CONDITION.IF')"
          @update:model-value="(v: unknown) => (node.all = v === 'all')"
        />
        {{ $t("STATE_MACHINE.CONDITION.OF_THESE") }}
      </div>

      <!-- StateMachineConditionHasPermission -->
      <div
        v-else-if="node.id === 'StateMachineConditionHasPermission'"
        class="sm-condition__line"
      >
        <VcSelect
          class="sm-condition__inline"
          size="small"
          :model-value="node.notHas ? 'no' : 'has'"
          :options="hasOptions"
          :clearable="false"
          :disabled="readonly"
          :aria-label="$t('STATE_MACHINE.CONDITION.PERMISSION')"
          @update:model-value="(v: unknown) => (node.notHas = v === 'no')"
        />
        {{ $t("STATE_MACHINE.CONDITION.PERMISSION") }}
        <VcSelect
          class="sm-condition__wide"
          size="small"
          multiple
          searchable
          :model-value="(node.permissions as string[] | undefined) ?? []"
          :options="permissionOptions"
          :placeholder="$t('STATE_MACHINE.CONDITION.SELECT')"
          :disabled="readonly"
          :aria-label="$t('STATE_MACHINE.CONDITION.PERMISSION')"
          @update:model-value="(v: unknown) => (node.permissions = v)"
        />
      </div>

      <!-- StateMachineConditionHasAccountType -->
      <div
        v-else-if="node.id === 'StateMachineConditionHasAccountType'"
        class="sm-condition__line"
      >
        {{ $t("STATE_MACHINE.CONDITION.USER") }}
        <VcSelect
          class="sm-condition__inline"
          size="small"
          :model-value="node.notHas ? 'no' : 'has'"
          :options="hasOptions"
          :clearable="false"
          :disabled="readonly"
          :aria-label="$t('STATE_MACHINE.CONDITION.ACCOUNT_TYPE')"
          @update:model-value="(v: unknown) => (node.notHas = v === 'no')"
        />
        {{ $t("STATE_MACHINE.CONDITION.ACCOUNT_TYPE") }}
        <VcSelect
          class="sm-condition__wide"
          size="small"
          searchable
          :model-value="node.accountType"
          :options="accountTypeOptions"
          :placeholder="$t('STATE_MACHINE.CONDITION.SELECT')"
          :disabled="readonly"
          :aria-label="$t('STATE_MACHINE.CONDITION.ACCOUNT_TYPE')"
          @update:model-value="(v: unknown) => (node.accountType = v)"
        />
      </div>

      <!-- an element of another module (no Vue template): its own fields, edited as plain values -->
      <div
        v-else
        class="sm-condition__line"
      >
        <span>{{ displayName(node) }}</span>
        <template
          v-for="field in ownFields(node)"
          :key="field"
        >
          <VcSwitch
            v-if="typeof node[field] === 'boolean'"
            :model-value="node[field] as boolean"
            :label="humanize(field)"
            :disabled="readonly"
            @update:model-value="(v?: boolean) => (node[field] = !!v)"
          />
          <VcInput
            v-else
            class="sm-condition__wide"
            size="small"
            :model-value="node[field] as string | number"
            :placeholder="humanize(field)"
            :aria-label="humanize(field)"
            :type="typeof node[field] === 'number' ? 'number' : 'text'"
            :disabled="readonly"
            @update:model-value="(v: unknown) => (node[field] = typeof node[field] === 'number' ? Number(v) : v)"
          />
        </template>
      </div>
    </template>

    <ul
      v-if="node.children?.length"
      class="sm-condition__children"
    >
      <li
        v-for="(child, index) in node.children"
        :key="index"
        class="sm-condition__child"
      >
        <ConditionNodeEditor
          :node="child"
          :label="offeredLabel(child)"
          :readonly="readonly"
          :permission-options="permissionOptions"
          :account-type-options="accountTypeOptions"
        />
        <VcButton
          v-if="!root && !readonly"
          icon="lucide-x"
          variant="ghost"
          size="icon-sm"
          :title="$t('STATE_MACHINE.CONDITION.REMOVE')"
          :aria-label="$t('STATE_MACHINE.CONDITION.REMOVE')"
          @click="node.children!.splice(index, 1)"
        />
      </li>
    </ul>

    <div
      v-if="!root && !readonly && isBlock(node)"
      class="sm-condition__add"
    >
      <VcButton
        v-for="offered in node.availableChildren"
        :key="offered.id"
        icon="lucide-plus"
        variant="secondary"
        size="sm"
        :title="$t('STATE_MACHINE.CONDITION.ADD')"
        @click="(node.children ??= []).push(newChild(offered))"
      >
        {{ displayName(offered) }}
      </VcButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { VcButton, VcInput, VcSelect, VcSwitch } from "@vc-shell/framework/ui";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { humanize, isBlock, newChild, ownFields, storedDisplayName } from "../conditions";
import type { ConditionNode } from "../types";

defineOptions({ name: "ConditionNodeEditor" });

const props = defineProps<{
  node: ConditionNode;
  /** the name its parent offers it under: a child added here has no legacy display name of its own */
  label?: string;
  root?: boolean;
  readonly?: boolean;
  permissionOptions: { id: string; title: string }[];
  accountTypeOptions: { id: string; title: string }[];
}>();

const { t, te } = useI18n({ useScope: "global" });

const hasOptions = computed(() => [
  { id: "has", title: t("STATE_MACHINE.CONDITION.HAS") },
  { id: "no", title: t("STATE_MACHINE.CONDITION.HAS_NO") },
]);

// legacy displayName of this module's expressions; another module's by the name saved with it, else its type name
const displayName = (node: ConditionNode) =>
  te(`STATE_MACHINE.CONDITION.ELEMENTS.${node.id}`)
    ? t(`STATE_MACHINE.CONDITION.ELEMENTS.${node.id}`)
    : (storedDisplayName(node) ?? (node === props.node ? props.label : undefined) ?? humanize(node.id));

const offeredLabel = (child: ConditionNode) => {
  const offered = props.node.availableChildren?.find((x) => x.id === child.id);
  return offered ? displayName(offered) : undefined;
};
</script>

<style lang="scss" scoped>
.sm-condition {
  display: flex;
  flex-direction: column;
  gap: 8px;

  &--block {
    padding: 8px 12px;
    border-left: 3px solid var(--primary-500);
    border-radius: 4px;
    background: var(--neutrals-50);
  }

  &__line {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 8px;
    color: var(--neutrals-800);
  }

  &__inline {
    width: 110px;
  }

  &__wide {
    flex: 1;
    min-width: 200px;
  }

  &__children {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
  }

  &__child {
    display: flex;
    align-items: flex-start;
    gap: 8px;

    > :first-child {
      flex: 1;
    }
  }

  &__add {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  }
}
</style>
