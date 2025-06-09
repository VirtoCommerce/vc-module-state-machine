# Data Structure

## Overview

The State Machine module uses several core data classes to model and manage state machine workflows. Understanding these data structures is essential for implementing custom logic and integrating with the state machine system.

## Core Data Classes

### StateMachineDefinition

The `StateMachineDefinition` class represents the blueprint for a state machine workflow. It defines the structure, states, and transitions for a specific entity type.

```csharp
public class StateMachineDefinition : AuditableEntity, ICloneable
{
    public string Version { get; set; }
    public string EntityType { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public IList<StateMachineState> States { get; set; }
    public string StatesGraph { get; set; }
    public string StatesCapture { get; set; }
}
```

**Key Properties:**
- **Version**: Version identifier for the state machine definition
- **EntityType**: The type of business entity this state machine applies to (e.g., "Product", "Order")
- **Name**: Human-readable name for the workflow
- **IsActive**: Indicates if this definition is currently active
- **States**: Collection of states that make up the workflow
- **StatesGraph**: JSON representation of the visual state machine layout
- **StatesCapture**: Snapshot of the state machine for versioning

### StateMachineInstance

The `StateMachineInstance` class represents a runtime instance of a state machine bound to a specific business entity.

```csharp
public class StateMachineInstance : AuditableEntity, ICloneable
{
    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public string StateMachineDefinitionId { get; set; }
    public string CurrentStateName { get; set; }
    public StateMachineState CurrentState { get; set; }
    public IEnumerable<string> PermittedTriggers { get; set; }
    public bool IsActive { get; set; }
    public StateMachineDefinition StateMachineDefinition { get; set; }
    public bool IsStopped { get; set; }
}
```

**Key Properties:**
- **EntityId**: Unique identifier of the business entity
- **EntityType**: Type of the business entity
- **StateMachineDefinitionId**: Reference to the state machine definition
- **CurrentStateName**: Name of the current state
- **CurrentState**: Full state object with all properties
- **PermittedTriggers**: List of triggers that can be executed from the current state
- **IsActive**: Indicates if the instance is active (not stopped and not in final state)
- **IsStopped**: Manual stop flag for the state machine

### StateMachineLocalization

The `StateMachineLocalization` class provides multi-language support for state machines.

```csharp
public class StateMachineLocalization : AuditableEntity, ICloneable
{
    public string DefinitionId { get; set; }
    public string Item { get; set; }
    public string Locale { get; set; }
    public string Value { get; set; }
}
```

**Key Properties:**
- **DefinitionId**: Reference to the state machine definition
- **Item**: The item being localized (state name, transition trigger, etc.)
- **Locale**: Language/culture code (e.g., "en-US", "fr-FR")
- **Value**: Localized text value

### StateMachineState

The `StateMachineState` class defines individual states within a state machine.

```csharp
public class StateMachineState : ValueObject
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsFailed { get; set; }
    public object StateData { get; set; }
    public string LocalizedValue { get; set; }
    public IList<StateMachineTransition> Transitions { get; set; }
}
```

## State Attributes

### IsInitial
- **Purpose**: Marks the starting state for new entities
- **Usage**: "Started" state should have `IsInitial = true`
- **Example**: "Draft" state for new products, "New" state for orders

### IsFinal
- **Purpose**: Indicates the workflow has completed
- **Usage**: Entities in final states cannot transition to other states
- **Example**: "Completed", "Cancelled", "Archived"

### IsSuccess
- **Purpose**: Marks successful completion of the workflow
- **Usage**: Used with `IsFinal = true` to indicate positive outcomes
- **Example**: "Approved", "Delivered", "Published"
- **Business Logic**: Often used for reporting and analytics

### IsFailed
- **Purpose**: Marks failed completion of the workflow
- **Usage**: Used with `IsFinal = true` to indicate negative outcomes
- **Example**: "Rejected", "Cancelled", "Failed"
- **Business Logic**: Triggers error handling and notification processes

## Data Relationships

```mermaid
erDiagram
    StateMachineDefinition {
        string Id PK
        string Version
        string EntityType
        string Name
        bool IsActive
        string StatesGraph
        string StatesCapture
        datetime CreatedDate
        string CreatedBy
        datetime ModifiedDate
        string ModifiedBy
    }

    StateMachineInstance {
        string Id PK
        string EntityId
        string EntityType
        string StateMachineDefinitionId FK
        bool IsStopped
        datetime CreatedDate
        string CreatedBy
        datetime ModifiedDate
        string ModifiedBy
    }

    StateMachineLocalization {
        string Id PK
        string DefinitionId FK
        string Item
        string Locale
        string Value
        datetime CreatedDate
        string CreatedBy
        datetime ModifiedDate
        string ModifiedBy
    }

    StateMachineState {
        string Name
        string Type
        string Description
        bool IsInitial
        bool IsFinal
        bool IsSuccess
        bool IsFailed
        object StateData
        string LocalizedValue
    }

    StateMachineTransition {
        string Trigger
        string ToState
        string FromState
        object Condition
    }

    StateMachineDefinition ||--o{ StateMachineInstance : "defines"
    StateMachineDefinition ||--o{ StateMachineLocalization : "localizes"
    StateMachineDefinition ||--o{ StateMachineState : "contains"
    StateMachineState ||--o{ StateMachineTransition : "has"
```

## Usage Examples

### Example 1: Product Approval Workflow

```json
{
  "name": "Product Approval",
  "entityType": "Product",
  "isActive": true,
  "states": [
    {
      "name": "Draft",
      "isInitial": true,
      "isFinal": false,
      "isSuccess": false,
      "isFailed": false,
      "transitions": [
        {
          "trigger": "Submit",
          "toState": "PendingReview"
        }
      ]
    },
    {
      "name": "PendingReview",
      "isInitial": false,
      "isFinal": false,
      "isSuccess": false,
      "isFailed": false,
      "transitions": [
        {
          "trigger": "Approve",
          "toState": "Approved"
        },
        {
          "trigger": "Reject",
          "toState": "Rejected"
        }
      ]
    },
    {
      "name": "Approved",
      "isInitial": false,
      "isFinal": true,
      "isSuccess": true,
      "isFailed": false
    },
    {
      "name": "Rejected",
      "isInitial": false,
      "isFinal": true,
      "isSuccess": false,
      "isFailed": true
    }
  ]
}
```

### Example 2: Using IsSuccess and IsFailed

The `IsSuccess` and `IsFailed` attributes are particularly useful for:

**Reporting and Analytics:**
```csharp
// Get success rate for product approvals
var totalProducts = instances.Count();
var successfulProducts = instances.Count(i => i.CurrentState.IsSuccess);
var failedProducts = instances.Count(i => i.CurrentState.IsFailed);
var successRate = (double)successfulProducts / totalProducts * 100;
```

**Conditional Business Logic:**
```csharp
// Trigger different actions based on outcome
if (currentState.IsSuccess)
{
    // Send success notification
    // Update product status to "Active"
    // Trigger marketing campaigns
}
else if (currentState.IsFailed)
{
    // Send rejection notification
    // Log failure reason
    // Trigger review process
}
```

**Dashboard Widgets:**
```csharp
// Display workflow statistics
var stats = new WorkflowStats
{
    InProgress = instances.Count(i => !i.CurrentState.IsFinal),
    Successful = instances.Count(i => i.CurrentState.IsSuccess),
    Failed = instances.Count(i => i.CurrentState.IsFailed),
    Total = instances.Count()
};
```

## Best Practices

### State Design
1. **Keep states simple**: Each state should represent a clear, distinct condition
2. **Use meaningful names**: State names should be self-explanatory
3. **Plan for localization**: Consider how state names will translate to other languages

### Transition Design
1. **Minimize complexity**: Avoid too many transitions from a single state
2. **Use clear triggers**: Trigger names should indicate the action being performed
3. **Document conditions**: Complex transition conditions should be well-documented

### Data Management
1. **Version control**: Use the Version field to track state machine changes
2. **Audit trails**: Leverage the AuditableEntity base class for change tracking
3. **Performance**: Consider indexing frequently queried fields like EntityType and EntityId

## Next Steps

- [Entity Types](03-entity-types.md): Learn how to register and manage entity types
- [Visual Editor](04-visual-editor.md): Create state machines using the visual interface
- [Transition Conditions](05-transition-conditions.md): Implement business rules and validation logic
