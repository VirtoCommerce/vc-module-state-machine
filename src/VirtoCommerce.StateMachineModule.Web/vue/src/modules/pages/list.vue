<template>
  <VcBlade
    :title="$t('STATEMACHINE.MENU.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      state-key="STATEMACHINES"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-workflow', title: $t('STATEMACHINE.PAGES.LIST.EMPTY') }"
      @row-click="(e: { data: StateMachineDefinition }) => openDetails(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('STATEMACHINE.PAGES.LIST.COLUMNS.NAME')"
        mobile-role="title"
        sortable
        always-visible
      />
      <VcColumn
        id="entityType"
        :title="$t('STATEMACHINE.PAGES.LIST.COLUMNS.ENTITY_TYPE')"
        mobile-role="field"
        sortable
      />
      <VcColumn
        id="isActive"
        :title="$t('STATEMACHINE.PAGES.LIST.COLUMNS.STATUS')"
        mobile-role="status"
        sortable
        always-visible
      >
        <template #body="{ data: d }">
          <div class="tw-inline-flex">
            <VcStatus :variant="d.isActive ? 'success' : 'info'">{{
              d.isActive ? $t("STATEMACHINE.PAGES.LIST.ACTIVE") : $t("STATEMACHINE.PAGES.LIST.INACTIVE")
            }}</VcStatus>
          </div>
        </template>
      </VcColumn>
      <VcColumn
        id="modifiedDate"
        :title="$t('STATEMACHINE.PAGES.LIST.COLUMNS.MODIFIED')"
        type="date"
        mobile-role="field"
        sortable
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  notification,
  useApiClient,
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  useLoading,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { StateMachineClient, type StateMachineDefinition } from "../../api_client/virtocommerce.statemachine";
import { importDefinition } from "../transfer";

defineBlade({
  url: "/state-machines",
  name: "StateMachineList",
  isWorkspace: true,
  permissions: ["statemachine:access"],
  menuItem: {
    title: "STATEMACHINE.MENU.TITLE",
    icon: "lucide-workflow",
    priority: 55,
  },
});

// The AngularJS list scrolled endlessly through the definitions; the server has no keyword search, so the list has none.
const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren } = useBlade();
const { debounce } = useFunctions();
const { getApiClient } = useApiClient(StateMachineClient);

const PAGE_SIZE = 20;
const items = ref<StateMachineDefinition[]>([]);
const totalCount = ref(0);
const activeItemId = ref<string>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "STATEMACHINES" });
const pagination = useDataTablePagination({ stateKey: "STATEMACHINES", pageSize: PAGE_SIZE, totalCount });

const { loading: listLoading, action: load } = useAsync(async () => {
  const result = await (await getApiClient()).search({ sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
  items.value = result.results ?? [];
  totalCount.value = result.totalCount ?? 0;
});

const { loading: importLoading, action: importFile } = useAsync<File>(async (file) => {
  try {
    const created = await importDefinition(file!, await getApiClient());
    notification.success(t("STATEMACHINE.ALERTS.IMPORTED"), { timeout: 2500 });
    await load();
    openDetails(created.id);
  } catch (e) {
    notification.error(t("STATEMACHINE.ALERTS.IMPORT_FAILED", { error: (e as Error).message }), { timeout: 8000 });
  }
});

const loading = useLoading(listLoading, importLoading);

load();
watch([sortExpression, () => pagination.skip], debounce(load, 300));

function openDetails(id?: string) {
  openBlade({
    name: "StateMachineDetails",
    param: id,
    onOpen: () => (activeItemId.value = id),
    onClose: () => (activeItemId.value = undefined),
  });
}

function pickArchive() {
  const input = document.createElement("input");
  input.type = "file";
  input.accept = ".zip";
  input.onchange = () => input.files?.[0] && importFile(input.files[0]);
  input.click();
}

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("STATEMACHINE.PAGES.LIST.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "new",
    title: t("STATEMACHINE.PAGES.LIST.TOOLBAR.NEW"),
    icon: "lucide-plus",
    permissions: "statemachine:create",
    clickHandler: () => openDetails(),
  },
  {
    id: "import",
    title: t("STATEMACHINE.PAGES.LIST.TOOLBAR.IMPORT"),
    icon: "lucide-upload",
    permissions: "statemachine:create",
    disabled: computed(() => importLoading.value),
    clickHandler: pickArchive,
  },
]);

exposeToChildren({
  reload: () => load(),
  // a new definition, once saved, is highlighted like an opened one
  select: (id: string) => (activeItemId.value = id),
});
</script>
