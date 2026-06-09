# Data Types

## GitHubUser

| Field             | Type              | Description                            |
|-------------------|-------------------|----------------------------------------|
| `login`           | string            | Username                               |
| `id`              | u64               | Numeric user ID                        |
| `node_id`         | string?           | GraphQL node ID                        |
| `avatar_url`      | string?           | Avatar URL                             |
| `gravatar_id`     | string?           | Gravatar hash                          |
| `html_url`        | string?           | GitHub profile URL                     |
| `name`            | string?           | Display name                           |
| `company`         | string?           | Company                                |
| `blog`            | string?           | Website URL                            |
| `location`        | string?           | Location                               |
| `email`           | string?           | Email address                          |
| `bio`             | string?           | Bio                                    |
| `twitter_username`| string?           | Twitter handle                         |
| `public_repos`    | u32?              | Public repo count                      |
| `public_gists`    | u32?              | Public gist count                      |
| `followers`       | u32?              | Follower count                         |
| `following`       | u32?              | Following count                        |
| `created_at`      | string?           | ISO 8601 creation date                 |
| `updated_at`      | string?           | ISO 8601 update date                   |
| `plan`            | UserPlan?         | Subscription plan (authenticated only) |

---

## UserPlan

| Field           | Type   | Description                  |
|-----------------|--------|------------------------------|
| `name`          | string | Plan name (`free`, `pro`, etc.) |
| `space`         | u64    | Total disk space in bytes    |
| `collaborators` | u32    | Max collaborators            |
| `private_repos` | u32    | Max private repos            |

---

## Repository

| Field                 | Type                | Description                          |
|-----------------------|---------------------|--------------------------------------|
| `id`                  | u64                 | Numeric repo ID                      |
| `node_id`             | string?             | GraphQL node ID                      |
| `name`                | string              | Repo name                            |
| `full_name`           | string              | `owner/repo` format                  |
| `private`             | bool                | Private flag                         |
| `owner`               | GitHubUser          | Owner object                         |
| `html_url`            | string              | GitHub URL                           |
| `description`         | string?             | Description text                     |
| `fork`                | bool                | Is a fork?                           |
| `url`                 | string              | API URL                              |
| `created_at`          | string              | ISO 8601 creation date               |
| `updated_at`          | string              | ISO 8601 update date                 |
| `pushed_at`           | string?             | ISO 8601 last push date              |
| `homepage`            | string?             | Homepage URL                         |
| `size`                | u64?                | Repo size (KB)                       |
| `stargazers_count`    | u32                 | Star count                           |
| `watchers_count`      | u32                 | Watcher count                        |
| `language`            | string?             | Primary language                     |
| `forks_count`         | u32                 | Fork count                           |
| `open_issues_count`   | u32                 | Open issues count                    |
| `topics`              | string[]?           | Topic tags                           |
| `visibility`          | string?             | Visibility (`public`, `private`)     |
| `default_branch`      | string              | Default branch name                  |
| `parent`              | RepositoryRef?      | Parent repo (if fork)                |
| `source`              | RepositoryRef?      | Source repo (if fork)                |
| `license`             | License?            | License info                         |
| `subscribers_count`   | u32                 | Subscriber count                     |
| `archived`            | bool                | Archived flag                        |
| `security_and_analysis` | SecurityAndAnalysis? | Security settings                 |

---

## RepositoryRef

| Field       | Type       | Description          |
|-------------|------------|----------------------|
| `id`        | u64        | Numeric repo ID      |
| `node_id`   | string?    | GraphQL node ID      |
| `name`      | string     | Repo name            |
| `full_name` | string     | `owner/repo` format  |
| `owner`     | GitHubUser | Owner object         |
| `html_url`  | string     | GitHub URL           |

---

## License

| Field     | Type      | Description            |
|-----------|-----------|------------------------|
| `key`     | string    | SPDX key               |
| `name`    | string    | License name           |
| `spdx_id` | string?   | SPDX identifier        |
| `url`     | string?   | License URL            |
| `node_id` | string?   | GraphQL node ID        |

---

## SecurityAnalysisStatus

| Field     | Type    | Description   |
|-----------|---------|---------------|
| `status`  | string  | Status value  |
| `enabled` | bool?   | Enabled flag  |

## SecurityAndAnalysis

| Field                              | Type                       | Description               |
|------------------------------------|----------------------------|---------------------------|
| `advanced_security`                | SecurityAnalysisStatus?    | Advanced security status  |
| `secret_scanning`                  | SecurityAnalysisStatus?    | Secret scanning status    |
| `secret_scanning_push_protection`  | SecurityAnalysisStatus?    | Push protection status    |

---

## Release

| Field               | Type          | Description                    |
|---------------------|---------------|--------------------------------|
| `id`                | u64           | Numeric release ID             |
| `tag_name`          | string        | Git tag name                   |
| `target_commitish`  | string        | Target branch/commit           |
| `name`              | string?       | Release title                  |
| `body`              | string?       | Release description            |
| `draft`             | bool          | Draft flag                     |
| `prerelease`        | bool          | Pre-release flag               |
| `created_at`        | string        | ISO 8601 creation date         |
| `published_at`      | string?       | ISO 8601 publish date          |
| `html_url`          | string        | GitHub release URL             |
| `assets`            | ReleaseAsset[]| Release assets                 |
| `tarball_url`       | string?       | Source tarball URL             |
| `zipball_url`       | string?       | Source zipball URL             |

---

## ReleaseAsset

| Field                  | Type    | Description             |
|------------------------|---------|-------------------------|
| `id`                   | u64     | Numeric asset ID        |
| `name`                 | string  | Filename                |
| `content_type`         | string  | MIME type               |
| `size`                 | u64     | Size in bytes           |
| `browser_download_url` | string  | Download URL            |
| `created_at`           | string  | ISO 8601 creation date  |
| `updated_at`           | string  | ISO 8601 update date    |

---

## WorkflowDefinition

| Field        | Type      | Description                 |
|--------------|-----------|-----------------------------|
| `id`         | u64       | Numeric workflow ID         |
| `node_id`    | string?   | GraphQL node ID             |
| `name`       | string    | Workflow name               |
| `path`       | string    | File path (e.g. `.github/workflows/ci.yml`) |
| `state`      | string    | State (`active`, etc.)      |
| `created_at` | string    | ISO 8601 creation date      |
| `updated_at` | string    | ISO 8601 update date        |

---

## Branch

| Field       | Type         | Description      |
|-------------|--------------|------------------|
| `name`      | string       | Branch name      |
| `commit`    | BranchCommit | Latest commit ref|
| `protected` | bool         | Protected flag   |

## BranchCommit

| Field | Type   | Description    |
|-------|--------|----------------|
| `sha` | string | Commit SHA     |
| `url` | string | Commit API URL |

---

## RateLimit

| Field       | Type                 | Description        |
|-------------|----------------------|--------------------|
| `resources` | RateLimitResources   | Per-category limits|

## RateLimitResources

| Field     | Type          | Description       |
|-----------|---------------|-------------------|
| `core`    | RateLimitItem | Core API limit    |
| `search`  | RateLimitItem | Search API limit  |
| `graphql` | RateLimitItem | GraphQL API limit |

## RateLimitItem

| Field       | Type   | Description             |
|-------------|--------|-------------------------|
| `limit`     | u32    | Max requests            |
| `remaining` | u32    | Remaining requests      |
| `reset`     | u64    | Unix timestamp of reset |
| `used`      | u32    | Requests used           |

---

## SearchResults\<T\>

Generic wrapper for GitHub search API responses.

| Field                | Type    | Description                 |
|----------------------|---------|-----------------------------|
| `total_count`        | u64     | Total matching results      |
| `incomplete_results` | bool    | Results truncated flag      |
| `items`              | T[]     | Array of result items       |

Currently used as `SearchResults<Repository>`.

---

## SyncForkResult

| Field     | Type   | Description                    |
|-----------|--------|--------------------------------|
| `status`  | string | Sync status                    |
| `message` | string | Result message                 |

---

## CompareResult

| Field           | Type          | Description               |
|-----------------|---------------|---------------------------|
| `status`        | string        | Comparison status         |
| `ahead_by`      | u32           | Commits ahead             |
| `behind_by`     | u32           | Commits behind            |
| `total_commits` | u32           | Total commits in range    |
| `files`         | CompareFile[] | Changed files             |

## CompareFile

| Field      | Type   | Description             |
|------------|--------|-------------------------|
| `sha`      | string | File SHA                |
| `filename` | string | File path               |
| `status`   | string | Change status           |

---

## WorkflowDispatchBody

| Field    | Type      | Description                         |
|----------|-----------|-------------------------------------|
| `ref`    | string    | Git ref (branch/tag) to dispatch on |
| `inputs` | string?   | JSON-encoded workflow inputs        |

---

## Contributor

| Field           | Type   | Description            |
|-----------------|--------|------------------------|
| `login`         | string | GitHub username        |
| `id`            | u64    | Numeric user ID        |
| `avatar_url`    | string | Avatar URL             |
| `html_url`      | string | GitHub profile URL     |
| `contributions` | u32    | Contribution count     |
| `type`          | string | `User` or `Bot`        |

---

## Commit

| Field      | Type             | Description       |
|------------|------------------|-------------------|
| `sha`      | string           | Commit SHA        |
| `commit`   | CommitDetails    | Commit metadata   |
| `author`   | ContributorBrief?| Author (may be null) |
| `committer`| ContributorBrief?| Committer         |

## CommitDetails

| Field      | Type         | Description       |
|------------|--------------|-------------------|
| `message`  | string       | Commit message    |
| `author`   | CommitAuthor | Author info       |
| `committer`| CommitAuthor | Committer info    |

## CommitAuthor

| Field   | Type   | Description          |
|---------|--------|----------------------|
| `name`  | string | Author name          |
| `email` | string | Author email         |
| `date`  | string | ISO 8601 commit date |

## ContributorBrief

| Field       | Type      | Description         |
|-------------|-----------|---------------------|
| `login`     | string?   | GitHub username     |
| `id`        | u64?      | Numeric user ID     |
| `avatar_url`| string?   | Avatar URL          |
| `html_url`  | string?   | GitHub profile URL  |
| `type`      | string?   | `User` or `Bot`     |

---

## Tag

| Field         | Type        | Description           |
|---------------|-------------|-----------------------|
| `name`        | string      | Tag name              |
| `commit`      | TagCommitRef| Tagged commit ref     |
| `zipball_url` | string      | Source zip URL        |
| `tarball_url` | string      | Source tarball URL    |

## TagCommitRef

| Field | Type   | Description    |
|-------|--------|----------------|
| `sha` | string | Commit SHA     |
| `url` | string | Commit API URL |

---

## Organization

| Field           | Type      | Description              |
|-----------------|-----------|--------------------------|
| `login`         | string    | Organization login name  |
| `id`            | u64       | Numeric organization ID  |
| `node_id`       | string?   | GraphQL node ID          |
| `avatar_url`    | string?   | Avatar URL               |
| `html_url`      | string?   | GitHub profile URL       |
| `name`          | string?   | Display name             |
| `company`       | string?   | Company field            |
| `blog`          | string?   | Website URL              |
| `location`      | string?   | Location                 |
| `email`         | string?   | Email address            |
| `description`   | string?   | Organization description |
| `public_repos`  | u32?      | Public repo count        |
| `public_gists`  | u32?      | Public gist count        |
| `followers`     | u32?      | Follower count           |
| `following`     | u32?      | Following count          |
| `created_at`    | string?   | ISO 8601 creation date   |
| `updated_at`    | string?   | ISO 8601 update date     |

---

## NotificationSubject

| Field                | Type   | Description                              |
|----------------------|--------|------------------------------------------|
| `title`              | string | Subject title (e.g. issue/PR title)      |
| `url`                | string | API URL for the subject                  |
| `latest_comment_url` | string | API URL for the latest comment           |
| `type`               | string | Subject type (`Issue`, `PullRequest`, `Commit`, `Release`, etc.) |

---

## NotificationThread

| Field              | Type                   | Description                              |
|--------------------|------------------------|------------------------------------------|
| `id`               | string                 | Thread ID (string GUID)                  |
| `repository`       | Repository             | The repository this notification belongs to |
| `subject`          | NotificationSubject    | The subject of the notification          |
| `reason`           | string                 | Reason for the notification (e.g. `mention`, `subscribed`, `review_requested`) |
| `unread`           | bool                   | Whether the notification is unread       |
| `updated_at`       | string                 | ISO 8601 last update time                |
| `last_read_at`     | string?                | ISO 8601 time when last read             |
| `url`              | string                 | API URL for the thread                   |
| `subscription_url` | string                 | API URL for the thread subscription      |

---

## ThreadSubscription

| Field            | Type      | Description                            |
|------------------|-----------|----------------------------------------|
| `subscribed`     | bool      | Whether the user is subscribed         |
| `ignored`        | bool      | Whether the thread is ignored          |
| `reason`         | string?   | Reason for subscription state          |
| `created_at`     | string?   | ISO 8601 creation date                 |
| `url`            | string    | API URL for the subscription           |
| `thread_url`     | string?   | API URL for the thread                 |
| `repository_url` | string?   | API URL for the repository             |

---

## MarkNotificationsResponse

| Field     | Type      | Description                            |
|-----------|-----------|----------------------------------------|
| `message` | string    | Status message                         |
| `url`     | string?   | Relevant URL (repo-scoped only)        |

---

# Enums

## Platform

```text
Android | Macos | Windows | Linux | All
```

Used to filter search results by platform topic tag.

---

## Language

```text
Kotlin | Java | JavaScript | TypeScript | Python | Swift | Rust | Go |
CSharp | CPlusPlus | C | Dart | Ruby | PHP | All
```

Used to filter search results by programming language.

---

## SortBy

```text
BestMatch | MostStars | MostForks
```

Controls the sort field for search queries.

---

## SortOrder

```text
Descending | Ascending
```

Controls sort direction for search queries.

---

## TimeRange

```text
daily | weekly | monthly
```

Placeholder for trending time range (currently unused in query building).

---

## DiscoveryChannel

```text
most_stars | most_forks | trending | hot_release | most_popular | topic | search
```

Used to identify which discovery channel triggered a search. Each variant maps to a label string (e.g. `most-stars`, `hot-release`).
