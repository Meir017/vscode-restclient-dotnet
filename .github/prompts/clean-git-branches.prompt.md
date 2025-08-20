---
mode: agent
description: Define a process for cleaning up git branches in a repository.
tools: ['runCommands']
---

Define the process of cleanup old git branches.

Use the `git` commands:

- Switch branch: `git checkout main`
- Prune remote branches: `git remote prune origin`
- List all branches: `git branch -a`
- Delete branch: `git branch -D <branch_name>`
