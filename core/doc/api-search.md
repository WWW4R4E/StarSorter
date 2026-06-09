# Search API

Base URL: `https://api.github.com`

---

## GET /search/repositories

Search repositories on GitHub.

**Query params:**

| Param      | Type                                                         | Description                        |
|------------|--------------------------------------------------------------|------------------------------------|
| `q`        | string                                                       | Search query (GitHub search syntax)|
| `sort`     | `stars`, `forks`, or empty (`BestMatch`)                     | Sort field                         |
| `order`    | `desc`, `asc`                                                | Sort order                         |
| `page`     | u32                                                          | Page number                        |
| `per_page` | u8                                                           | Results per page                   |

**Response:** [`SearchResults<Repository>`](types.md#searchresultst)

**Client method:** `searchRepos(query, sort, order, page, per_page)` (internal)

---

## Search Wrappers

These are convenience methods that build queries automatically.

| Method                    | Query                           | Sort         | Page | Per Page | Export |
|---------------------------|---------------------------------|--------------|------|----------|--------|
| `searchMostStars()`       | `stars:>1000`                   | MostStars    | var  | var      | `gh_search_most_stars(wrapper, page, per_page)` |
| `searchMostForks()`       | `forks:>1000`                   | MostForks    | var  | var      | `gh_search_most_forks(wrapper, page, per_page)` |
| `getMostPopular()`        | `stars:>10000`                  | MostStars    | var  | var      | `gh_get_most_popular(wrapper, page, per_page)` |
| `searchTrending(lang, range)` | `stars:>100+language:{x}+pushed:>2025-01-01` | MostStars | 1 | 30 | `gh_search_trending(wrapper, language, range)` |
| `getHotReleaseRepos()`    | `pushed:>{date}+stars:>100`     | MostStars    | var  | var      | `gh_get_hot_release_repos(wrapper, page, per_page)` |

All return serialized JSON.

---

## searchRepositories

Combined search with language and platform filters.

**Parameters:**

| Param      | Type                                     | Description          |
|------------|------------------------------------------|----------------------|
| `query`    | string                                   | Base search query    |
| `platform` | [`Platform`](types.md#platform)          | Filter by platform   |
| `language` | [`Language`](types.md#language)          | Filter by language   |
| `sort`     | [`SortBy`](types.md#sortby)              | Sort field           |
| `order`    | [`SortOrder`](types.md#sortorder)        | Sort order           |
| `page`     | u32                                      | Page number          |
| `per_page` | u8                                       | Results per page     |

**Response:** [`SearchResults<Repository>`](types.md#searchresultst)

**Client method:** `searchRepositories(query, platform, language, sort, order, page, per_page)`

**Export:** `gh_search_repositories(wrapper, query, platform, language, sort, order, page, per_page)` → serialized JSON

---

## searchByTopic / getTopicRepositories

Search repositories by topic keywords.

`searchByTopic(keywords, page, per_page)` — passes multiple `topic:{kw}` terms.

**Export:** `gh_search_by_topic(wrapper, topic, page, per_page)` → serialized JSON

`getTopicRepositories(page, per_page)` — hardcoded to AI/ML topics: `artificial-intelligence`, `machine-learning`, `ai`.

**Export:** `gh_get_topic_repos(wrapper, page, per_page)` → serialized JSON

---

## searchDailyDevs

Search developers with >1000 followers.

**Export:** `gh_search_daily_devs(wrapper, page, per_page)` → serialized JSON
