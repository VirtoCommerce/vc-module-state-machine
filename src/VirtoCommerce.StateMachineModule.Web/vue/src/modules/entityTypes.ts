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

/**
 * Types the existing definitions use. Offered next to the registered ones, so a definition of a type no module
 * registers yet can still be created for it.
 */
export async function usedEntityTypes(): Promise<string[]> {
  const res = await api.stateMachine.search({ take: 1000 });
  return [...new Set((res.results ?? []).map((x) => x.entityType).filter((x): x is string => !!x))];
}

/** Display name of an entity type; an unregistered one shows its id. */
export const entityTypeName = (types: StateMachineEntityType[], id?: string) =>
  types.find((x) => x.id?.toLowerCase() === id?.toLowerCase())?.name ?? id;

/** Whether transitions of the type can have conditions (it registered a condition tree prototype). */
export const hasConditionTree = (types: StateMachineEntityType[], id?: string) =>
  !!types.find((x) => x.id?.toLowerCase() === id?.toLowerCase())?.hasConditionTree;
