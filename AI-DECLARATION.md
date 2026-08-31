---
version: "0.1.2"
level: copilot
processes:
  design: copilot
  implementation: copilot
  testing: pair
  documentation: copilot
  review: pair
  deployment: copilot
---

This format is based on [AI-DECLARATION.md](https://ai-declaration.md/en/0.1.2).

## Notes

- The plugin is implemented by Claude Code sessions directed by the maintainer;
  commits carry `Co-Authored-By: Claude` trailers.
- The maintainer decides features and scope, verifies every change in game
  before it ships, and authorizes all releases. The sun-removal mechanism
  (`SkyVisibility = 0`) was found through in-game experimentation by the
  maintainer using a research window built for that purpose.
- EnvState offsets and the EnvStateCopy signature are sourced from the
  third-party, human-written [Ktisis](https://github.com/ktisis-tools/Ktisis)
  project.
