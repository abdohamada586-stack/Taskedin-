---
description: Prepare and run a build of the Taskedin solution
---

Prepare and run a build of the Taskedin solution, then report the outcome.

!`dotnet build Taskedin.sln`

Interpret the result honestly:
- **Succeeded** → summarize warnings if any (there should be no new ones) and confirm which files
  were changed before the build.
- **Compile errors (CS####)** → list each error with file and line, and fix them only if the
  changes are part of the currently approved task.
- **MSB3021/MSB3027 file-lock errors** → these mean Visual Studio or a running `Taskedin` process
  holds files under `bin`/`obj`. State that exactly. Do not terminate processes, do not redirect
  output paths, and do not claim the build succeeded.
