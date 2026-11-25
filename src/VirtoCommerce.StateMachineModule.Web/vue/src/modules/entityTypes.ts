import { api } from "./api";
import type { StateMachineEntityType } from "../api_client/virtocommerce.statemachine";

let entityTypes: Promise<StateMachineEntityType[]> | undefined;

/**
 * Entity types state machines can be defined for, as the installed modules registered them on the server
 * (IStateMachineEntityTypeRegistrar; legacy stateMachineTypes.addType). Loaded once; a failed load is retried on the
 * next call.
 */
export function loadEntityTypes(): Promise<StateMachineEntityType[]> {
  entityTypes ??= api.stateMachine
    .getEntityTypes()
    .then((x) => x ?? [])
    .catch(() => {
      entityTypes = undefined;
      return [];
    });
  return entityTypes;
}

/** Display name of an entity type; an unregistered one shows its id. */
export const entityTypeName = (types: StateMachineEntityType[], id?: string) =>
  types.find((x) => x.id?.toLowerCase() === id?.toLowerCase())?.name ?? id;

/** Whether transitions of the type can have conditions (it registered a condition tree prototype). */
export const hasConditionTree = (types: StateMachineEntityType[], id?: string) =>
  !!types.find((x) => x.id?.toLowerCase() === id?.toLowerCase())?.hasConditionTree;
