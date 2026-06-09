# Releases API

Base URL: `https://api.github.com`

---

## GET /repos/{owner}/{repo}/releases

List releases for a repository.

**Query params:**

| Param      | Type   | Description          |
|------------|--------|----------------------|
| `page`     | u32    | Page number          |
| `per_page` | u8     | Results per page     |

**Response:** `[]`[`Release`](types.md#release)

**Client method:** `getRepositoryReleases(owner, repo, page, per_page)`

**Export:** `gh_get_releases(wrapper, owner, repo, page, per_page)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/releases (all pages)

Fetch all releases across all pages (paginates internally).

**Response:** `[]`[`Release`](types.md#release)

**Client method:** `fetchAllReleasesForRepo(owner, repo)`

**Export:** `gh_fetch_all_releases(wrapper, owner, repo)` → serialized JSON array

---

## GET /repos/{owner}/{repo}/releases (incremental)

Fetch releases published after a given Unix timestamp.

**Response:** `[]`[`Release`](types.md#release)

**Client method:** `getIncrementalRepositoryReleases(owner, repo, since)`

**Export:** `gh_get_incremental_releases(wrapper, owner, repo, since)` → serialized JSON array
