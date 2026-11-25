// Export / import of a definition as a .zip holding one JSON file, in the legacy stateMachineExportImportService format,
// so archives move between the two UIs. JSZip is bundled (legacy loaded it from a CDN) and loaded on first use.
import { api } from "./api";
import type { StateMachineDefinition } from "../api_client/virtocommerce.statemachine";
import type { DefinitionModel } from "./types";

export async function exportDefinition({ entity, localizations, attributes }: DefinitionModel) {
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
  // revoked right after click(), some browsers haven't started the download yet
  setTimeout(() => URL.revokeObjectURL(link.href), 1000);
}

/**
 * Creates the definition from an archive (or overwrites the one with the archive's id, as the legacy import did), then
 * its localizations and attributes. As in legacy, the definition counts even when its texts fail.
 */
export async function importDefinition(file: File): Promise<StateMachineDefinition> {
  const { default: JSZip } = await import("jszip");
  const zip = await JSZip.loadAsync(file);
  const json = Object.values(zip.files).find((f) => !f.dir && f.name.endsWith(".json"));
  if (!json) throw new Error("No JSON file found in the zip archive");
  const { localizations = [], attributes = [], ...definition } = JSON.parse(await json.async("string"));
  if (!Array.isArray(definition.states)) throw new Error("Invalid state machine format");

  const created = await api.stateMachine.createNewDefinition({ definition });
  const definitionId = created.id;
  if (definitionId) {
    await Promise.all([
      localizations.length &&
        api.stateMachine
          .updateLocalization({ localizations: localizations.map((x: object) => ({ ...x, definitionId })) })
          .catch(() => undefined),
      attributes.length &&
        api.stateMachine
          .updateAttribute({ attributes: attributes.map((x: object) => ({ ...x, definitionId })) })
          .catch(() => undefined),
    ]);
  }
  return created;
}

/** Lets the user pick a file (legacy created a hidden file input the same way). */
export function pickFile(accept: string): Promise<File | undefined> {
  return new Promise((resolve) => {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = accept;
    input.onchange = () => resolve(input.files?.[0]);
    input.click();
  });
}
