# State Machine Actions in Vendor Portal

## Overview

The Vendor Portal provides customer-facing state machine functionality, allowing vendors and external users to interact with workflows through a modern, responsive interface. This document explains how to integrate state machine actions into customer portals, implement permitted triggers, and create intuitive user experiences for workflow management.

## Architecture Overview

### Vendor Portal Structure

The Vendor Portal uses Vue.js/TypeScript with a composable-based architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Navigation Header                        │
├─────────────────────────────────────────────────────────────┤
│ Sidebar │                Main Content                       │
│         │                                                   │
│ ┌─────┐ │ ┌─────────────────────────────────────────────┐   │
│ │Menu │ │ │              Entity View                    │   │
│ │     │ │ │                                             │   │
│ │     │ │ ├─────────────────────────────────────────────┤   │
│ │     │ │ │          State Machine Card                 │   │
│ │     │ │ │  ┌─────────────┐ ┌─────────────────────┐    │   │
│ │     │ │ │  │Status Badge │ │  Action Buttons     │    │   │
│ │     │ │ │  └─────────────┘ └─────────────────────┘    │   │
│ │     │ │ │                                             │   │
│ │     │ │ │          Progress Indicator                 │   │
│ └─────┘ │ └─────────────────────────────────────────────┘   │
└─────────┴───────────────────────────────────────────────────┘
```

### Key Components

1. **State Display Components**: Show current state and progress
2. **Action Buttons**: Trigger permitted state transitions
3. **Progress Indicators**: Visual workflow progress
4. **History Timeline**: Audit trail of state changes
5. **Notification System**: Real-time updates and feedback

## Implementation Example: Product Details

### Composable Implementation

Based on the vendor portal product details example:

```typescript
// composables/useProductDetails/index.ts
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useNotifications } from '@/shared/notification'
import { useStateMachine } from '@/shared/state-machine'
import type { Product, StateMachineInstance, StateMachineAction } from '@/types'

export function useProductDetails(productId: string) {
  const { t } = useI18n()
  const { showNotification } = useNotifications()
  const {
    getStateMachineInstance,
    executeStateMachineAction,
    getPermittedTriggers
  } = useStateMachine()

  // Reactive state
  const product = ref<Product | null>(null)
  const stateMachine = ref<StateMachineInstance | null>(null)
  const availableActions = ref<StateMachineAction[]>([])
  const isLoading = ref(false)
  const isExecutingAction = ref(false)

  // Computed properties
  const currentState = computed(() => stateMachine.value?.currentState)
  const isInFinalState = computed(() => currentState.value?.isFinal ?? false)
  const isSuccessState = computed(() => currentState.value?.isSuccess ?? false)
  const isFailedState = computed(() => currentState.value?.isFailed ?? false)

  // State machine status computed
  const statusInfo = computed(() => {
    if (!currentState.value) return null

    return {
      name: currentState.value.localizedValue || currentState.value.name,
      description: currentState.value.description,
      cssClass: getStateClass(currentState.value),
      icon: getStateIcon(currentState.value),
      canEdit: !isInFinalState.value && hasEditPermission.value
    }
  })

  // Permission checking
  const hasEditPermission = computed(() => {
    // Check if user can edit this product
    return product.value?.permissions?.includes('product:edit') ?? false
  })

  // Load product data
  async function loadProduct() {
    try {
      isLoading.value = true

      // Load product data
      const productData = await productApi.getById(productId)
      product.value = productData

      // Load state machine instance
      await loadStateMachine()

    } catch (error) {
      console.error('Failed to load product:', error)
      showNotification({
        type: 'error',
        title: t('errors.loadFailed'),
        message: t('errors.productLoadFailed')
      })
    } finally {
      isLoading.value = false
    }
  }

  // Load state machine instance
  async function loadStateMachine() {
    if (!product.value?.id) return

    try {
      const instance = await getStateMachineInstance({
        entityType: 'Product',
        entityId: product.value.id
      })

      stateMachine.value = instance
      await loadAvailableActions()

    } catch (error) {
      console.error('Failed to load state machine:', error)
    }
  }

  // Load available actions based on permitted triggers
  async function loadAvailableActions() {
    if (!stateMachine.value) return

    try {
      const permittedTriggers = await getPermittedTriggers(stateMachine.value.id)

      availableActions.value = permittedTriggers.map(trigger =>
        createActionFromTrigger(trigger)
      ).filter(action => action !== null)

    } catch (error) {
      console.error('Failed to load available actions:', error)
    }
  }

  // Create action object from trigger
  function createActionFromTrigger(trigger: string): StateMachineAction | null {
    const actionMap: Record<string, Partial<StateMachineAction>> = {
      'Submit': {
        name: t('actions.submitForReview'),
        icon: 'paper-plane',
        variant: 'primary',
        confirmMessage: t('confirmations.submitProduct'),
        requiresConfirmation: true
      },
      'Publish': {
        name: t('actions.publish'),
        icon: 'globe',
        variant: 'success',
        confirmMessage: t('confirmations.publishProduct'),
        requiresConfirmation: true
      },
      'Unpublish': {
        name: t('actions.unpublish'),
        icon: 'eye-slash',
        variant: 'warning',
        confirmMessage: t('confirmations.unpublishProduct'),
        requiresConfirmation: true
      },
      'Archive': {
        name: t('actions.archive'),
        icon: 'archive',
        variant: 'secondary',
        confirmMessage: t('confirmations.archiveProduct'),
        requiresConfirmation: true,
        requiresComment: true
      },
      'RequestReview': {
        name: t('actions.requestReview'),
        icon: 'user-check',
        variant: 'info',
        requiresComment: true
      }
    }

    const actionConfig = actionMap[trigger]
    if (!actionConfig) {
      // Default action for unmapped triggers
      return {
        trigger,
        name: trigger,
        icon: 'cog',
        variant: 'outline',
        requiresConfirmation: false,
        requiresComment: false
      }
    }

    return {
      trigger,
      requiresConfirmation: false,
      requiresComment: false,
      ...actionConfig
    } as StateMachineAction
  }

  // Execute state machine action
  async function executeAction(action: StateMachineAction, comment?: string) {
    if (!stateMachine.value || !product.value) return false

    try {
      isExecutingAction.value = true

      const context = {
        userId: getCurrentUserId(),
        comment,
        timestamp: new Date().toISOString(),
        entityData: product.value,
        source: 'vendor-portal'
      }

      const result = await executeStateMachineAction({
        instanceId: stateMachine.value.id,
        trigger: action.trigger,
        context
      })

      if (result.success) {
        // Show success notification
        showNotification({
          type: 'success',
          title: t('notifications.actionCompleted'),
          message: t('notifications.actionSuccess', { action: action.name })
        })

        // Reload data to reflect changes
        await loadProduct()

        // Execute post-action logic
        await executePostActionLogic(action, result)

        return true
      } else {
        throw new Error(result.error || 'Action failed')
      }

    } catch (error) {
      console.error('Action execution failed:', error)

      showNotification({
        type: 'error',
        title: t('notifications.actionFailed'),
        message: error.message || t('errors.actionExecutionFailed')
      })

      return false
    } finally {
      isExecutingAction.value = false
    }
  }

  // Post-action logic
  async function executePostActionLogic(action: StateMachineAction, result: any) {
    switch (action.trigger) {
      case 'Submit':
        // Track submission analytics
        trackEvent('product_submitted', { productId: product.value?.id })
        break

      case 'Publish':
        // Trigger SEO indexing
        await triggerSeoIndexing(product.value?.id)
        break

      case 'Archive':
        // Clean up related data
        await cleanupProductData(product.value?.id)
        break
    }
  }

  // Helper functions
  function getStateClass(state: any): string {
    if (state.isSuccess) return 'success'
    if (state.isFailed) return 'error'
    if (state.isFinal) return 'info'
    if (state.isInitial) return 'default'
    return 'warning'
  }

  function getStateIcon(state: any): string {
    if (state.isSuccess) return 'check-circle'
    if (state.isFailed) return 'x-circle'
    if (state.isFinal) return 'flag'
    if (state.isInitial) return 'play-circle'
    return 'clock'
  }

  function getCurrentUserId(): string {
    // Implementation to get current user ID
    return 'current-user-id'
  }

  // Utility functions for post-action logic
  async function trackEvent(eventName: string, data: any) {
    // Analytics tracking implementation
    console.log('Tracking event:', eventName, data)
  }

  async function triggerSeoIndexing(productId: string) {
    // SEO indexing implementation
    console.log('Triggering SEO indexing for product:', productId)
  }

  async function cleanupProductData(productId: string) {
    // Data cleanup implementation
    console.log('Cleaning up data for product:', productId)
  }

  // Watch for product ID changes
  watch(() => productId, () => {
    if (productId) {
      loadProduct()
    }
  }, { immediate: true })

  return {
    // State
    product,
    stateMachine,
    availableActions,
    isLoading,
    isExecutingAction,

    // Computed
    currentState,
    statusInfo,
    isInFinalState,
    isSuccessState,
    isFailedState,
    hasEditPermission,

    // Methods
    loadProduct,
    loadStateMachine,
    executeAction,

    // Utilities
    getStateClass,
    getStateIcon
  }
}
```

### Vue Component Implementation

```vue
<!-- pages/products/details.vue -->
<template>
  <div class="product-details">
    <!-- Loading State -->
    <div v-if="isLoading" class="loading-container">
      <VcLoading />
    </div>

    <!-- Product Content -->
    <div v-else-if="product" class="product-content">

      <!-- Header Section -->
      <div class="product-header">
        <div class="product-title">
          <h1>{{ product.name }}</h1>
          <p class="product-sku">SKU: {{ product.sku }}</p>
        </div>

        <!-- State Machine Status -->
        <div v-if="statusInfo" class="status-section">
          <VcBadge
            :variant="statusInfo.cssClass"
            :icon="statusInfo.icon"
            size="lg"
          >
            {{ statusInfo.name }}
          </VcBadge>

          <p v-if="statusInfo.description" class="status-description">
            {{ statusInfo.description }}
          </p>
        </div>
      </div>

      <!-- Product Information -->
      <div class="product-info">
        <!-- Basic product details form -->
        <VcCard title="Product Information">
          <ProductForm
            v-model="product"
            :readonly="!statusInfo?.canEdit"
            @save="handleProductSave"
          />
        </VcCard>

        <!-- State Machine Card -->
        <VcCard v-if="stateMachine" title="Workflow Status" class="workflow-card">

          <!-- Current Status Display -->
          <div class="current-status">
            <div class="status-indicator">
              <VcIcon :name="statusInfo?.icon" size="lg" />
              <div class="status-text">
                <h3>{{ statusInfo?.name }}</h3>
                <p v-if="statusInfo?.description">{{ statusInfo.description }}</p>
              </div>
            </div>
          </div>

          <!-- Available Actions -->
          <div v-if="availableActions.length > 0" class="actions-section">
            <h4>{{ $t('workflow.availableActions') }}</h4>
            <div class="action-buttons">
              <VcButton
                v-for="action in availableActions"
                :key="action.trigger"
                :variant="action.variant"
                :icon="action.icon"
                :loading="isExecutingAction"
                @click="handleActionClick(action)"
              >
                {{ action.name }}
              </VcButton>
            </div>
          </div>

          <!-- Workflow Progress -->
          <div class="workflow-progress">
            <h4>{{ $t('workflow.progress') }}</h4>
            <WorkflowProgress
              :states="stateMachine.stateMachineDefinition.states"
              :current-state="currentState"
            />
          </div>

          <!-- State History -->
          <div v-if="stateMachine.history" class="state-history">
            <h4>{{ $t('workflow.history') }}</h4>
            <StateHistory :history="stateMachine.history" />
          </div>
        </VcCard>
      </div>
    </div>

    <!-- Error State -->
    <div v-else class="error-container">
      <VcAlert variant="error">
        {{ $t('errors.productNotFound') }}
      </VcAlert>
    </div>

    <!-- Action Confirmation Modal -->
    <ActionConfirmationModal
      v-model:visible="showConfirmationModal"
      :action="selectedAction"
      @confirm="handleActionConfirm"
      @cancel="handleActionCancel"
    />

    <!-- Comment Modal -->
    <CommentModal
      v-model:visible="showCommentModal"
      :action="selectedAction"
      @submit="handleCommentSubmit"
      @cancel="handleCommentCancel"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useProductDetails } from '@/modules/products/composables/useProductDetails'
import type { StateMachineAction } from '@/types'

// Composables
const route = useRoute()
const { t } = useI18n()
const productId = route.params.id as string

const {
  product,
  stateMachine,
  availableActions,
  isLoading,
  isExecutingAction,
  currentState,
  statusInfo,
  executeAction
} = useProductDetails(productId)

// Modal state
const showConfirmationModal = ref(false)
const showCommentModal = ref(false)
const selectedAction = ref<StateMachineAction | null>(null)

// Action handling
function handleActionClick(action: StateMachineAction) {
  selectedAction.value = action

  if (action.requiresComment) {
    showCommentModal.value = true
  } else if (action.requiresConfirmation) {
    showConfirmationModal.value = true
  } else {
    executeAction(action)
  }
}

function handleActionConfirm() {
  if (selectedAction.value) {
    executeAction(selectedAction.value)
  }
  showConfirmationModal.value = false
  selectedAction.value = null
}

function handleActionCancel() {
  showConfirmationModal.value = false
  selectedAction.value = null
}

function handleCommentSubmit(comment: string) {
  if (selectedAction.value) {
    executeAction(selectedAction.value, comment)
  }
  showCommentModal.value = false
  selectedAction.value = null
}

function handleCommentCancel() {
  showCommentModal.value = false
  selectedAction.value = null
}

function handleProductSave(updatedProduct: Product) {
  // Handle product save logic
  console.log('Product saved:', updatedProduct)
}
</script>

<style scoped>
.product-details {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.product-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.product-title h1 {
  margin: 0 0 0.5rem 0;
  font-size: 2rem;
  font-weight: 600;
}

.product-sku {
  color: var(--text-secondary);
  margin: 0;
}

.status-section {
  text-align: right;
}

.status-description {
  margin-top: 0.5rem;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.product-info {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 2rem;
}

.workflow-card {
  height: fit-content;
}

.current-status {
  margin-bottom: 1.5rem;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.status-text h3 {
  margin: 0 0 0.25rem 0;
  font-size: 1.25rem;
}

.status-text p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.actions-section {
  margin-bottom: 1.5rem;
}

.actions-section h4 {
  margin: 0 0 1rem 0;
  font-size: 1rem;
  font-weight: 600;
}

.action-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.workflow-progress,
.state-history {
  margin-bottom: 1.5rem;
}

.workflow-progress h4,
.state-history h4 {
  margin: 0 0 1rem 0;
  font-size: 1rem;
  font-weight: 600;
}

.loading-container,
.error-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

@media (max-width: 768px) {
  .product-header {
    flex-direction: column;
    gap: 1rem;
  }

  .status-section {
    text-align: left;
  }

  .product-info {
    grid-template-columns: 1fr;
  }

  .action-buttons {
    flex-direction: column;
  }
}
</style>
```

### Supporting Components

#### WorkflowProgress Component

```vue
<!-- components/WorkflowProgress.vue -->
<template>
  <div class="workflow-progress">
    <div class="progress-track">
      <div
        v-for="(state, index) in states"
        :key="state.name"
        class="progress-step"
        :class="getStepClass(state, index)"
      >
        <div class="step-indicator">
          <VcIcon :name="getStateIcon(state)" />
        </div>
        <div class="step-label">
          {{ state.localizedValue || state.name }}
        </div>

        <!-- Connector line -->
        <div
          v-if="index < states.length - 1"
          class="step-connector"
          :class="{ active: isStepCompleted(state) }"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { StateMachineState } from '@/types'

interface Props {
  states: StateMachineState[]
  currentState: StateMachineState | null
}

const props = defineProps<Props>()

const currentStateIndex = computed(() => {
  if (!props.currentState) return -1
  return props.states.findIndex(state => state.name === props.currentState?.name)
})

function getStepClass(state: StateMachineState, index: number): string[] {
  const classes: string[] = []

  if (index < currentStateIndex.value) {
    classes.push('completed')
  } else if (index === currentStateIndex.value) {
    classes.push('current')
  } else {
    classes.push('pending')
  }

  if (state.isSuccess) classes.push('success')
  if (state.isFailed) classes.push('error')

  return classes
}

function getStateIcon(state: StateMachineState): string {
  if (state.isSuccess) return 'check-circle'
  if (state.isFailed) return 'x-circle'
  if (state.isFinal) return 'flag'
  if (state.isInitial) return 'play-circle'
  return 'circle'
}

function isStepCompleted(state: StateMachineState): boolean {
  const stateIndex = props.states.findIndex(s => s.name === state.name)
  return stateIndex < currentStateIndex.value
}
</script>

<style scoped>
.workflow-progress {
  padding: 1rem 0;
}

.progress-track {
  display: flex;
  align-items: flex-start;
  position: relative;
}

.progress-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex: 1;
  position: relative;
}

.step-indicator {
  width: 3rem;
  height: 3rem;
  border-radius: 50%;
  background: var(--color-neutral-200);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 0.5rem;
  position: relative;
  z-index: 2;
  transition: all 0.3s ease;
}

.progress-step.completed .step-indicator {
  background: var(--color-success);
  color: white;
}

.progress-step.current .step-indicator {
  background: var(--color-primary);
  color: white;
  box-shadow: 0 0 0 4px var(--color-primary-light);
}

.progress-step.error .step-indicator {
  background: var(--color-error);
  color: white;
}

.step-label {
  text-align: center;
  font-size: 0.75rem;
  font-weight: 500;
  max-width: 5rem;
  line-height: 1.2;
}

.progress-step.current .step-label {
  font-weight: 600;
  color: var(--color-primary);
}

.step-connector {
  position: absolute;
  top: 1.5rem;
  left: 50%;
  right: -50%;
  height: 2px;
  background: var(--color-neutral-200);
  z-index: 1;
  transition: background-color 0.3s ease;
}

.step-connector.active {
  background: var(--color-success);
}

.progress-step:last-child .step-connector {
  display: none;
}
</style>
```

#### StateHistory Component

```vue
<!-- components/StateHistory.vue -->
<template>
  <div class="state-history">
    <div class="history-timeline">
      <div
        v-for="(item, index) in sortedHistory"
        :key="index"
        class="history-item"
      >
        <div class="history-marker">
          <VcIcon :name="getActionIcon(item.trigger)" />
        </div>

        <div class="history-content">
          <div class="history-header">
            <span class="history-action">{{ item.trigger }}</span>
            <span class="history-timestamp">
              {{ formatDate(item.timestamp) }}
            </span>
          </div>

          <div class="history-transition">
            <span class="from-state">{{ item.fromState }}</span>
            <VcIcon name="arrow-right" size="sm" />
            <span class="to-state">{{ item.toState }}</span>
          </div>

          <div v-if="item.userName" class="history-user">
            {{ $t('workflow.performedBy', { user: item.userName }) }}
          </div>

          <div v-if="item.comment" class="history-comment">
            <VcIcon name="message-circle" size="sm" />
            {{ item.comment }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import type { StateMachineHistoryItem } from '@/types'

interface Props {
  history: StateMachineHistoryItem[]
}

const props = defineProps<Props>()
const { t } = useI18n()

const sortedHistory = computed(() => {
  return [...props.history].sort((a, b) =>
    new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
  )
})

function getActionIcon(trigger: string): string {
  const iconMap: Record<string, string> = {
    'Submit': 'paper-plane',
    'Approve': 'check-circle',
    'Reject': 'x-circle',
    'Publish': 'globe',
    'Unpublish': 'eye-slash',
    'Archive': 'archive'
  }

  return iconMap[trigger] || 'activity'
}

function formatDate(timestamp: string): string {
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(new Date(timestamp))
}
</script>

<style scoped>
.state-history {
  max-height: 400px;
  overflow-y: auto;
}

.history-timeline {
  position: relative;
  padding-left: 2rem;
}

.history-timeline::before {
  content: '';
  position: absolute;
  left: 1rem;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--color-neutral-200);
}

.history-item {
  position: relative;
  margin-bottom: 1.5rem;
}

.history-marker {
  position: absolute;
  left: -2rem;
  top: 0;
  width: 2rem;
  height: 2rem;
  border-radius: 50%;
  background: var(--color-primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
}

.history-content {
  background: var(--color-neutral-50);
  border-radius: 0.5rem;
  padding: 1rem;
  border-left: 3px solid var(--color-primary);
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.history-action {
  font-weight: 600;
  color: var(--color-primary);
}

.history-timestamp {
  font-size: 0.75rem;
  color: var(--text-secondary);
}

.history-transition {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
}

.from-state {
  color: var(--text-secondary);
}

.to-state {
  font-weight: 500;
  color: var(--color-primary);
}

.history-user {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin-bottom: 0.5rem;
}

.history-comment {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-style: italic;
  color: var(--text-secondary);
  background: white;
  padding: 0.5rem;
  border-radius: 0.25rem;
  border: 1px solid var(--color-neutral-200);
}
</style>
```

## Advanced Features

### Real-time Updates

Implement real-time state machine updates using WebSockets:

```typescript
// composables/useRealtimeStateMachine.ts
import { ref, onMounted, onUnmounted } from 'vue'
import { useWebSocket } from '@/shared/websocket'

export function useRealtimeStateMachine(entityId: string, entityType: string) {
  const { connect, disconnect, subscribe } = useWebSocket()
  const stateMachineUpdates = ref<any[]>([])

  onMounted(() => {
    connect()

    // Subscribe to state machine updates for this entity
    subscribe(`statemachine.${entityType}.${entityId}`, (update) => {
      stateMachineUpdates.value.push(update)

      // Handle different types of updates
      switch (update.type) {
        case 'state_changed':
          handleStateChange(update)
          break
        case 'action_available':
          handleActionAvailable(update)
          break
        case 'workflow_completed':
          handleWorkflowCompleted(update)
          break
      }
    })
  })

  onUnmounted(() => {
    disconnect()
  })

  function handleStateChange(update: any) {
    // Update local state machine data
    console.log('State changed:', update)
  }

  function handleActionAvailable(update: any) {
    // Notify user of new available actions
    console.log('New action available:', update)
  }

  function handleWorkflowCompleted(update: any) {
    // Handle workflow completion
    console.log('Workflow completed:', update)
  }

  return {
    stateMachineUpdates
  }
}
```

### Bulk Operations

Implement bulk state machine operations:

```typescript
// composables/useBulkStateMachine.ts
export function useBulkStateMachine() {
  const isExecuting = ref(false)
  const results = ref<BulkOperationResult[]>([])

  async function executeBulkAction(
    items: Array<{ id: string; stateMachineId: string }>,
    action: StateMachineAction
  ) {
    isExecuting.value = true
    results.value = []

    try {
      const promises = items.map(async (item) => {
        try {
          const result = await executeStateMachineAction({
            instanceId: item.stateMachineId,
            trigger: action.trigger,
            context: {
              userId: getCurrentUserId(),
              bulkOperation: true
            }
          })

          return {
            itemId: item.id,
            success: true,
            result
          }
        } catch (error) {
          return {
            itemId: item.id,
            success: false,
            error: error.message
          }
        }
      })

      results.value = await Promise.all(promises)

      // Show summary notification
      const successful = results.value.filter(r => r.success).length
      const failed = results.value.length - successful

      showNotification({
        type: successful === results.value.length ? 'success' : 'warning',
        title: 'Bulk Operation Completed',
        message: `${successful} successful, ${failed} failed`
      })

    } finally {
      isExecuting.value = false
    }
  }

  return {
    isExecuting,
    results,
    executeBulkAction
  }
}
```

## Best Practices

### 1. User Experience

#### Responsive Design
- Ensure state machine components work on all device sizes
- Use appropriate touch targets for mobile devices
- Implement progressive disclosure for complex workflows

#### Clear Visual Feedback
- Use consistent colors and icons for different states
- Provide loading states during action execution
- Show clear success/error messages

#### Accessibility
- Ensure proper ARIA labels for screen readers
- Use semantic HTML elements
- Provide keyboard navigation support

### 2. Performance

#### Lazy Loading
- Load state machine data only when needed
- Implement virtual scrolling for large history lists
- Use pagination for bulk operations

#### Caching
- Cache frequently accessed state machine data
- Implement optimistic updates for better UX
- Use service workers for offline support

### 3. Security

#### Permission Validation
- Always validate permissions on both client and server
- Hide actions that users cannot perform
- Implement proper authentication checks

#### Data Sanitization
- Sanitize all user input before sending to server
- Validate action parameters
- Use HTTPS for all communications

### 4. Error Handling

#### Graceful Degradation
- Provide fallback UI when state machine data is unavailable
- Handle network errors gracefully
- Implement retry mechanisms for failed operations

#### User-Friendly Messages
- Translate technical errors into user-friendly messages
- Provide actionable guidance for resolving issues
- Log detailed errors for debugging

## Troubleshooting

### Common Issues

1. **Actions Not Showing**: Check user permissions and state machine configuration
2. **State Not Updating**: Verify WebSocket connections and refresh logic
3. **Performance Issues**: Profile component rendering and optimize data loading
4. **Mobile Issues**: Test responsive design and touch interactions

### Debugging Tools

```typescript
// Debug helper for state machine issues
export function debugStateMachine(stateMachine: StateMachineInstance) {
  console.group('State Machine Debug')
  console.log('Current State:', stateMachine.currentState)
  console.log('Permitted Triggers:', stateMachine.permittedTriggers)
  console.log('Definition:', stateMachine.stateMachineDefinition)
  console.log('History:', stateMachine.history)
  console.groupEnd()
}

// Performance monitoring
export function monitorActionPerformance(action: StateMachineAction) {
  const startTime = performance.now()

  return {
    end: () => {
      const endTime = performance.now()
      console.log(`Action ${action.trigger} took ${endTime - startTime}ms`)
    }
  }
}
```

## Next Steps

- [Main Concept](01-main-concept.md): Review fundamental state machine concepts
- [Data Structure](02-data-structure.md): Understand the core data models
- [Operator Portal Actions](06-operator-portal-actions.md): Learn about admin interface integration
