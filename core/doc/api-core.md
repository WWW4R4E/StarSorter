# Core/Meta API

Base URL: `https://api.github.com`

---

## GET /rate_limit

Get the current API rate limit status for the authenticated user.

**Response:** [`RateLimit`](types.md#ratelimit)

**Client method:** `checkRateLimit()`

**Export:** `gh_check_rate_limit(wrapper)` → raw GitHub JSON
