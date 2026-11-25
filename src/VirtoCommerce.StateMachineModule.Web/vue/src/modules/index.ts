import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule } from "@vc-shell/framework";

// No widgets: the legacy UI registers none on other modules' blades.
export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
