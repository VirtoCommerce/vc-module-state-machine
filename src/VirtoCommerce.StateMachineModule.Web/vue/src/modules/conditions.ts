// Data for the condition tree editor (legacy dynamicConditions: permissionConditionController,
// accoutTypeConditionController). Loaded once per session; a failed load is retried on the next call.
import { api, http } from "./api";
import type { ConditionNode } from "./types";

let permissions: Promise<string[]> | undefined;
/** Names of all registered permissions (legacy roles.queryPermissions). */
export function loadPermissions(): Promise<string[]> {
  permissions ??= api.security
    .getAllRegisteredPermissions()
    .then((x) => (x ?? []).map((p) => p.name!).filter(Boolean))
    .catch(() => {
      permissions = undefined;
      return [];
    });
  return permissions;
}

let accountTypes: Promise<string[]> | undefined;
/** The platform's account types (legacy settings.get VirtoCommerce.Platform.Security.AccountTypes). */
export function loadAccountTypes(): Promise<string[]> {
  accountTypes ??= http<unknown[] | null>("GET", "/api/platform/settings/values/VirtoCommerce.Platform.Security.AccountTypes")
    .then((x) => (x ?? []).map(String))
    .catch(() => {
      accountTypes = undefined;
      return [];
    });
  return accountTypes;
}

/** The tree's structural keys; every other key of a node is one of its own fields. */
const STRUCTURE = new Set(["id", "children", "availableChildren"]);

/** Fields the legacy expression designer added for display; trees it saved still carry them (legacy stripOffUiInformation). */
const LEGACY_UI = new Set(["displayName", "getValidationError", "groupName", "newChildLabel", "templateURL"]);
const isUiField = (key: string) => LEGACY_UI.has(key) || key.startsWith("$$");

/**
 * A node's own editable fields: strings, numbers and booleans (e.g. ApprovalPolicy of another module's element). A null
 * one ({ approvalPolicy: null } in a prototype) is an unset string.
 */
export const ownFields = (node: ConditionNode) =>
  Object.keys(node).filter(
    (key) =>
      !STRUCTURE.has(key) &&
      !isUiField(key) &&
      (node[key] === null || ["string", "number", "boolean"].includes(typeof node[key])),
  );

/** The label of an element: the legacy display name saved with it, else its type name. */
export const storedDisplayName = (node: ConditionNode) =>
  typeof node.displayName === "string" ? node.displayName : undefined;

/** A node that holds other conditions: it offers children to add. */
export const isBlock = (node: ConditionNode) => !!node.availableChildren?.length;

/** "StateMachineConditionHasPermission" -> "State machine condition has permission" */
export const humanize = (id = "") => {
  const words = id.replace(/([a-z0-9])([A-Z])/g, "$1 $2").toLowerCase();
  return words.charAt(0).toUpperCase() + words.slice(1);
};

/** A new child from an offered one: a copy without children, and without the legacy display fields. */
export function newChild(offered: ConditionNode): ConditionNode {
  const copy = JSON.parse(JSON.stringify(offered)) as ConditionNode;
  Object.keys(copy)
    .filter(isUiField)
    .forEach((key) => delete copy[key]);
  return { ...copy, children: [] };
}
