// Localizations and attributes of states and transitions. They are keyed by the item's text (a state's name, a
// transition's trigger), as in the legacy editor (saveCurrentTranslations / saveCurrentAttributes).
import type { DefinitionModel } from "./types";

/** The values of an item's texts, by locale or attribute key. */
export function itemValues(model: DefinitionModel, kind: "localizations" | "attributes", item: string, keys: string[]) {
  const values: Record<string, string> = {};
  for (const key of keys) {
    const found =
      kind === "localizations"
        ? model.localizations.find((x) => x.item === item && x.locale === key)
        : model.attributes.find((x) => x.item === item && x.attributeKey === key);
    values[key] = found?.value ?? "";
  }
  return values;
}

/**
 * Applies the dialog's values: changes the existing records, adds new ones. As in legacy, an emptied value is not
 * sent, so its record keeps its stored value.
 */
export function applyItemValues(
  model: DefinitionModel,
  kind: "localizations" | "attributes",
  item: string,
  values: Record<string, string>,
) {
  const definitionId = model.entity.id;
  for (const [key, value] of Object.entries(values)) {
    if (!value) continue;
    if (kind === "localizations") {
      const found = model.localizations.find((x) => x.item === item && x.locale === key);
      if (found) found.value = value;
      else model.localizations.push({ definitionId, item, locale: key, value });
    } else {
      const found = model.attributes.find((x) => x.item === item && x.attributeKey === key);
      if (found) found.value = value;
      else model.attributes.push({ definitionId, item, attributeKey: key, value });
    }
  }
}

/** A renamed state or trigger keeps its texts (legacy updateLocalizationsAttributes). */
export function renameItem(model: DefinitionModel, from: string, to: string) {
  if (!from || !to || from === to) return;
  model.localizations.filter((x) => x.item === from).forEach((x) => (x.item = to));
  model.attributes.filter((x) => x.item === from).forEach((x) => (x.item = to));
}
