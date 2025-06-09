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

### Step 1: Register your entity type in your module's main JavaScript file:

```javascript
// Example from vc-module-marketplace-registration
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineTypes', function (stateMachineTypes) {
        // Register the RegistrationRequest entity type
        stateMachineTypes.addType({
            caption: 'marketplaceRegistration.state-machine-entity-types.registration-request',
            value: 'VirtoCommerce.MarketplaceRegistrationModule.Core.Models.RegistrationRequest'

        });
    }]);
```

## Creating custom Operator-Portal frontend actions

### Step 2: Register Custom Actions

Custom actions allow you to implement specific business logic when state transitions occur:

```javascript
// Example from vc-module-marketplace-registration
angular.module('virtoCommerce.marketplaceRegistrationModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineRegistrar', function (stateMachineRegistrar) {
        stateMachineRegistrar.registerStateAction('CompleteRegistrationRequest', {
            callbackFn: function (blade, successCallback) {
                var foundMetaFields = metaFormsService.getMetaFields('SellerAdd');
                var createSellerCommand = {
                    sellerName: blade.currentEntity.organizationName,
                    ownerDetails: {
                        firstName: blade.currentEntity.firstName,
                        lastName: blade.currentEntity.lastName,
                        email: blade.currentEntity.contactEmail
                    }
                };
                var newBlade = {
                    id: 'registrationRequestComplete',
                    command: createSellerCommand,
                    title: 'marketplace.blades.seller-add.title',
                    subtitle: 'marketplace.blades.seller-add.subtitle',
                    controller: 'virtoCommerce.marketplaceModule.sellerAddController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceVendor)/Scripts/blades/seller-add.tpl.html',
                    metaFields: foundMetaFields,
                    successCallback: successCallback
                };
                blade.childBlade = newBlade;
                bladeNavigationService.showBlade(newBlade, blade);
            }
        });
    }]);
```

## Best Practices

### 1. Naming Conventions
- Use PascalCase for entity type values: `RegistrationRequest`, `ProductApproval`
- Use descriptive display names: "Vendor Registration Request", "Product Approval Workflow"
- Keep trigger names consistent: `SubmitTrigger`, `ApproveTrigger`, `RejectTrigger`, `CancelTrigger`

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

- [Visual Editor](04-visual-editor.md): Learn to create state machines using the visual interface
- [Transition Conditions](05-transition-conditions.md): Implement complex business rules
- [Operator Portal Actions](06-operator-portal-actions.md): Integrate with admin interfaces
