# Repository API

Base URL: `https://api.github.com`

---

## GET /repos/{owner}/{repo}

Get a single repository.

**Response:** [`Repository`](types.md#repository)

**Client method:** `getRepository(owner, repo)`

**Export:** `gh_get_repository(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/languages

Get the programming languages used in a repository.

**Response:** Raw JSON object (`{ "language": bytes, ... }`)

**Client method:** `getRepositoryLanguages(owner, repo)` — returns raw `[]u8`

**Export:** `gh_get_repo_languages(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/readme

Get the repository's README content (base64-encoded from GitHub API).

**Response:** Raw GitHub JSON with `content` field (base64), decode on C# side.

**Client method:** `getRepositoryReadme(owner, repo)` — decoded text

**Export:** `gh_get_repository_readme(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/releases/latest

Get the latest published release.

**Response:** [`Release`](types.md#release)

**Client method:** `getLatestRelease(owner, repo)`

**Export:** `gh_get_latest_release(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/contributors

List repository contributors.

**Query params:** (implicit — uses GitHub defaults)

**Response:** `[]`[`Contributor`](types.md#contributor)

**Client method:** `getContributors(owner, repo)`

**Export:** `gh_get_contributors(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/forks

List repository forks.

**Query params:**

| Param      | Type   | Description                          |
|------------|--------|--------------------------------------|
| `sort`     | string | `newest`, `oldest`, `stargazers`     |
| `per_page` | u8     | Results per page                     |

**Response:** `[]`[`Repository`](types.md#repository)

**Client method:** `getForks(owner, repo, sort, per_page)`

**Export:** `gh_get_forks(wrapper, owner, repo, sort, per_page)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/commits

List recent commits.

**Query params:**

| Param      | Type   | Description          |
|------------|--------|----------------------|
| `per_page` | u8     | Results per page     |
| `page`     | u32    | Page number          |

**Response:** `[]`[`Commit`](types.md#commit)

**Client method:** `getCommits(owner, repo, per_page, page)`

**Export:** `gh_get_commits(wrapper, owner, repo, per_page, page)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/tags

List repository tags.

**Response:** `[]`[`Tag`](types.md#tag)

**Client method:** `getTags(owner, repo)`

**Export:** `gh_get_tags(wrapper, owner, repo)` → raw GitHub JSON

---

## GET /user/starred/{owner}/{repo}

Check if the authenticated user has starred a repository.

**Response:** `"true"` or `"false"` string

**Client method:** `checkStarred(owner, repo)`

**Export:** `gh_check_starred(wrapper, owner, repo)` → `"true"` / `"false"`

---

## GET /repos/{owner}/{repo}/branches

List repository branches.

**Response:** `[]`[`Branch`](types.md#branch)

**Client method:** `getBranches(owner, repo)`

**Export:** `gh_get_branches(wrapper, owner, repo)` → raw GitHub JSON
