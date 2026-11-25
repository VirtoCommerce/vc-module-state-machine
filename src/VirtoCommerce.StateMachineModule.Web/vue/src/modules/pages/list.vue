<template>
  <VcBlade
    :title="$t('STATE_MACHINE.LIST.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :state-key="'STATE_MACHINE_LIST'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-workflow', title: $t('STATE_MACHINE.LIST.EMPTY') }"
      @row-click="(e: { data: StateMachineDefinition }) => edit(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('STATE_MACHINE.LIST.NAME')"
        mobile-role="title"
        sortable
        always-visible
      />
      <VcColumn
        id="isActive"
        :title="$t('STATE_MACHINE.LIST.STATUS')"
        mobile-role="status"
        sortable
        width="120px"
      >
        <template #body="{ data }">
          <span :class="data.isActive ? 'sm-status sm-status--active' : 'sm-status'">
            {{ data.isActive ? $t("STATE_MACHINE.LIST.ACTIVE") : $t("STATE_MACHINE.LIST.INACTIVE") }}
          </span>
        </template>
      </VcColumn>
      <VcColumn
        id="entityType"
        :title="$t('STATE_MACHINE.LIST.ENTITY_TYPE')"
        mobile-role="field"
        sortable
      >
        <template #body="{ data }">{{ entityTypeName(entityTypes, data.entityType) }}</template>
      </VcColumn>
      <VcColumn
        id="modifiedDate"
        :title="$t('STATE_MACHINE.LIST.MODIFIED')"
        type="date"
        mobile-role="field"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, useDataTableSort, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api, errorText } from "../api";
import { entityTypeName, loadEntityTypes } from "../entityTypes";
import { importDefinition, pickFile } from "../transfer";
import type { StateMachineDefinition, StateMachineEntityType } from "../../api_client/virtocommerce.statemachine";

// legacy state-machine-list, the workspace opened from the main menu
defineBlade({
  url: "/state-machines",
  name: "StateMachineList",
  isWorkspace: true,
  permissions: ["statemachine:access"],
  menuItem: { title: "STATE_MACHINE.MENU.TITLE", icon: "lucide-workflow", priority: 800 },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren } = useBlade();
const { showError, showInfo } = usePopup();

const items = ref<StateMachineDefinition[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const activeItemId = ref<string>();
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "name", initialDirection: "ASC" });
const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const entityTypes = ref<StateMachineEntityType[]>([]);
loadEntityTypes().then((x) => (entityTypes.value = x));

async function load() {
  loading.value = true;
  try {
    const res = await api.stateMachine.search({ sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
    items.value = res.results ?? [];
    totalCount.value = res.totalCount ?? 0;
  } catch (e) {
    showError(errorText(e));
  } finally {
    loading.value = false;
  }
}
load();
watch([sortExpression, () => pagination.skip], load);

function edit(id?: string) {
  activeItemId.value = id;
  openBlade({ name: "StateMachineDetails", param: id, onClose: () => (activeItemId.value = undefined) });
}

async function importArchive() {
  const file = await pickFile(".zip");
  if (!file) return;
  loading.value = true;
  try {
    const created = await importDefinition(file);
    showInfo(t("STATE_MACHINE.LIST.IMPORTED", { name: created.name ?? "" }));
  } catch (e) {
    showError(errorText(e));
  } finally {
    loading.value = false;
  }
  await load();
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("STATE_MACHINE.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("STATE_MACHINE.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "statemachine:create",
    clickHandler: () => edit(),
  },
  {
    id: "import",
    title: t("STATE_MACHINE.TOOLBAR.IMPORT"),
    icon: "lucide-download",
    // the import saves through the create endpoint
    permissions: "statemachine:create",
    disabled: loading.value,
    clickHandler: importArchive,
  },
]);

// select: a saved new definition's row becomes the active one
exposeToChildren({ reload: load, select: (id: string) => (activeItemId.value = id) });
</script>

<style lang="scss" scoped>
.sm-status {
  color: var(--neutrals-500);

  &--active {
    color: var(--success-600);
  }
}
</style>
