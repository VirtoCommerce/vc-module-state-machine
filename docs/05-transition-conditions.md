# Transition Conditions

## Overview

Transition conditions are the business rules that determine when a state machine can move from one state to another. They provide the logic layer that enforces business constraints, validates data, checks permissions, and ensures that workflows follow proper procedures. This document covers built-in conditions, custom condition development, and best practices for implementing complex business logic.

## Understanding Conditions

### What Are Transition Conditions?

Transition conditions are boolean expressions that must evaluate to `true` for a transition to be allowed. They act as gatekeepers, ensuring that:

- **Business Rules**: Only valid business operations are performed
- **Data Validation**: Entity data meets required criteria
- **Security**: Users have appropriate permissions
- **Timing**: Operations occur at the right time
- **Dependencies**: Prerequisites are satisfied

### Condition Evaluation Process

When a trigger is fired, the state machine evaluates conditions in this order:

1. **Permission Checks**: Verify user has required permissions
2. **Built-in Conditions**: Evaluate system-defined rules
3. **Custom Conditions**: Execute business-specific logic
4. **Final Validation**: Confirm all conditions pass

If any condition fails, the transition is blocked and an appropriate error message is returned.

## Built-in Condition Types

### Field Conditions

Field conditions compare entity properties against specific values or patterns.

#### Basic Field Comparisons
```json
{
  "type": "FieldCondition",
  "fieldName": "status",
  "operator": "equals",
  "value": "active"
}
```

#### Supported Operators
- **equals**: Exact match comparison
- **notEquals**: Not equal comparison
- **contains**: String contains substring
- **startsWith**: String starts with value
- **endsWith**: String ends with value
- **greaterThan**: Numeric greater than
- **lessThan**: Numeric less than
- **greaterThanOrEqual**: Numeric greater than or equal
- **lessThanOrEqual**: Numeric less than or equal
- **in**: Value is in a list
- **notIn**: Value is not in a list
- **isEmpty**: Field is null or empty
- **isNotEmpty**: Field has a value

#### Complex Field Conditions
```json
{
  "type": "FieldCondition",
  "fieldName": "price",
  "operator": "greaterThan",
  "value": 100,
  "dataType": "decimal"
}
```

### Role Conditions

Role conditions check user permissions and roles.

#### Basic Role Check
```json
{
  "type": "RoleCondition",
  "requiredRole": "ProductManager"
}
```

#### Permission-Based Conditions
```json
{
  "type": "PermissionCondition",
  "requiredPermission": "product:approve"
}
```

#### Multiple Role Conditions
```json
{
  "type": "RoleCondition",
  "requiredRoles": ["ProductManager", "Administrator"],
  "requireAll": false
}
```

### Time Conditions

Time conditions enforce temporal business rules.

#### Business Hours Check
```json
{
  "type": "TimeCondition",
  "timeType": "businessHours",
  "timezone": "UTC",
  "businessStart": "09:00",
  "businessEnd": "17:00"
}
```

#### Date Range Conditions
```json
{
  "type": "DateRangeCondition",
  "startDate": "2024-01-01",
  "endDate": "2024-12-31"
}
```

#### Minimum Age Condition
```json
{
  "type": "AgeCondition",
  "fieldName": "createdDate",
  "minimumAge": "24:00:00",
  "ageUnit": "hours"
}
```

### Composite Conditions

Combine multiple conditions using logical operators.

#### AND Conditions
```json
{
  "type": "AndCondition",
  "conditions": [
    {
      "type": "FieldCondition",
      "fieldName": "status",
      "operator": "equals",
      "value": "draft"
    },
    {
      "type": "RoleCondition",
      "requiredRole": "ProductManager"
    }
  ]
}
```

#### OR Conditions
```json
{
  "type": "OrCondition",
  "conditions": [
    {
      "type": "RoleCondition",
      "requiredRole": "Administrator"
    },
    {
      "type": "RoleCondition",
      "requiredRole": "SuperUser"
    }
  ]
}
```

#### NOT Conditions
```json
{
  "type": "NotCondition",
  "condition": {
    "type": "FieldCondition",
    "fieldName": "isLocked",
    "operator": "equals",
    "value": true
  }
}
```

## Creating Custom Conditions

### Backend Custom Conditions

#### Step 1: Implement IStateMachineCondition

```csharp
public class ProductCategoryCondition : IStateMachineCondition
{
    private readonly ICategoryService _categoryService;

    public ProductCategoryCondition(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public string Type => "ProductCategoryCondition";

    public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
    {
        // Get the product entity
        var productId = context.EntityId;
        var product = await GetProductAsync(productId);

        if (product == null)
            return false;

        // Check if product belongs to allowed categories
        var allowedCategories = GetAllowedCategories(context);
        return allowedCategories.Contains(product.CategoryId);
    }

    private string[] GetAllowedCategories(StateMachineTriggerContext context)
    {
        // Extract allowed categories from condition configuration
        var config = context.ConditionData as JObject;
        return config?["allowedCategories"]?.ToObject<string[]>() ?? new string[0];
    }
}
```

#### Step 2: Register the Condition

```csharp
// In your module's Initialize method
public void Initialize(IServiceCollection serviceCollection)
{
    serviceCollection.AddTransient<IStateMachineCondition, ProductCategoryCondition>();
}
```

#### Step 3: Use in State Machine Definition

```json
{
  "trigger": "Approve",
  "toState": "Approved",
  "condition": {
    "type": "ProductCategoryCondition",
    "allowedCategories": ["electronics", "books", "clothing"]
  }
}
```

### Advanced Custom Condition Example

```csharp
public class VendorApprovalCondition : IStateMachineCondition
{
    private readonly IVendorService _vendorService;
    private readonly IUserService _userService;
    private readonly ISettingsManager _settingsManager;

    public string Type => "VendorApprovalCondition";

    public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
    {
        try
        {
            // Get vendor information
            var vendor = await _vendorService.GetByIdAsync(context.EntityId);
            if (vendor == null) return false;

            // Check minimum requirements
            if (!await CheckMinimumRequirements(vendor))
                return false;

            // Verify user permissions
            if (!await CheckUserPermissions(context.UserId, vendor))
                return false;

            // Check business rules
            if (!await CheckBusinessRules(vendor, context))
                return false;

            return true;
        }
        catch (Exception ex)
        {
            // Log error and fail safely
            _logger.LogError(ex, "Error evaluating vendor approval condition");
            return false;
        }
    }

    private async Task<bool> CheckMinimumRequirements(Vendor vendor)
    {
        // Check required fields
        if (string.IsNullOrEmpty(vendor.CompanyName) ||
            string.IsNullOrEmpty(vendor.ContactEmail) ||
            string.IsNullOrEmpty(vendor.TaxId))
        {
            return false;
        }

        // Check document uploads
        var requiredDocuments = await _settingsManager.GetValueAsync<string[]>(
            "VendorModule.RequiredDocuments");

        foreach (var docType in requiredDocuments)
        {
            if (!vendor.Documents.Any(d => d.Type == docType))
                return false;
        }

        return true;
    }

    private async Task<bool> CheckUserPermissions(string userId, Vendor vendor)
    {
        var user = await _userService.GetByIdAsync(userId);

        // Check if user can approve vendors
        if (!user.HasPermission("vendor:approve"))
            return false;

        // Check if user can approve this specific vendor type
        var vendorType = vendor.Type;
        var requiredPermission = $"vendor:approve:{vendorType}";

        return user.HasPermission(requiredPermission);
    }

    private async Task<bool> CheckBusinessRules(Vendor vendor, StateMachineTriggerContext context)
    {
        // Check for duplicate vendors
        var existingVendor = await _vendorService.FindByTaxIdAsync(vendor.TaxId);
        if (existingVendor != null && existingVendor.Id != vendor.Id)
            return false;

        // Check credit rating if available
        if (vendor.CreditRating.HasValue)
        {
            var minimumRating = await _settingsManager.GetValueAsync<int>(
                "VendorModule.MinimumCreditRating");
            if (vendor.CreditRating < minimumRating)
                return false;
        }

        return true;
    }
}
```

## Registering Entity Types with Conditions

### Example from Marketplace Vendor Module

```csharp
// From vc-module-marketplace-vendor module.js
angular.module('virtoCommerce.marketplaceVendorModule')
    .run(['virtoCommerce.stateMachineModule.stateMachineTypes',
          'virtoCommerce.stateMachineModule.conditionRegistry',
    function (stateMachineTypes, conditionRegistry) {

        // Register the vendor entity type
        stateMachineTypes.addType({
            value: 'Vendor',
            displayName: 'Marketplace Vendor',
            description: 'Vendor approval and lifecycle management',
            icon: 'fa fa-building'
        });

        // Register custom conditions for vendors
        conditionRegistry.registerCondition({
            type: 'VendorTypeCondition',
            displayName: 'Vendor Type Check',
            description: 'Validates vendor type against allowed types',
            configurationTemplate: 'vendor-type-condition.html',
            defaultConfig: {
                allowedTypes: ['standard', 'premium']
            }
        });

        conditionRegistry.registerCondition({
            type: 'VendorDocumentCondition',
            displayName: 'Required Documents Check',
            description: 'Ensures all required documents are uploaded',
            configurationTemplate: 'vendor-document-condition.html'
        });

        conditionRegistry.registerCondition({
            type: 'VendorCreditCondition',
            displayName: 'Credit Rating Check',
            description: 'Validates vendor credit rating meets minimum requirements',
            configurationTemplate: 'vendor-credit-condition.html',
            defaultConfig: {
                minimumRating: 600,
                requireCreditCheck: true
            }
        });
    }]);
```

### Frontend Condition Configuration

#### Condition Configuration Template

```html
<!-- vendor-type-condition.html -->
<div class="form-group">
    <label>Allowed Vendor Types</label>
    <div class="checkbox" ng-repeat="type in availableVendorTypes">
        <label>
            <input type="checkbox"
                   ng-model="condition.config.allowedTypes[type.value]"
                   ng-true-value="type.value"
                   ng-false-value="">
            {{type.displayName}}
        </label>
    </div>
</div>

<div class="form-group">
    <label>Validation Mode</label>
    <select class="form-control" ng-model="condition.config.validationMode">
        <option value="strict">Strict - Must match exactly</option>
        <option value="flexible">Flexible - Allow similar types</option>
    </select>
</div>
```

#### Condition Controller

```javascript
angular.module('virtoCommerce.marketplaceVendorModule')
    .controller('vendorTypeConditionController',
    ['$scope', 'virtoCommerce.marketplaceVendorModule.vendorTypes',
    function ($scope, vendorTypes) {

        $scope.availableVendorTypes = [];

        // Load available vendor types
        vendorTypes.query().$promise.then(function(types) {
            $scope.availableVendorTypes = types;
        });

        // Initialize condition configuration
        if (!$scope.condition.config) {
            $scope.condition.config = {
                allowedTypes: [],
                validationMode: 'strict'
            };
        }

        // Validation function
        $scope.validateCondition = function() {
            if (!$scope.condition.config.allowedTypes ||
                $scope.condition.config.allowedTypes.length === 0) {
                return "At least one vendor type must be selected";
            }
            return null;
        };
    }]);
```

## Condition Context and Data

### StateMachineTriggerContext

The context object provides access to all relevant data for condition evaluation:

```csharp
public class StateMachineTriggerContext
{
    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public string UserId { get; set; }
    public string Trigger { get; set; }
    public string FromState { get; set; }
    public string ToState { get; set; }
    public object EntityData { get; set; }
    public object ConditionData { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Accessing Entity Data

```csharp
public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
{
    // Method 1: Direct entity data access
    var entityData = context.EntityData as JObject;
    var status = entityData?["status"]?.Value<string>();

    // Method 2: Deserialize to specific type
    var product = context.EntityData.ToObject<Product>();

    // Method 3: Load fresh data from repository
    var currentEntity = await _repository.GetByIdAsync(context.EntityId);

    return ValidateEntity(currentEntity);
}
```

### Condition Configuration Data

```csharp
public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
{
    // Access condition configuration
    var config = context.ConditionData as JObject;

    // Extract configuration values
    var threshold = config?["threshold"]?.Value<decimal>() ?? 0;
    var categories = config?["allowedCategories"]?.ToObject<string[]>();
    var strictMode = config?["strictMode"]?.Value<bool>() ?? false;

    // Use configuration in validation logic
    return ValidateWithConfig(context.EntityData, threshold, categories, strictMode);
}
```

## Error Handling and Debugging

### Condition Error Handling

```csharp
public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
{
    try
    {
        // Condition logic here
        return await EvaluateCondition(context);
    }
    catch (ValidationException ex)
    {
        // Log validation errors
        _logger.LogWarning(ex, "Validation failed for condition {ConditionType}", Type);
        return false;
    }
    catch (Exception ex)
    {
        // Log unexpected errors
        _logger.LogError(ex, "Unexpected error in condition {ConditionType}", Type);

        // Fail safely - decide whether to allow or deny
        return false; // Conservative approach
    }
}
```

### Debugging Conditions

#### Enable Debug Logging

```csharp
public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
{
    _logger.LogDebug("Evaluating condition {ConditionType} for entity {EntityId}",
                     Type, context.EntityId);

    var result = await EvaluateCondition(context);

    _logger.LogDebug("Condition {ConditionType} result: {Result}", Type, result);

    return result;
}
```

#### Condition Testing

```csharp
[Test]
public async Task VendorApprovalCondition_ValidVendor_ReturnsTrue()
{
    // Arrange
    var vendor = CreateValidVendor();
    var context = new StateMachineTriggerContext
    {
        EntityId = vendor.Id,
        EntityType = "Vendor",
        UserId = "admin-user-id",
        EntityData = vendor
    };

    var condition = new VendorApprovalCondition(_vendorService, _userService);

    // Act
    var result = await condition.IsSatisfiedByAsync(context);

    // Assert
    Assert.IsTrue(result);
}
```

## Performance Considerations

### Caching Strategies

```csharp
public class CachedVendorCondition : IStateMachineCondition
{
    private readonly IMemoryCache _cache;
    private readonly IVendorService _vendorService;

    public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
    {
        var cacheKey = $"vendor_validation_{context.EntityId}";

        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }

        var result = await EvaluateCondition(context);

        // Cache for 5 minutes
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}
```

### Async Best Practices

```csharp
public async Task<bool> IsSatisfiedByAsync(StateMachineTriggerContext context)
{
    // Use ConfigureAwait(false) for library code
    var entity = await _repository.GetByIdAsync(context.EntityId)
                                 .ConfigureAwait(false);

    // Parallel execution for independent checks
    var tasks = new[]
    {
        CheckDocuments(entity),
        CheckCreditRating(entity),
        CheckBusinessRules(entity)
    };

    var results = await Task.WhenAll(tasks).ConfigureAwait(false);

    return results.All(r => r);
}
```

## Best Practices

### 1. Condition Design Principles

#### Single Responsibility
- Each condition should check one specific business rule
- Avoid complex conditions that validate multiple unrelated aspects
- Use composite conditions to combine simple conditions

#### Fail-Safe Design
- Default to denying transitions when in doubt
- Handle exceptions gracefully
- Provide clear error messages

#### Performance Optimization
- Cache expensive operations
- Use async/await properly
- Minimize database calls

### 2. Configuration Management

#### Externalize Configuration
```json
{
  "VendorApproval": {
    "MinimumCreditRating": 600,
    "RequiredDocuments": ["tax-certificate", "business-license"],
    "ApprovalTimeout": "7.00:00:00"
  }
}
```

#### Environment-Specific Settings
- Use different configurations for development, staging, and production
- Allow runtime configuration updates where appropriate
- Document all configuration options

### 3. Testing Strategies

#### Unit Testing
```csharp
[TestFixture]
public class VendorApprovalConditionTests
{
    [Test]
    public async Task IsSatisfiedBy_ValidVendor_ReturnsTrue()
    {
        // Test implementation
    }

    [Test]
    public async Task IsSatisfiedBy_MissingDocuments_ReturnsFalse()
    {
        // Test implementation
    }

    [Test]
    public async Task IsSatisfiedBy_InsufficientCreditRating_ReturnsFalse()
    {
        // Test implementation
    }
}
```

#### Integration Testing
- Test conditions with real data
- Verify condition interactions
- Test error scenarios

### 4. Documentation

#### Code Documentation
```csharp
/// <summary>
/// Validates that a vendor meets all requirements for approval.
/// </summary>
/// <remarks>
/// This condition checks:
/// - Required documents are uploaded
/// - Credit rating meets minimum threshold
/// - Tax ID is unique
/// - User has appropriate permissions
/// </remarks>
public class VendorApprovalCondition : IStateMachineCondition
{
    // Implementation
}
```

#### Business Rule Documentation
- Document the business logic behind each condition
- Maintain a registry of all custom conditions
- Provide examples of condition usage

## Troubleshooting

### Common Issues

#### Condition Not Executing
1. **Check Registration**: Ensure condition is properly registered in DI container
2. **Verify Type Name**: Condition type name must match exactly
3. **Check Dependencies**: Ensure all required services are available

#### Condition Always Fails
1. **Debug Logging**: Enable debug logging to see evaluation details
2. **Check Context Data**: Verify entity data is properly populated
3. **Validate Configuration**: Ensure condition configuration is correct

#### Performance Issues
1. **Profile Conditions**: Use profiling tools to identify slow conditions
2. **Optimize Queries**: Review database queries for efficiency
3. **Implement Caching**: Cache expensive operations

### Debugging Tools

#### Condition Debugger
```csharp
public class ConditionDebugger
{
    public static async Task<ConditionResult> DebugCondition(
        IStateMachineCondition condition,
        StateMachineTriggerContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await condition.IsSatisfiedByAsync(context);
            stopwatch.Stop();

            return new ConditionResult
            {
                Success = result,
                ExecutionTime = stopwatch.Elapsed,
                ConditionType = condition.Type
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new ConditionResult
            {
                Success = false,
                ExecutionTime = stopwatch.Elapsed,
                ConditionType = condition.Type,
                Error = ex.Message
            };
        }
    }
}
```

## Next Steps

- [Operator Portal Actions](06-operator-portal-actions.md): Learn to integrate state machines with admin interfaces
- [Vendor Portal Actions](07-vendor-portal-actions.md): Implement customer-facing state machine operations
- [Main Concept](01-main-concept.md): Review fundamental state machine concepts
