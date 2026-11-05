import type { Ref } from "vue";
import type {
  StateMachineAttribute,
  StateMachineDefinition,
  StateMachineLocalization,
  StateMachineSettings,
} from "../api_client/virtocommerce.statemachine";

// Like the AngularJS blades, the visual editor edits the definition open in the details blade (its statesGraph and
// the localizations / attributes of its states and transitions); the details blade's Save persists everything.
// The details blade hands its draft to the editor through the blade options.
export interface DraftModel {
  entity: StateMachineDefinition;
  localizations: StateMachineLocalization[];
  attributes: StateMachineAttribute[];
}

export interface Draft {
  model: Ref<DraftModel>;
  settings: Ref<StateMachineSettings>;
}
