# NEsper 8.9.x — Active Plans

This directory contains detailed implementation plans for in-progress refactoring work.

## Plans

| Directory | Plan | Branch | Status |
|-----------|------|--------|--------|
| `service-locator-removal/` | [Service Locator → Constructor Injection](service-locator-removal/PLAN.md) | `service-locator-refactor` | Planned |

## How to Use

Each plan directory contains a `PLAN.md` with:
- Phase breakdown and dependency graph
- Specific files to modify with line-level guidance
- Breaking changes per phase
- Test strategy and success criteria per phase

Start the next unstarted phase, run the prescribed tests, then move to the next.

## Finding Plans

The active plans are also referenced from:
- `CLAUDE.md` → "Active Refactoring Work" section (auto-loaded in every Claude Code session)
- `~/.claude/projects/.../memory/MEMORY.md` → "Service Locator Removal" section
