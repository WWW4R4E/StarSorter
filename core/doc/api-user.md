# User API

Base URL: `https://api.github.com`

Authentication: Bearer token via `Authorization: Bearer <token>`

---

## GET /user

Get the authenticated user's profile.

**Response:** [`GitHubUser`](types.md#githubuser)

**Client method:** `getCurrentUser()`

**Export:** `gh_get_current_user(wrapper)` → raw GitHub JSON

---

## GET /user/starred

List repositories starred by the authenticated user.

**Query params:**

| Param     | Type   | Description                              |
|-----------|--------|------------------------------------------|
| `page`    | u32    | Page number (1-based)                    |
| `per_page`| u8     | Results per page (max 100)               |
| `sort`    | string | `created` (default)                      |
| `direction` | string | `desc` (default)                       |

**Response:** `[]`[`Repository`](types.md#repository)

**Client method:** `getStarredRepositories(page, per_page)`

**Export:** `gh_get_starred_repos(wrapper, page, per_page)` → raw GitHub JSON

---

## GET /user/starred (all pages)

Fetch all starred repositories across all pages (paginates internally).

**Response:** `[]`[`Repository`](types.md#repository)

**Client method:** `getAllStarredRepositories()`

**Export:** `gh_get_all_starred_repos(wrapper)` → serialized JSON array

---

## PUT /user/starred/{owner}/{repo}

Star a repository for the authenticated user.

**Response:** `void` (204 No Content)

**Client method:** `starRepository(owner, repo)`

**Export:** `gh_star_repository(wrapper, owner, repo)` → 204 No Content

---

## DELETE /user/starred/{owner}/{repo}

Unstar a repository for the authenticated user.

**Response:** `void` (204 No Content)

**Client method:** `unstarRepository(owner, repo)`

**Export:** `gh_unstar_repository(wrapper, owner, repo)` → 204 No Content

---

## GET /user/repos?type=forks&sort=updated&per_page=100

List the authenticated user's forked repositories.

**Response:** `[]`[`Repository`](types.md#repository)

**Client method:** `getUserForks()`

**Export:** `gh_get_user_forks(wrapper)` → raw GitHub JSON

---

## GET /users/{login}

Get a public user's profile by login name.

**Response:** [`GitHubUser`](types.md#githubuser)

**Client method:** `getUserByLogin(login)` (available internally, no export)
