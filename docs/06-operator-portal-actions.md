# State Machine Actions in Operator Portal

## Overview

The Operator Portal (Admin Interface) provides comprehensive state machine management capabilities for administrators and operators. This document explains how to integrate state machine actions into admin interfaces, create custom workflows, and manage entity lifecycles through the administrative portal.

## Integration Architecture

### Admin Portal Structure

The Operator Portal integration follows Virto Commerce's blade-based architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Main Navigation                          │
├─────────────────────────────────────────────────────────────┤
│ Entity List │              Entity Detail                    │
│   Blade     │                Blade                          │
│             │                                               │
│ ┌─────────┐ │ ┌─────────────────────────────────────────┐   │
│ │ Entity  │ │ │           Entity Form                   │   │
│ │ Items   │ │ │                                         │   │
│ │         │ │ ├─────────────────────────────────────────┤   │
│ │ [State] │ │ │        State Machine Widget             │   │
│ └─────────┘ │ │  ┌─────────────┐ ┌─────────────────┐    │   │
│             │ │  │Current State│ │Available Actions│    │   │
│             │ │  └─────────────┘ └─────────────────┘    │   │
│             │ └─────────────────────────────────────────┘   │
└─────────────┴───────────────────────────────────────────────┘
```

### Key Components

1. **Entity List Integration**: Display state information in entity lists
2. **Detail View Widget**: State machine controls in entity detail views
3. **Action Buttons**: Trigger state transitions
4. **State Visualization**: Current state and workflow progress
5. **History Tracking**: Audit trail of state changes

## Implementation Example: Registration Request Details

### Controller Implementation

Based on the registration-request-details.js example:

```javascript
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .controller('virtoCommerce.marketplaceRegistrationModule.registrationRequestDetailsController',
    ['$scope', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
     'virtoCommerce.marketplaceRegistrationModule.registrationRequests',
     'virtoCommerce.stateMachineModule.stateMachineApi',
     'platformWebApp.authService',
    function ($scope, $timeout, bladeNavigationService, dialogService, registrationRequests,
              stateMachineApi, authService) {

        var blade = $scope.blade;
        blade.updatePermission = 'marketplace:registration:update';
        blade.isLoading = false;

        // Initialize the blade
        blade.refresh = function () {
            blade.isLoading = true;

            registrationRequests.get({ id: blade.currentEntityId }, function (data) {
                initializeBlade(data);
                loadStateMachine();
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });
        };

        function initializeBlade(data) {
            blade.currentEntity = angular.copy(data);
            blade.origEntity = data;
            blade.isLoading = false;
            blade.title = data.companyName || 'Registration Request';
        }

        // Load state machine instance for the entity
        function loadStateMachine() {
            if (!blade.currentEntity.id) return;

            stateMachineApi.getInstanceForEntity({
                entityType: 'RegistrationRequest',
                entityId: blade.currentEntity.id
            }).$promise.then(function (stateMachine) {
                blade.stateMachine = stateMachine;
                loadAvailableActions();
            }).catch(function (error) {
                console.error('Failed to load state machine:', error);
            });
        }

        // Load available actions based on current state and user permissions
        function loadAvailableActions() {
            if (!blade.stateMachine) return;

            blade.availableActions = [];

            // Get permitted triggers from state machine
            var permittedTriggers = blade.stateMachine.permittedTriggers || [];

            // Filter actions based on user permissions
            permittedTriggers.forEach(function (trigger) {
                var action = createActionFromTrigger(trigger);
                if (action && hasPermissionForAction(action)) {
                    blade.availableActions.push(action);
                }
            });
        }

        // Create action object from trigger
        function createActionFromTrigger(trigger) {
            var actionMap = {
                'Submit': {
                    name: 'Submit for Review',
                    icon: 'fa fa-paper-plane',
                    cssClass: 'btn-primary',
                    permission: 'marketplace:registration:submit',
                    confirmMessage: 'Submit this registration request for review?'
                },
                'Approve': {
                    name: 'Approve Request',
                    icon: 'fa fa-check-circle',
                    cssClass: 'btn-success',
                    permission: 'marketplace:registration:approve',
                    confirmMessage: 'Approve this registration request?'
                },
                'Reject': {
                    name: 'Reject Request',
                    icon: 'fa fa-times-circle',
                    cssClass: 'btn-danger',
                    permission: 'marketplace:registration:reject',
                    confirmMessage: 'Reject this registration request?',
                    requiresComment: true
                },
                'RequestMoreInfo': {
                    name: 'Request More Information',
                    icon: 'fa fa-question-circle',
                    cssClass: 'btn-warning',
                    permission: 'marketplace:registration:manage',
                    confirmMessage: 'Request additional information?',
                    requiresComment: true
                }
            };

            var action = actionMap[trigger];
            if (action) {
                action.trigger = trigger;
                return action;
            }

            // Default action for unmapped triggers
            return {
                trigger: trigger,
                name: trigger,
                icon: 'fa fa-cog',
                cssClass: 'btn-default',
                permission: 'marketplace:registration:manage'
            };
        }

        // Check if user has permission for action
        function hasPermissionForAction(action) {
            return authService.checkPermission(action.permission);
        }

        // Execute state machine action
        $scope.executeAction = function (action) {
            if (action.requiresComment) {
                showCommentDialog(action);
            } else if (action.confirmMessage) {
                showConfirmDialog(action);
            } else {
                performAction(action);
            }
        };

        // Show confirmation dialog
        function showConfirmDialog(action) {
            var dialog = {
                id: "confirmActionDialog",
                title: "Confirm Action",
                message: action.confirmMessage,
                callback: function (confirmed) {
                    if (confirmed) {
                        performAction(action);
                    }
                }
            };
            dialogService.showConfirmationDialog(dialog);
        }

        // Show comment dialog for actions requiring comments
        function showCommentDialog(action) {
            var newBlade = {
                id: 'actionCommentBlade',
                title: 'Add Comment',
                subtitle: action.name,
                controller: 'actionCommentController',
                template: 'Modules/$(VirtoCommerce.MarketplaceRegistration)/Scripts/blades/action-comment.tpl.html',
                action: action,
                callback: function (comment) {
                    performAction(action, comment);
                }
            };
            bladeNavigationService.showBlade(newBlade, blade);
        }

        // Perform the actual state machine action
        function performAction(action, comment) {
            blade.isLoading = true;

            var context = {
                userId: authService.userName,
                comment: comment,
                timestamp: new Date().toISOString(),
                entityData: blade.currentEntity
            };

            stateMachineApi.fireTrigger({
                instanceId: blade.stateMachine.id,
                trigger: action.trigger,
                context: context
            }).$promise.then(function (result) {
                // Success handling
                blade.refresh(); // Reload the entity and state machine
                showSuccessNotification(action);

                // Execute post-action logic
                executePostActionLogic(action, result);

            }).catch(function (error) {
                // Error handling
                blade.isLoading = false;
                var errorMessage = error.data && error.data.message ?
                                 error.data.message : 'Action failed';
                bladeNavigationService.setError(errorMessage, blade);
            });
        }

        // Show success notification
        function showSuccessNotification(action) {
            var notification = {
                title: 'Action Completed',
                message: action.name + ' completed successfully',
                type: 'success'
            };
            // Assuming notification service is available
            // notificationService.success(notification);
        }

        // Execute additional logic after successful action
        function executePostActionLogic(action, result) {
            switch (action.trigger) {
                case 'Approve':
                    // Trigger vendor account creation
                    createVendorAccount();
                    break;
                case 'Reject':
                    // Send rejection notification
                    sendRejectionNotification();
                    break;
                case 'RequestMoreInfo':
                    // Send information request email
                    sendInfoRequestNotification();
                    break;
            }
        }

        // Additional business logic functions
        function createVendorAccount() {
            // Implementation for vendor account creation
            console.log('Creating vendor account...');
        }

        function sendRejectionNotification() {
            // Implementation for rejection notification
            console.log('Sending rejection notification...');
        }

        function sendInfoRequestNotification() {
            // Implementation for info request notification
            console.log('Sending information request...');
        }

        // Blade toolbar configuration
        blade.toolbarCommands = [
            {
                name: "platform.commands.save",
                icon: 'fa fa-save',
                executeMethod: function () {
                    $scope.saveChanges();
                },
                canExecuteMethod: function () {
                    return isDirty() && blade.hasUpdatePermission();
                },
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.reset",
                icon: 'fa fa-undo',
                executeMethod: function () {
                    angular.copy(blade.origEntity, blade.currentEntity);
                },
                canExecuteMethod: function () {
                    return isDirty();
                }
            }
        ];

        // Helper functions
        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        }

        $scope.saveChanges = function () {
            blade.isLoading = true;

            registrationRequests.update(blade.currentEntity, function (data) {
                blade.refresh();
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });
        };

        // Initialize blade
        blade.refresh();
    }]);
```

### Template Implementation

```html
<!-- registration-request-details.tpl.html -->
<div class="blade-content">
    <div class="blade-inner">
        <div class="inner-block">

            <!-- Entity Information Form -->
            <form class="form-horizontal" name="detailForm">

                <!-- Basic Information -->
                <fieldset>
                    <legend>Basic Information</legend>

                    <div class="form-group">
                        <label class="col-md-2 control-label">Company Name</label>
                        <div class="col-md-10">
                            <input class="form-control" ng-model="blade.currentEntity.companyName"
                                   ng-readonly="!blade.hasUpdatePermission()" required>
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="col-md-2 control-label">Contact Email</label>
                        <div class="col-md-10">
                            <input type="email" class="form-control"
                                   ng-model="blade.currentEntity.contactEmail"
                                   ng-readonly="!blade.hasUpdatePermission()" required>
                        </div>
                    </div>

                    <!-- Additional form fields... -->
                </fieldset>

                <!-- State Machine Widget -->
                <fieldset ng-if="blade.stateMachine">
                    <legend>Workflow Status</legend>

                    <!-- Current State Display -->
                    <div class="form-group">
                        <label class="col-md-2 control-label">Current State</label>
                        <div class="col-md-10">
                            <div class="state-indicator">
                                <span class="label" ng-class="getStateClass(blade.stateMachine.currentState)">
                                    <i class="fa" ng-class="getStateIcon(blade.stateMachine.currentState)"></i>
                                    {{blade.stateMachine.currentState.localizedValue || blade.stateMachine.currentState.name}}
                                </span>

                                <!-- State Description -->
                                <p class="help-block" ng-if="blade.stateMachine.currentState.description">
                                    {{blade.stateMachine.currentState.description}}
                                </p>
                            </div>
                        </div>
                    </div>

                    <!-- Available Actions -->
                    <div class="form-group" ng-if="blade.availableActions.length > 0">
                        <label class="col-md-2 control-label">Available Actions</label>
                        <div class="col-md-10">
                            <div class="btn-group" role="group">
                                <button type="button"
                                        class="btn"
                                        ng-class="action.cssClass || 'btn-default'"
                                        ng-repeat="action in blade.availableActions"
                                        ng-click="executeAction(action)"
                                        ng-disabled="blade.isLoading">
                                    <i class="fa" ng-class="action.icon"></i>
                                    {{action.name}}
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- Workflow Progress Visualization -->
                    <div class="form-group">
                        <label class="col-md-2 control-label">Progress</label>
                        <div class="col-md-10">
                            <div class="workflow-progress">
                                <div class="progress-step"
                                     ng-repeat="state in blade.stateMachine.stateMachineDefinition.states"
                                     ng-class="getProgressStepClass(state)">
                                    <div class="step-indicator">
                                        <i class="fa" ng-class="getStateIcon(state)"></i>
                                    </div>
                                    <div class="step-label">{{state.name}}</div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- State History -->
                    <div class="form-group" ng-if="blade.stateMachine.history">
                        <label class="col-md-2 control-label">History</label>
                        <div class="col-md-10">
                            <div class="state-history">
                                <div class="history-item" ng-repeat="item in blade.stateMachine.history">
                                    <div class="history-timestamp">
                                        {{item.timestamp | date:'medium'}}
                                    </div>
                                    <div class="history-action">
                                        <strong>{{item.trigger}}</strong> by {{item.userName}}
                                    </div>
                                    <div class="history-transition">
                                        {{item.fromState}} → {{item.toState}}
                                    </div>
                                    <div class="history-comment" ng-if="item.comment">
                                        <em>{{item.comment}}</em>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </fieldset>
            </form>
        </div>
    </div>
</div>
```

### CSS Styling

```css
/* State Machine Widget Styles */
.state-indicator {
    margin-bottom: 10px;
}

.state-indicator .label {
    font-size: 14px;
    padding: 8px 12px;
    margin-right: 10px;
}

.label-success { background-color: #5cb85c; }
.label-warning { background-color: #f0ad4e; }
.label-danger { background-color: #d9534f; }
.label-info { background-color: #5bc0de; }
.label-default { background-color: #777; }

/* Workflow Progress Styles */
.workflow-progress {
    display: flex;
    align-items: center;
    margin: 20px 0;
}

.progress-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    flex: 1;
    position: relative;
}

.progress-step:not(:last-child)::after {
    content: '';
    position: absolute;
    top: 20px;
    right: -50%;
    width: 100%;
    height: 2px;
    background-color: #ddd;
    z-index: 1;
}

.progress-step.completed::after {
    background-color: #5cb85c;
}

.progress-step.current::after {
    background-color: #f0ad4e;
}

.step-indicator {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background-color: #ddd;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 8px;
    z-index: 2;
    position: relative;
}

.progress-step.completed .step-indicator {
    background-color: #5cb85c;
    color: white;
}

.progress-step.current .step-indicator {
    background-color: #f0ad4e;
    color: white;
}

.step-label {
    font-size: 12px;
    text-align: center;
    max-width: 80px;
    word-wrap: break-word;
}

/* State History Styles */
.state-history {
    max-height: 300px;
    overflow-y: auto;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 10px;
}

.history-item {
    padding: 10px;
    border-bottom: 1px solid #eee;
    margin-bottom: 10px;
}

.history-item:last-child {
    border-bottom: none;
    margin-bottom: 0;
}

.history-timestamp {
    font-size: 12px;
    color: #666;
    margin-bottom: 5px;
}

.history-action {
    margin-bottom: 3px;
}

.history-transition {
    font-size: 14px;
    color: #333;
    margin-bottom: 5px;
}

.history-comment {
    font-size: 13px;
    color: #666;
    font-style: italic;
}
```

### Helper Functions

```javascript
// Add these functions to the controller scope

// Get CSS class for state display
$scope.getStateClass = function(state) {
    if (!state) return 'label-default';

    if (state.isSuccess) return 'label-success';
    if (state.isFailed) return 'label-danger';
    if (state.isFinal) return 'label-info';
    if (state.isInitial) return 'label-default';

    return 'label-warning'; // In-progress states
};

// Get icon for state
$scope.getStateIcon = function(state) {
    if (!state) return 'fa-circle';

    if (state.isSuccess) return 'fa-check-circle';
    if (state.isFailed) return 'fa-times-circle';
    if (state.isFinal) return 'fa-flag-checkered';
    if (state.isInitial) return 'fa-play-circle';

    return 'fa-clock-o'; // In-progress states
};

// Get progress step class
$scope.getProgressStepClass = function(state) {
    if (!blade.stateMachine) return '';

    var currentStateName = blade.stateMachine.currentState.name;
    var states = blade.stateMachine.stateMachineDefinition.states;

    // Find the index of states
    var currentIndex = states.findIndex(s => s.name === currentStateName);
    var stateIndex = states.findIndex(s => s.name === state.name);

    if (stateIndex < currentIndex) return 'completed';
    if (stateIndex === currentIndex) return 'current';

    return 'pending';
};
```

## Advanced Features

### Bulk Operations

For managing multiple entities with state machines:

```javascript
// Bulk action controller
$scope.executeBulkAction = function(action, selectedItems) {
    var promises = selectedItems.map(function(item) {
        return stateMachineApi.fireTrigger({
            instanceId: item.stateMachine.id,
            trigger: action.trigger,
            context: {
                userId: authService.userName,
                bulkOperation: true
            }
        }).$promise;
    });

    Promise.all(promises).then(function(results) {
        // Handle bulk operation results
        var successful = results.filter(r => r.success).length;
        var failed = results.length - successful;

        showBulkOperationResults(successful, failed);
        blade.refresh();
    });
};
```

### Custom Action Dialogs

For complex actions requiring additional input:

```javascript
// Custom action dialog controller
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .controller('actionCommentController',
    ['$scope', 'platformWebApp.bladeNavigationService',
    function ($scope, bladeNavigationService) {

        var blade = $scope.blade;
        blade.isLoading = false;

        $scope.comment = {
            text: '',
            isRequired: blade.action.requiresComment
        };

        $scope.canSave = function() {
            return !blade.action.requiresComment ||
                   ($scope.comment.text && $scope.comment.text.trim().length > 0);
        };

        $scope.save = function() {
            if (blade.callback) {
                blade.callback($scope.comment.text);
            }
            bladeNavigationService.closeBlade(blade);
        };

        $scope.cancel = function() {
            bladeNavigationService.closeBlade(blade);
        };
    }]);
```

### State Machine Dashboard

Create a dashboard widget for state machine statistics:

```javascript
// Dashboard widget controller
angular.module('virtoCommerce.stateMachineModule')
    .controller('stateMachineDashboardController',
    ['$scope', 'virtoCommerce.stateMachineModule.stateMachineApi',
    function ($scope, stateMachineApi) {

        $scope.statistics = {};
        $scope.isLoading = true;

        function loadStatistics() {
            stateMachineApi.getStatistics({
                entityType: $scope.entityType,
                dateRange: $scope.dateRange
            }).$promise.then(function(stats) {
                $scope.statistics = stats;
                $scope.isLoading = false;

                // Prepare chart data
                prepareChartData(stats);
            });
        }

        function prepareChartData(stats) {
            // Implementation for chart data preparation
            $scope.chartData = {
                labels: stats.states.map(s => s.name),
                datasets: [{
                    data: stats.states.map(s => s.count),
                    backgroundColor: stats.states.map(s => getStateColor(s))
                }]
            };
        }

        loadStatistics();
    }]);
```

## Best Practices

### 1. User Experience

#### Clear State Visualization
- Use consistent colors and icons for states
- Provide clear descriptions of what each state means
- Show progress through the workflow

#### Intuitive Actions
- Use descriptive action names
- Group related actions together
- Provide confirmation for destructive actions

#### Error Handling
- Show clear error messages
- Provide guidance on how to resolve issues
- Log errors for debugging

### 2. Performance

#### Lazy Loading
- Load state machine data only when needed
- Cache frequently accessed data
- Use pagination for large datasets

#### Efficient Updates
- Update only changed data
- Use optimistic updates where appropriate
- Batch multiple operations

### 3. Security

#### Permission Checks
- Verify permissions before showing actions
- Re-check permissions before executing actions
- Log all state machine operations

#### Data Validation
- Validate all input data
- Sanitize user input
- Use HTTPS for all communications

### 4. Maintainability

#### Modular Code
- Separate concerns into different controllers
- Use reusable components
- Follow consistent naming conventions

#### Documentation
- Document all custom actions
- Provide examples of usage
- Maintain API documentation

## Troubleshooting

### Common Issues

1. **Actions Not Appearing**: Check user permissions and state machine configuration
2. **Actions Failing**: Verify condition logic and entity data
3. **State Not Updating**: Check for caching issues and refresh logic
4. **Performance Issues**: Profile database queries and optimize conditions

### Debugging Tips

```javascript
// Enable debug mode
$scope.debugMode = true;

// Log state machine data
console.log('State Machine:', blade.stateMachine);
console.log('Available Actions:', blade.availableActions);
console.log('Current User Permissions:', authService.permissions);

// Test action execution
$scope.testAction = function(action) {
    console.log('Testing action:', action);
    // Perform dry run without actual execution
};
```

## Next Steps

- [Vendor Portal Actions](vendor-portal-actions.md): Learn about customer-facing state machine integration
- [Transition Conditions](transition-conditions.md): Understand business rule implementation
- [Entity Types](entity-types.md): Review entity type registration and management
