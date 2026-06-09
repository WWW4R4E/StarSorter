# Forks API

Base URL: `https://api.github.com`

---

## POST /repos/{owner}/{repo}/merge-upstream

Sync a fork with its upstream repository (merges `main` branch).

**Request body:**

```json
{ "branch": "main" }
```

**Response:** [`SyncForkResult`](types.md#syncforkresult)

**Client method:** `syncFork(owner, repo)`

**Export:** `gh_sync_fork(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/compare/{branch}

Compare a fork's branch with its upstream default branch to check if sync is needed.

**Response:** [`CompareResult`](types.md#compareresult)

**Client method:** `checkForkSyncNeeded(owner, repo, branch)`

**Export:** `gh_check_fork_sync(wrapper, owner, repo, branch)` → raw GitHub JSON


