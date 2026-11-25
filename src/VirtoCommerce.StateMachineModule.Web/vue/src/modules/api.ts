import { SecurityClient } from "@vc-shell/framework";
import { StateMachineClient } from "../api_client/virtocommerce.statemachine";

// The generated clients are plain fetch wrappers (same-origin cookie auth), so one instance each is enough.
export const api = {
  stateMachine: new StateMachineClient(),
  security: new SecurityClient(),
};

/** Small JSON fetch for platform endpoints the generated clients don't cover. */
export async function http<T = unknown>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: body !== undefined ? { "Content-Type": "application/json" } : {},
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`${method} ${url}: ${res.status} ${await res.text()}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

/** Deep copy that also works on reactive proxies (structuredClone rejects proxies). */
export const clone = <T>(x: T): T => (x === undefined ? x : JSON.parse(JSON.stringify(x)));

/** Message of a failed call, for showError. */
export const errorText = (e: unknown) => String((e as Error)?.message ?? e);
