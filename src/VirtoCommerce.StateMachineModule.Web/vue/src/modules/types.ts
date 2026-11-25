// Blade options and shared models; type aliases, not interfaces, so they fit openBlade's Record<string, unknown> options
import type {
  IConditionTree,
  StateMachineAttribute,
  StateMachineDefinition,
  StateMachineLocalization,
} from "../api_client/virtocommerce.statemachine";

/**
 * What the details blade edits and saves together: the definition and the localizations / attributes of its states and
 * transitions. As in the legacy blades, the visual editor changes this same object and the details blade's Save
 * persists it.
 */
export type DefinitionModel = {
  entity: StateMachineDefinition;
  localizations: StateMachineLocalization[];
  attributes: StateMachineAttribute[];
};

/** StateMachineVisualEditor: the details blade's model (shared, not copied) */
export type EditorOptions = {
  model: DefinitionModel;
};

/** StateMachineCondition: the transition whose condition is edited */
export type ConditionOptions = {
  entityType: string;
  transitionId: string;
  trigger: string;
  condition?: IConditionTree;
};

/** A condition tree node as the server serializes it: id (type name), children, offered children, own fields */
export type ConditionNode = {
  id?: string;
  children?: ConditionNode[];
  availableChildren?: ConditionNode[];
  [field: string]: unknown;
};
