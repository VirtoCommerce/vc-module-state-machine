<template>
  <!-- legacy context-menu: the right-click menu of a state, a transition or a dropped connection -->
  <ul
    ref="menuEl"
    class="sm-menu"
    role="menu"
    :style="{ left: `${x}px`, top: `${y}px` }"
    @keydown.esc.stop="emit('close')"
    @contextmenu.prevent
  >
    <li
      v-for="item in items"
      :key="item.id"
      role="none"
    >
      <button
        type="button"
        role="menuitem"
        class="sm-menu__item"
        @click="choose(item)"
      >
        <VcIcon
          :icon="item.icon"
          size="s"
        />
        {{ item.label }}
      </button>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { VcIcon } from "@vc-shell/framework/ui";
import { nextTick, onBeforeUnmount, onMounted, ref } from "vue";

export type MenuItem = { id: string; label: string; icon: string; action: () => void };

defineProps<{ items: MenuItem[]; x: number; y: number }>();
const emit = defineEmits<{ close: [] }>();
const menuEl = ref<HTMLElement>();

function choose(item: MenuItem) {
  emit("close");
  item.action();
}

// a press anywhere else closes it
function onPointerDown(e: PointerEvent) {
  if (!menuEl.value?.contains(e.target as Node)) emit("close");
}

onMounted(async () => {
  document.addEventListener("pointerdown", onPointerDown, true);
  await nextTick();
  menuEl.value?.querySelector<HTMLElement>("button")?.focus();
});
onBeforeUnmount(() => document.removeEventListener("pointerdown", onPointerDown, true));
</script>

<style lang="scss" scoped>
.sm-menu {
  position: absolute;
  z-index: 20;
  min-width: 200px;
  margin: 0;
  padding: 4px 0;
  list-style: none;
  border: 1px solid var(--neutrals-200);
  border-radius: 6px;
  background: var(--additional-50);
  box-shadow: 0 4px 16px rgb(0 0 0 / 15%);

  &__item {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;
    padding: 6px 12px;
    border: 0;
    background: none;
    color: var(--neutrals-800);
    font: inherit;
    text-align: left;
    cursor: pointer;

    &:hover,
    &:focus-visible {
      outline: none;
      background: var(--primary-50);
    }
  }
}
</style>
