<template>
  <!-- legacy three-position-toggle on a final state: Success / neither / Fail -->
  <div
    class="sm-toggle"
    role="radiogroup"
  >
    <span
      class="sm-toggle__label"
      @click.stop="select('left')"
      @pointerdown.stop
      >{{ $t("STATE_MACHINE.EDITOR.SUCCESS") }}</span
    >
    <!-- as legacy: a click anywhere on the track picks the third under the pointer -->
    <div
      class="sm-toggle__track"
      @click.stop="pick"
      @pointerdown.stop
    >
      <button
        v-for="option in OPTIONS"
        :key="option.value"
        type="button"
        role="radio"
        :aria-checked="modelValue === option.value"
        :aria-label="option.label ? $t(option.label) : undefined"
        :disabled="disabled"
        :class="['sm-toggle__option', `sm-toggle__option--${option.value}`, { 'sm-toggle__option--on': modelValue === option.value }]"
        @click.stop="select(option.value)"
      >
        <VcIcon
          v-if="option.icon && modelValue === option.value"
          :icon="option.icon"
          size="xs"
        />
      </button>
    </div>
    <span
      class="sm-toggle__label"
      @click.stop="select('right')"
      @pointerdown.stop
      >{{ $t("STATE_MACHINE.EDITOR.FAIL") }}</span
    >
  </div>
</template>

<script setup lang="ts">
import { VcIcon } from "@vc-shell/framework/ui";

export type TogglePosition = "left" | "center" | "right";

const OPTIONS: { value: TogglePosition; icon?: string; label?: string }[] = [
  { value: "left", icon: "lucide-check", label: "STATE_MACHINE.EDITOR.SUCCESS" },
  { value: "center" },
  { value: "right", icon: "lucide-ban", label: "STATE_MACHINE.EDITOR.FAIL" },
];

const props = defineProps<{ modelValue: TogglePosition; disabled?: boolean }>();
const emit = defineEmits<{ "update:modelValue": [value: TogglePosition] }>();

function select(value: TogglePosition) {
  if (!props.disabled) emit("update:modelValue", value);
}

function pick(e: MouseEvent) {
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
  const third = ((e.clientX - rect.left) / rect.width) * 3;
  select(third < 1 ? "left" : third >= 2 ? "right" : "center");
}
</script>

<style lang="scss" scoped>
.sm-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  font-size: 11px;

  &__label {
    color: var(--neutrals-500);
    cursor: pointer;
    user-select: none;
  }

  &__track {
    display: flex;
    gap: 2px;
    padding: 2px;
    border-radius: 4px;
    background: var(--neutrals-200);
    cursor: pointer;
  }

  // every position is drawn, as legacy, so the three choices are visible
  &__option {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 16px;
    padding: 0;
    border: 0;
    border-radius: 3px;
    background: var(--neutrals-300);
    color: #fff;
    cursor: pointer;

    &:disabled {
      cursor: not-allowed;
      opacity: 0.6;
    }

    &--on {
      background: var(--neutrals-500);
    }

    &--left.sm-toggle__option--on {
      background: var(--success-500);
    }

    &--right.sm-toggle__option--on {
      background: var(--danger-500);
    }

    &:focus-visible {
      outline: 2px solid var(--primary-500);
    }
  }
}
</style>
