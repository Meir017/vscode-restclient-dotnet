---
description: "Complete GitHub feature branch workflow with automated PR management"
mode: agent
tools: ['changes', 'codebase', 'editFiles', 'extensions', 'fetch', 'findTestFiles', 'githubRepo', 'new', 'openSimpleBrowser', 'problems', 'readCellOutput', 'runCommands', 'runNotebooks', 'runTasks', 'runTests', 'search', 'searchResults', 'terminalLastCommand', 'terminalSelection', 'testFailure', 'updateUserPreferences', 'usages', 'vscodeAPI', 'github', 'create_pull_request', 'get_job_logs', 'get_pull_request', 'get_pull_request_comments', 'get_pull_request_diff', 'get_pull_request_status', 'get_workflow_run', 'get_workflow_run_logs', 'list_pull_requests', 'list_workflow_runs', 'rerun_failed_jobs', 'rerun_workflow_run', 'run_workflow', 'search_code', 'update_issue', 'update_pull_request', 'update_pull_request_branch', 'microsoft.docs.mcp', 'context7']
---

# GitHub Feature Branch Workflow

You are tasked with implementing a complete feature branch workflow including branch creation, development, pull request management, and cleanup.

## Workflow Overview

This prompt guides you through the complete GitHub feature development lifecycle:

1. **Create Feature Branch** - Create and checkout new feature branch
2. **Implement Changes** - Make code changes and commit them
3. **Create Pull Request** - Submit PR with proper description
4. **Monitor CI/CD** - Watch GitHub Actions and handle failures
5. **Merge PR** - Merge when all checks pass
6. **Cleanup** - Delete feature branch and return to main

## Prerequisites

- You have push access to the repository
- The repository has GitHub Actions configured
- The main branch is protected (recommended)

## Implementation Steps

### 1. Create Feature Branch

```bash
# Ensure we're on main and up to date
git checkout main
git pull origin main

# Create new feature branch
git checkout -b feature/{feature-name}
```

**Branch Naming Convention:**
- `feature/{description}` - New features
- `bugfix/{description}` - Bug fixes  
- `docs/{description}` - Documentation updates
- `refactor/{description}` - Code refactoring

### 2. Implement Changes

Make your code changes following project conventions:

- Follow existing code style and patterns
- Add comprehensive tests for new functionality
- Update documentation as needed
- Ensure all existing tests continue to pass

**Commit Guidelines:**
- Use conventional commit messages
- Make atomic commits (one logical change per commit)
- Write descriptive commit messages

```bash
# Stage and commit changes
git add .
git commit -m "feat: add new feature implementation

- Add core functionality
- Include comprehensive tests  
- Update documentation"
```

### 3. Push and Create Pull Request

```bash
# Push feature branch to remote
git push origin feature/{feature-name}
```

**Pull Request Requirements:**
- **Title**: Follow conventional commit format
- **Description**: Include implementation details, breaking changes, testing notes
- **Labels**: Add appropriate labels (feature, bug, documentation, etc.)
- **Reviewers**: Request reviews from relevant team members

**request a review from copilot using #request_copilot_review tool**

**PR Description Template:**
```markdown
## Description
Brief description of the changes made.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## How Has This Been Tested?
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
```

### 4. Monitor GitHub Actions

**Check CI/CD Status:**
- Build pipeline status
- Test execution results
- Code coverage reports
- Security scans
- Any deployment validations

**If Actions Fail:**
1. Review the failure logs
2. Fix issues in your feature branch
3. Push additional commits
4. Wait for actions to re-run

**Common Failure Scenarios:**
- Build errors (compilation failures)
- Test failures (unit/integration tests)
- Code quality issues (linting, formatting)
- Security vulnerabilities
- Coverage below threshold

### 5. Handle PR Review

**During Review Process:**
- Respond to review comments promptly
- Make requested changes in additional commits
- Engage in constructive discussion
- Update PR description if scope changes

**Review Resolution:**
```bash
# Make review changes
git add .
git commit -m "fix: address review feedback

- Update implementation based on reviewer suggestions
- Fix code style issues
- Add additional test cases"

git push origin feature/{feature-name}
```

### 6. Merge Pull Request

**When All Checks Pass:**
- All GitHub Actions are green ✅
- All requested reviews are approved ✅  
- No merge conflicts exist ✅
- Branch is up to date with target branch ✅

**Merge Options:**
- **Merge Commit**: Preserves full history (recommended for features)
- **Squash and Merge**: Clean history for small changes
- **Rebase and Merge**: Linear history without merge commits

### 7. Post-Merge Cleanup

```bash
# Switch back to main branch
git checkout main

# Pull latest changes (includes your merged PR)
git pull origin main

# Delete local feature branch
git branch -d feature/{feature-name}

# Delete remote feature branch (if not auto-deleted)
git push origin --delete feature/{feature-name}
```

**Verification Steps:**
- Confirm your changes are in main branch
- Verify all tests still pass on main
- Check that any deployed environments are updated

## Automation Opportunities

**GitHub CLI Commands:**
```bash
# Create PR from command line
gh pr create --title "feat: add new feature" --body-file pr-template.md

# Check PR status
gh pr status

# Merge PR when ready
gh pr merge --auto --squash
```

**GitHub Actions Integration:**
- Auto-assign reviewers based on code paths
- Auto-merge when all checks pass (with branch protection)
- Auto-delete feature branches after merge
- Notify teams on Slack/Teams when PR is ready

## Best Practices

### Code Quality
- Run tests locally before pushing
- Use pre-commit hooks for formatting/linting
- Keep commits focused and atomic
- Write clear, descriptive commit messages

### PR Management
- Keep PRs reasonably sized (< 500 lines when possible)
- Provide context in PR descriptions
- Link to relevant issues or documentation
- Use draft PRs for work-in-progress

### Branch Hygiene
- Delete merged branches promptly
- Keep feature branches short-lived
- Rebase feature branches to keep history clean
- Avoid long-running feature branches

## Troubleshooting

**Common Issues:**
- **Merge Conflicts**: Rebase feature branch onto latest main
- **Failed Actions**: Check logs and fix issues incrementally
- **Review Delays**: Follow up with reviewers, consider smaller PRs
- **Branch Protection**: Ensure you meet all required checks

**Emergency Procedures:**
- Hotfix process for critical issues
- Rollback procedures if issues are discovered
- Communication protocols for breaking changes

Please specify:
1. **Feature Name**: What feature are you implementing?
2. **Repository**: Which repository are you working with?
3. **Target Branch**: Usually `main` or `develop`
4. **Review Requirements**: Who should review the PR?

Let's begin the workflow!
