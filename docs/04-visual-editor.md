# State Machine Visual Editor

## Overview

The State Machine Visual Editor is an intuitive drag-and-drop interface that allows you to create, edit, and manage state machine workflows without writing code. This powerful tool provides a graphical representation of your business processes, making it easy to design complex workflows and understand their flow at a glance.

## Accessing the Visual Editor

### Navigation Path
1. Log into the Virto Commerce Admin Portal
2. Navigate to **Configuration** → **State Machines**
3. Click **Add New** to create a new state machine or select an existing one to edit
4. The Visual Editor opens automatically in the state machine detail view

### Prerequisites
- Administrative access to the Virto Commerce platform
- Appropriate permissions for state machine management
- A registered entity type (see [Entity Types](entity-types.md) documentation)

## Editor Interface Overview

The Visual Editor consists of several key areas:

```
┌─────────────────────────────────────────────────────────────┐
│                    Toolbar & Actions                        │
├─────────────────────────────────────────────────────────────┤
│ Toolbox │                                                   │
│         │              Canvas Area                          │
│ States  │                                                   │
│ Tools   │         (Drag & Drop Interface)                   │
│         │                                                   │
├─────────┼───────────────────────────────────────────────────┤
│         │              Properties Panel                     │
│         │                                                   │
└─────────┴───────────────────────────────────────────────────┘
```

### Toolbar & Actions
- **Save**: Save the current state machine definition
- **Validate**: Check for errors and inconsistencies
- **Preview**: Test the state machine flow
- **Export**: Export the definition as JSON
- **Import**: Import a state machine from JSON
- **Undo/Redo**: Navigate through edit history

### Toolbox
- **State Tool**: Add new states to the workflow
- **Transition Tool**: Create connections between states
- **Selection Tool**: Select and move elements
- **Delete Tool**: Remove states or transitions

### Canvas Area
- **Grid**: Visual grid for alignment
- **Zoom Controls**: Zoom in/out for better visibility
- **Pan**: Navigate large state machines
- **Auto-Layout**: Automatically arrange states

### Properties Panel
- **State Properties**: Configure selected state attributes
- **Transition Properties**: Set up transition rules and conditions
- **General Settings**: Overall state machine configuration

## Creating Your First State Machine

### Step 1: Basic Setup

1. **Create New State Machine**
   - Click **Add New** in the state machines list
   - Enter a descriptive name (e.g., "Product Approval Workflow")
   - Select the entity type from the dropdown
   - Choose whether to start with a template or blank canvas

2. **Configure General Settings**
   ```
   Name: Product Approval Workflow
   Entity Type: Product
   Version: 1.0
   Description: Manages product approval process from draft to published
   ```

### Step 2: Adding States

1. **Select State Tool** from the toolbox
2. **Click on Canvas** where you want to place the state
3. **Configure State Properties** in the properties panel:

   ```
   State Name: Draft
   Display Name: Draft Product
   Description: Product is in draft mode
   ☑ Is Initial State
   ☐ Is Final State
   ☐ Is Success State
   ☐ Is Failed State
   ```

4. **Add Additional States** following the same process:
   - **Pending Review**: Intermediate state for review process
   - **Approved**: Final success state
   - **Rejected**: Final failure state

### Step 3: Creating Transitions

1. **Select Transition Tool** from the toolbox
2. **Click and Drag** from source state to target state
3. **Configure Transition Properties**:

   ```
   Trigger Name: Submit
   From State: Draft
   To State: Pending Review
   Description: Submit product for review
   ```

4. **Add More Transitions**:
   - **Approve**: Pending Review → Approved
   - **Reject**: Pending Review → Rejected
   - **Revise**: Pending Review → Draft (for revisions)

## Advanced Editor Features

### State Configuration

#### Initial State Setup
- **Purpose**: Defines where new entities start in the workflow
- **Configuration**: Check "Is Initial State" in properties panel
- **Validation**: Only one initial state per state machine is allowed
- **Visual Indicator**: Initial states display with a green border

#### Final State Configuration
- **Success States**: Mark positive workflow completion
  ```
  ☑ Is Final State
  ☑ Is Success State
  ☐ Is Failed State
  ```
- **Failure States**: Mark negative workflow completion
  ```
  ☑ Is Final State
  ☐ Is Success State
  ☑ Is Failed State
  ```

#### State Data
- **Custom Properties**: Add entity-specific data to states
- **JSON Format**: Store complex state information
- **Example**:
  ```json
  {
    "reviewerRole": "ProductManager",
    "requiredApprovals": 2,
    "notificationTemplate": "product-approval-needed"
  }
  ```

### Transition Configuration

#### Basic Transition Setup
```
Trigger: Approve
From State: Pending Review
To State: Approved
Description: Approve the product for publication
```

#### Conditional Transitions
- **Add Conditions**: Click "Add Condition" in transition properties
- **Condition Types**:
  - Field conditions (entity property checks)
  - Role conditions (user permission checks)
  - Custom conditions (business logic)

#### Transition Actions
- **Pre-Actions**: Execute before state change
- **Post-Actions**: Execute after state change
- **Examples**:
  - Send notification emails
  - Update entity properties
  - Log audit information

### Localization Support

#### Adding Localizations
1. **Select State or Transition**
2. **Open Localization Panel**
3. **Add Language Variants**:
   ```
   English (en-US): "Pending Review"
   French (fr-FR): "En Attente de Révision"
   Spanish (es-ES): "Pendiente de Revisión"
   German (de-DE): "Ausstehende Überprüfung"
   ```

#### Localization Best Practices
- **Consistent Terminology**: Use the same terms across all workflows
- **Cultural Adaptation**: Consider cultural differences in business processes
- **Professional Translation**: Use professional translators for customer-facing text

## Visual Design Best Practices

### Layout Guidelines

#### State Positioning
- **Left to Right Flow**: Arrange states in logical progression
- **Vertical Grouping**: Group related states vertically
- **Consistent Spacing**: Maintain uniform spacing between elements

#### Transition Routing
- **Minimize Crossings**: Avoid overlapping transition lines
- **Clear Paths**: Ensure transition directions are obvious
- **Curved Lines**: Use curves for better visual flow

### Color Coding

#### State Colors
- **Green**: Success states and positive outcomes
- **Red**: Failure states and negative outcomes
- **Blue**: Initial states and starting points
- **Yellow**: Warning states and attention-required states
- **Gray**: Inactive or disabled states

#### Transition Colors
- **Green**: Approval and positive transitions
- **Red**: Rejection and negative transitions
- **Blue**: Standard workflow transitions
- **Orange**: Exception and error handling transitions

### Naming Conventions

#### State Names
- **Clear and Concise**: "Draft", "Pending Review", "Approved"
- **Action-Oriented**: "Awaiting Approval", "Under Review"
- **Consistent Tense**: Use present tense consistently

#### Trigger Names
- **Verb-Based**: "Submit", "Approve", "Reject", "Cancel"
- **User-Friendly**: Names that make sense to end users
- **Consistent Style**: Use same naming pattern throughout

## Working with Complex Workflows

### Hierarchical States
- **Parent States**: Group related sub-states
- **Sub-States**: Detailed workflow steps within parent states
- **Entry/Exit Points**: Define how entities enter and exit state groups

### Parallel Workflows
- **Fork States**: Split workflow into parallel paths
- **Join States**: Merge parallel paths back together
- **Synchronization**: Ensure all parallel paths complete before proceeding

### Exception Handling
- **Error States**: Handle unexpected conditions
- **Timeout Transitions**: Automatic transitions after time periods
- **Escalation Paths**: Route to supervisors or administrators

## Validation and Testing

### Built-in Validation

The editor automatically validates your state machine for:

#### Structural Issues
- **Missing Initial State**: Every state machine must have exactly one initial state
- **Unreachable States**: All states must be reachable from the initial state
- **Dead Ends**: Non-final states must have at least one outgoing transition
- **Orphaned States**: States with no incoming transitions (except initial state)

#### Logical Issues
- **Circular Dependencies**: Detect infinite loops in transitions
- **Conflicting Conditions**: Multiple transitions with overlapping conditions
- **Missing Permissions**: Transitions without proper permission checks

### Testing Your State Machine

#### Preview Mode
1. **Click Preview** in the toolbar
2. **Select Test Entity**: Choose an entity to simulate
3. **Execute Transitions**: Click triggers to test workflow
4. **Observe State Changes**: Verify correct behavior

#### Test Scenarios
- **Happy Path**: Test the ideal workflow progression
- **Error Conditions**: Test failure scenarios and error handling
- **Edge Cases**: Test unusual but valid scenarios
- **Permission Checks**: Verify security and access controls

## Importing and Exporting

### Export Options

#### JSON Export
```json
{
  "name": "Product Approval Workflow",
  "entityType": "Product",
  "version": "1.0",
  "states": [
    {
      "name": "Draft",
      "isInitial": true,
      "transitions": [
        {
          "trigger": "Submit",
          "toState": "PendingReview"
        }
      ]
    }
  ]
}
```

#### Visual Export
- **PNG/SVG**: Export visual diagram for documentation
- **PDF**: Generate printable workflow documentation
- **Visio**: Export to Microsoft Visio format

### Import Sources
- **JSON Files**: Import from exported state machine definitions
- **Templates**: Use pre-built workflow templates
- **Other Systems**: Import from external workflow tools

## Troubleshooting

### Common Issues

#### States Not Connecting
- **Check Selection**: Ensure both states are properly selected
- **Verify Permissions**: Confirm you have edit permissions
- **Canvas Zoom**: Try zooming in for better precision

#### Validation Errors
- **Read Error Messages**: Validation panel shows specific issues
- **Check State Properties**: Verify all required fields are filled
- **Review Transitions**: Ensure all transitions have valid targets

#### Performance Issues
- **Large Workflows**: Consider breaking into smaller sub-workflows
- **Browser Memory**: Close other tabs and refresh the page
- **Network Issues**: Check internet connection stability

### Getting Help

#### Built-in Help
- **Tooltips**: Hover over tools and buttons for quick help
- **Context Menu**: Right-click for context-specific options
- **Help Panel**: Access detailed help documentation

#### Support Resources
- **Documentation**: Comprehensive online documentation
- **Community Forums**: Ask questions and share experiences
- **Support Tickets**: Contact technical support for complex issues

## Advanced Tips and Tricks

### Keyboard Shortcuts
- **Ctrl+S**: Save state machine
- **Ctrl+Z**: Undo last action
- **Ctrl+Y**: Redo last action
- **Delete**: Remove selected element
- **Ctrl+A**: Select all elements

### Productivity Features
- **Templates**: Create reusable workflow templates
- **Copy/Paste**: Duplicate states and transitions
- **Bulk Operations**: Select multiple elements for batch operations
- **Auto-Save**: Automatic saving prevents data loss

### Integration Tips
- **Version Control**: Export definitions for version control
- **Documentation**: Generate documentation from visual diagrams
- **Collaboration**: Share workflows with team members

## Next Steps

- [Transition Conditions](transition-conditions.md): Learn to implement complex business rules
- [Operator Portal Actions](operator-portal-actions.md): Integrate with admin interfaces
- [Vendor Portal Actions](vendor-portal-actions.md): Implement customer-facing workflows
