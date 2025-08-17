---
description: "Release a new version of the project by creating a new Tag and pushing it to the repository."
mode: agent
tools: ['changes', 'codebase', 'editFiles', 'extensions', 'fetch', 'findTestFiles', 'githubRepo', 'new', 'openSimpleBrowser', 'problems', 'readCellOutput', 'runCommands', 'runNotebooks', 'runTasks', 'runTests', 'search', 'searchResults', 'terminalLastCommand', 'terminalSelection', 'testFailure', 'updateUserPreferences', 'usages', 'vscodeAPI', 'github', 'create_pull_request', 'get_job_logs', 'get_pull_request', 'get_pull_request_comments', 'get_pull_request_diff', 'get_pull_request_status', 'get_workflow_run', 'get_workflow_run_logs', 'list_pull_requests', 'list_workflow_runs', 'rerun_failed_jobs', 'rerun_workflow_run', 'run_workflow', 'search_code', 'update_issue', 'update_pull_request', 'update_pull_request_branch']
---

# GitHub Feature Branch Workflow

You are tasked with releasing a new version of the project by creating a new Tag and pushing it to the repository.

## Release Overview

Get the last git-tag using git
```bash
git describe --tags --abbrev=0
```

## Incrementing the Version

get the commits between the last tag and the current HEAD
```bash
git log $(git describe --tags --abbrev=0)..HEAD --oneline
```

If the changes are minor, you can increment the patch version. If there are breaking changes, increment the major version. If it's a new feature, increment the minor version.

## Release Steps

Ensure you are on the main branch and it is up to date:
```bash
git checkout main
git pull origin main
```

### Run git commands:

```bash
git tag new-version
git push origin new-version
```