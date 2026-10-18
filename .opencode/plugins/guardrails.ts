import { Plugin } from "@opencode/plugin"

// Taskedin guardrails: defense-in-depth on top of the permission rules in
// opencode.jsonc. OpenCode permission rules match whole command strings, so a
// compound command such as "dotnet build X && git push" could otherwise ride in
// on an allowed prefix. This hook re-checks commands that resolved to "allow"
// and escalates dangerous ones to an explicit approval prompt with a reason.
//
// Explicit configured denials (e.g. "git push *") are final and never reach
// this hook; the hook only ever tightens "allow" -> "ask", never loosens.

const BLOCKED: readonly { token: string; reason: string }[] = [
  {
    token: "git push",
    reason: "Pushing to GitHub requires explicit user approval (AGENTS.md).",
  },
  {
    token: "git reset --hard",
    reason: "Hard reset discards existing work; the repository holds uncommitted user changes (AGENTS.md).",
  },
  {
    token: "git clean",
    reason: "git clean deletes untracked files; existing user work must be preserved (AGENTS.md).",
  },
  {
    token: "dotnet ef database update",
    reason: "Database-changing commands require explicit user approval (AGENTS.md).",
  },
  {
    token: "dotnet-ef database update",
    reason: "Database-changing commands require explicit user approval (AGENTS.md).",
  },
  {
    token: "ef database drop",
    reason: "Dropping the database requires explicit user approval (AGENTS.md).",
  },
  {
    token: "ef migrations remove",
    reason: "Removing a migration deletes generated files; requires explicit user approval (AGENTS.md).",
  },
]

export default Plugin.define({
  id: "taskedin-guardrails",
  async setup(ctx) {
    await ctx.permission.hook("evaluate", (event) => {
      if (event.action !== "shell" || event.effect !== "allow") return

      const command = event.resources.join(" ").trim().toLowerCase()
      const hit = BLOCKED.find(
        (rule) => command === rule.token || command.startsWith(rule.token + " ") || command.includes("; " + rule.token) || command.includes("&& " + rule.token),
      )
      if (!hit) return

      event.effect = "ask"
      event.message = hit.reason
    })
  },
})
