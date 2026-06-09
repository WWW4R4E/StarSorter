# Organizations API

Base URL: `https://api.github.com`

---

## GET /orgs/{org}

Get an organization's public profile.

**Response:** [`Organization`](types.md#organization)

**Client method:** `getOrganization(org)`

**Export:** `gh_get_organization(wrapper, org)` → raw GitHub JSON
