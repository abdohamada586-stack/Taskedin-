---
description: Review Taskedin code or changes for bugs, security issues and architecture violations
agent: code-reviewer
---

Review $ARGUMENTS (when no path or scope is given, review the current working-tree changes,
including untracked files).

Follow the code-review skill procedure: establish scope with Git, review against `AGENTS.md`, and
report findings in severity order (Critical → High → Medium → Low → Nit) with `file:line`
references and concrete suggestions. Do not modify any files. If a build is attempted, report the
result exactly, including file-lock failures caused by Visual Studio or a running application.
