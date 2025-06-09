# Entity Types in State Machines

## Overview

Entity types are the foundation of the State Machine module, allowing you to bind state machine workflows to any business entity in your system. This document explains how to register entity types, create custom actions, and integrate state machines with your business logic.

## Entity Type Registration

### Understanding Entity Types

An entity type represents a category of business objects that can be managed by state machines. Examples include:
- **Products**: Manage product approval workflows
- **Orders**: Control order processing states
- **Vendors**: Handle vendor registration and approval
- **Content**: Manage content publication workflows

### Registration Services

The module provides two key services for entity type management:

#### stateMachineTypes.js
This service manages the registry of available entity types:

```javascript
angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineTypes', [function () {
        var registeredTypes = [];

        return {
            addType: function (entityType) {
                registeredTypes.push(entityType);
            },
            getAllTypes: function () {
                return registeredTypes;
            },
            getTypeInfo: function (type) {
                return registeredTypes.find(x => x.value === type);
            }
        };
    }]);
```

#### stateMachineRegistrar.js
This service manages custom state actions:

```javascript
angular.module('virtoCommerce.stateMachineModule')
    .factory('virtoCommerce.stateMachineModule.stateMachineRegistrar', [function () {
        var registeredStates = {};

        return {
            registerStateAction: function (stateName, action) {
                registeredStates[stateName] = action;
            },
            getStateAction: function (stateName) {
                return registeredStates[stateName];
            }
        };
    }]);
```

## How to Register Entity Types

### Step 1: Basic Entity Type Registration

Register your entity type in your module's main JavaScript file:

```javascript
// Example from vc-module-marketplace-registration
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineTypes', function (stateMachineTypes) {
        // Register the RegistrationRequest entity type
        stateMachineTypes.addType({
            value: 'RegistrationRequest',
            displayName: 'Registration Request',
            description: 'Vendor registration approval workflow',
            icon: 'fa fa-user-plus',
            detailBlade: 'virtoCommerce.marketplaceRegistrationModule.registrationRequestDetails'
        });
    }]);
```

### Step 2: Entity Type Configuration

Define the entity type configuration object:

```javascript
var entityTypeConfig = {
    value: 'YourEntityType',           // Unique identifier
    displayName: 'Your Entity Type',   // Human-readable name
    description: 'Description of the workflow',
    icon: 'fa fa-icon-name',          // FontAwesome icon
    detailBlade: 'your.module.detailBlade',  // Detail view blade
    listBlade: 'your.module.listBlade',      // List view blade (optional)
    apiEndpoint: '/api/your-entity',          // API endpoint (optional)
    permissions: ['your:entity:read']         // Required permissions (optional)
};
```

## Creating Custom Frontend Actions

### Step 3: Register Custom Actions

Custom actions allow you to implement specific business logic when state transitions occur:

```javascript
// Example from vc-module-marketplace-registration
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineRegistrar', function (stateMachineRegistrar) {

        // Register custom action for approval state
        stateMachineRegistrar.registerStateAction('Approved', function(entity, stateMachine) {
            // Custom logic for approved registration requests
            return {
                name: 'Create Vendor Account',
                icon: 'fa fa-check-circle',
                executeMethod: function() {
                    // Call API to create vendor account
                    return vendorService.createFromRegistration(entity.id);
                },
                permission: 'marketplace:registration:approve',
                condition: function() {
                    // Only show if user has permission and entity is in correct state
                    return entity.currentState === 'Approved' &&
                           authService.checkPermission('marketplace:registration:approve');
                }
            };
        });

        // Register custom action for rejection state
        stateMachineRegistrar.registerStateAction('Rejected', function(entity, stateMachine) {
            return {
                name: 'Send Rejection Notice',
                icon: 'fa fa-times-circle',
                executeMethod: function() {
                    return notificationService.sendRejectionEmail(entity.id);
                },
                permission: 'marketplace:registration:manage'
            };
        });
    }]);
```

### Action Configuration Properties

```javascript
{
    name: 'Action Name',              // Display name for the action
    icon: 'fa fa-icon',              // FontAwesome icon class
    executeMethod: function() {       // Function to execute the action
        // Return a promise for async operations
        return apiService.performAction();
    },
    permission: 'permission:name',    // Required permission (optional)
    condition: function() {           // Condition to show/hide action (optional)
        return true; // or false
    },
    confirmMessage: 'Are you sure?',  // Confirmation dialog (optional)
    successMessage: 'Action completed', // Success notification (optional)
    errorMessage: 'Action failed'     // Error notification (optional)
}
```

## Using Entity Types

### Frontend Integration

Once registered, entity types can be used throughout the Virto Commerce platform:

#### 1. State Machine Management Interface

The admin interface automatically discovers registered entity types and displays them in:
- State machine definition creation
- Entity type selection dropdowns
- Workflow configuration screens

#### 2. Entity Detail Views

Integrate state machine controls into your entity detail views:

```javascript
// Example from registration-request-details.js
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .controller('virtoCommerce.marketplaceRegistrationModule.registrationRequestDetailsController',
    ['$scope', 'virtoCommerce.stateMachineModule.stateMachineApi', function ($scope, stateMachineApi) {

        // Load state machine instance for the entity
        $scope.loadStateMachine = function() {
            stateMachineApi.getInstanceForEntity({
                entityType: 'RegistrationRequest',
                entityId: $scope.blade.currentEntity.id
            }).$promise.then(function(result) {
                $scope.stateMachineInstance = result;
                $scope.availableActions = result.permittedTriggers;
            });
        };

        // Execute state machine action
        $scope.executeAction = function(trigger) {
            stateMachineApi.fireTrigger({
                instanceId: $scope.stateMachineInstance.id,
                trigger: trigger,
                context: {
                    userId: $scope.currentUser.id,
                    entityData: $scope.blade.currentEntity
                }
            }).$promise.then(function() {
                $scope.loadStateMachine(); // Reload to get updated state
                $scope.blade.refresh();    // Refresh the entity
            });
        };
    }]);
```

#### 3. List Views with State Information

Display state information in entity list views:

```html
<!-- Example list template -->
<div class="table-responsive">
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Current State</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            <tr ng-repeat="item in blade.currentEntities">
                <td>{{item.name}}</td>
                <td>{{item.status}}</td>
                <td>
                    <span class="label" ng-class="getStateClass(item.stateMachine.currentState)">
                        {{item.stateMachine.currentState.localizedValue || item.stateMachine.currentState.name}}
                    </span>
                </td>
                <td>
                    <button ng-repeat="action in item.stateMachine.permittedTriggers"
                            class="btn btn-sm btn-default"
                            ng-click="executeAction(item, action)">
                        {{action}}
                    </button>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

### Backend Integration

#### Entity Service Integration

Integrate state machine operations into your entity services:

```csharp
public class RegistrationRequestService : IRegistrationRequestService
{
    private readonly IStateMachineInstanceService _stateMachineService;

    public async Task<RegistrationRequest> CreateAsync(RegistrationRequest request)
    {
        // Create the entity
        var result = await _repository.AddAsync(request);

        // Initialize state machine
        await _stateMachineService.InitializeForEntityAsync(
            entityType: "RegistrationRequest",
            entityId: result.Id,
            initialData: new { UserId = request.UserId }
        );

        return result;
    }

    public async Task<bool> ApproveAsync(string requestId, string userId)
    {
        // Execute state machine transition
        var success = await _stateMachineService.FireTriggerAsync(
            entityType: "RegistrationRequest",
            entityId: requestId,
            trigger: "Approve",
            context: new { UserId = userId }
        );

        if (success)
        {
            // Perform additional business logic
            await CreateVendorAccountAsync(requestId);
        }

        return success;
    }
}
```

## Advanced Entity Type Features

### Conditional Registration

Register entity types conditionally based on module availability or configuration:

```javascript
angular.module('yourModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineTypes', 'platformWebApp.settings',
    function (stateMachineTypes, settings) {

        // Only register if feature is enabled
        settings.getValues({ id: 'YourModule.EnableWorkflows' }, function(data) {
            if (data && data[0] && data[0].value === 'true') {
                stateMachineTypes.addType({
                    value: 'YourEntityType',
                    displayName: 'Your Entity Type',
                    // ... other properties
                });
            }
        });
    }]);
```

### Multi-Workflow Support

Support multiple workflows for the same entity type:

```javascript
// Register multiple workflows for products
stateMachineTypes.addType({
    value: 'Product',
    displayName: 'Product Approval',
    workflowType: 'approval'
});

stateMachineTypes.addType({
    value: 'Product',
    displayName: 'Product Lifecycle',
    workflowType: 'lifecycle'
});
```

### Dynamic Action Registration

Register actions dynamically based on runtime conditions:

```javascript
// Register actions based on user roles
angular.module('yourModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineRegistrar', 'platformWebApp.authService',
    function (stateMachineRegistrar, authService) {

        if (authService.checkPermission('advanced:actions')) {
            stateMachineRegistrar.registerStateAction('AdvancedState', function(entity) {
                return {
                    name: 'Advanced Action',
                    executeMethod: function() {
                        return advancedService.performAction(entity.id);
                    }
                };
            });
        }
    }]);
```

## Best Practices

### 1. Naming Conventions
- Use PascalCase for entity type values: `RegistrationRequest`, `ProductApproval`
- Use descriptive display names: "Vendor Registration Request", "Product Approval Workflow"
- Keep trigger names consistent: `Submit`, `Approve`, `Reject`, `Cancel`

### 2. Error Handling
```javascript
$scope.executeAction = function(trigger) {
    $scope.loading = true;

    stateMachineApi.fireTrigger({
        instanceId: $scope.stateMachineInstance.id,
        trigger: trigger
    }).$promise.then(function(result) {
        // Success handling
        notificationService.success('Action completed successfully');
        $scope.loadStateMachine();
    }).catch(function(error) {
        // Error handling
        notificationService.error('Action failed: ' + error.data.message);
    }).finally(function() {
        $scope.loading = false;
    });
};
```

### 3. Performance Optimization
- Cache state machine instances when possible
- Use lazy loading for entity lists with state information
- Implement pagination for large datasets

### 4. Security Considerations
- Always validate permissions before executing actions
- Sanitize user input in custom actions
- Use HTTPS for all state machine API calls

## Troubleshooting

### Common Issues

1. **Entity Type Not Appearing**: Ensure the registration code runs after the state machine module is loaded
2. **Actions Not Working**: Check that permissions are correctly configured
3. **State Not Updating**: Verify that the entity ID and type match exactly

### Debugging Tips

```javascript
// Enable debug logging
angular.module('yourModule')
    .run(['$log', function($log) {
        $log.debug('Registering entity type: YourEntityType');
    }]);

// Check registered types
console.log(stateMachineTypes.getAllTypes());

// Verify action registration
console.log(stateMachineRegistrar.getStateAction('YourState'));
```

## Next Steps

- [Visual Editor](visual-editor.md): Learn to create state machines using the visual interface
- [Transition Conditions](transition-conditions.md): Implement complex business rules
- [Operator Portal Actions](operator-portal-actions.md): Integrate with admin interfaces
