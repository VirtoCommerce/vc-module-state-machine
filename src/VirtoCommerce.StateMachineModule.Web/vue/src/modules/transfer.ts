// Export / import of a definition as a .zip holding one JSON file — the AngularJS stateMachineExportImportService
// format, so archives move between the two UIs.
// JSZip is loaded on first use, so it stays out of the chunk the host loads at startup.
import type { StateMachineClient } from "../api_client/virtocommerce.statemachine";
import type { DraftModel } from "./draft";

/** Saves a definition's localizations and attributes (each only when there are any). */
export function saveTexts(
  client: StateMachineClient,
  definitionId: string | undefined,
  { localizations, attributes }: { localizations: object[]; attributes: object[] },
  guard = <T>(p: Promise<T>): Promise<T | undefined> => p,
) {
  return Promise.all([
    localizations.length && guard(client.updateLocalization({ localizations: localizations.map((x) => ({ ...x, definitionId })) })),
    attributes.length && guard(client.updateAttribute({ attributes: attributes.map((x) => ({ ...x, definitionId })) })),
  ]);
}

export async function exportDefinition({ entity, localizations, attributes }: DraftModel) {
  const data = {
    id: entity.id,
    name: entity.name,
    version: entity.version,
    entityType: entity.entityType,
    isActive: entity.isActive,
    statesGraph: entity.statesGraph,
    statesCapture: entity.statesCapture,
    states: JSON.parse(entity.statesGraph || "[]"),
    createdDate: entity.createdDate,
    modifiedDate: entity.modifiedDate,
    createdBy: entity.createdBy,
    modifiedBy: entity.modifiedBy,
    localizations,
    attributes,
  };
  const name = entity.name || "state-machine-export";
  const { default: JSZip } = await import("jszip");
  const zip = new JSZip();
  zip.file(`${name}.json`, JSON.stringify(data, null, 2));
  const blob = await zip.generateAsync({ type: "blob", compression: "DEFLATE", compressionOptions: { level: 6 } });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `${name}.zip`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  // revoked right after click(), Firefox and Safari may not have started the download yet
  setTimeout(() => URL.revokeObjectURL(link.href), 1000);
}

/** Creates the definition (or overwrites the one with the archive's id), then its localizations and attributes. */
export async function importDefinition(file: File, client: StateMachineClient) {
  const { default: JSZip } = await import("jszip");
  const zip = await JSZip.loadAsync(file);
  const json = Object.values(zip.files).find((f) => !f.dir && f.name.endsWith(".json"));
  if (!json) throw new Error("No JSON file found in the zip archive");
  const { localizations = [], attributes = [], ...definition } = JSON.parse(await json.async("string"));
  if (!Array.isArray(definition.states)) throw new Error("Invalid state machine format");

  const created = await client.createNewDefinition({ definition });
  // as the AngularJS import: the definition counts even when its texts fail
  await saveTexts(client, created.id, { localizations, attributes }, (p) => p.catch(() => undefined));
  return created;
}
