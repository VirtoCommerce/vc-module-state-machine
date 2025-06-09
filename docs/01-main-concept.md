# Main Concept

## Overview

The Virto Commerce State Machine module provides a comprehensive framework for modeling and managing complex business processes through configurable finite state machines. This module enables organizations to define, visualize, and control the lifecycle of any business entity by establishing clear states, transitions, and business rules.

## What is a State Machine?

A **state machine** (also known as a finite state machine or FSM) is a computational model used to design algorithms and model complex behaviors. In business contexts, state machines help manage the lifecycle of entities by defining:

- **States**: Specific conditions or situations an entity can be in
- **Transitions**: Rules for moving from one state to another
- **Triggers**: Events or actions that initiate transitions
- **Conditions**: Business rules that determine if a transition can occur

### Real-World Example: Order Processing

Consider an e-commerce order that moves through these states:
1. **New** → Customer places order
2. **Paid** → Payment is processed
3. **Shipped** → Order is dispatched
4. **Delivered** → Customer receives order
5. **Completed** → Process finished

Each transition has rules (e.g., "can only ship after payment") and may trigger actions (e.g., "send notification email").

## State Machine Components

### States
States represent the current condition of an entity. Each state has several attributes:

- **Name**: Human-readable identifier
- **IsInitial**: Marks the starting state for new entities
- **IsFinal**: Indicates the workflow has completed
- **IsSuccess**: Marks successful completion (for final states)
- **IsFailed**: Marks failed completion (for final states)

### Transitions
Transitions define how entities move between states:
- **Trigger**: The event name that initiates the transition
- **From State**: Source state
- **To State**: Destination state
- **Conditions**: Optional business rules that must be satisfied

### Conditions
Business rules that control when transitions can occur:
- **Built-in Conditions**: Pre-defined rules for common scenarios
- **Custom Conditions**: User-defined logic specific to business needs
- **Expression-based**: Use simple expressions or complex code

## Module Architecture

The State Machine module follows Virto Commerce's modular architecture:

```
┌─────────────────────────────────────┐
│           Frontend Layer            │
│  ┌─────────────────┐ ┌─────────────┐│
│  │ Operator Portal │ │Vendor Portal││
│  └─────────────────┘ └─────────────┘│
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│            Web API Layer            │
│     REST endpoints for CRUD and     │
│     state machine operations        │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│          Business Logic             │
│  ┌──────────────┐ ┌──────────────┐  │
│  │ State Engine │ │ Condition    │  │
│  │              │ │ Processor    │  │
│  └──────────────┘ └──────────────┘  │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│            Data Layer               │
│    Entity Framework, Repository     │
│         Pattern, Database           │
└─────────────────────────────────────┘
```

## Key Benefits

### 1. **Business Process Clarity**
- Visual representation of complex workflows
- Clear documentation of business rules
- Consistent process execution across the organization

### 2. **Flexibility and Maintainability**
- No-code/low-code workflow design
- Easy modification of business rules
- Version control for process changes

### 3. **Integration Ready**
- RESTful API for external system integration
- Event-driven architecture for real-time updates
- Webhook support for process notifications

### 4. **Audit and Compliance**
- Complete history of state changes
- User tracking for all transitions
- Configurable approval workflows

### 5. **Multi-language Support**
- Localized state and transition names
- International business process support
- Cultural adaptation of workflows

## Core Mechanisms

### State Machine Engine
The engine is built on the **Stateless** library, providing:
- Thread-safe state management
- Hierarchical state machines
- Guard conditions and entry/exit actions

### Entity Binding
Any business entity can be bound to a state machine:
```csharp
// Example: Binding products to approval workflow
stateMachineRegistrar.RegisterEntityType(new StateMachineEntityType
{
    EntityTypeName = "Product",
    DisplayName = "Product Approval",
    WorkflowName = "ProductApprovalWorkflow"
});
```

### Condition Framework
Extensible condition system supports:
- **Field Conditions**: Compare entity properties
- **Role Conditions**: Check user permissions
- **Time Conditions**: Schedule-based rules
- **Custom Conditions**: Domain-specific logic

## Use Cases

### E-commerce
- Order processing workflows
- Product approval processes
- Customer onboarding
- Return/refund management

### B2B Marketplaces
- Vendor registration approval
- Product catalog management
- Contract negotiation workflows
- Supplier onboarding

### Content Management
- Content approval workflows
- Publication processes
- Review and moderation
- Multi-stage content lifecycle

### Healthcare
- Patient care pathways
- Treatment approval processes
- Medical device certification
- Regulatory compliance workflows

## Getting Started

1. **Define Your Process**: Identify states and transitions for your business entity
2. **Register Entity Type**: Bind your entity to the state machine system
3. **Create State Machine**: Use the visual editor to design your workflow
4. **Configure Conditions**: Set up business rules and validation logic
5. **Integrate Frontend**: Implement state actions in your user interfaces
6. **Test and Deploy**: Validate your workflow with real data

## Next Steps

- [Data Structure](data-structure.md): Learn about the core data models
- [Entity Types](entity-types.md): Understand entity registration and management
- [Visual Editor](visual-editor.md): Master the state machine design interface
- [Transition Conditions](transition-conditions.md): Implement business rules and logic
