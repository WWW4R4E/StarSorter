# Notifications API

Base URL: `https://api.github.com`

---

## GET /notifications

List notifications for the authenticated user, sorted by most recently updated.

**Query params:**

| Param           | Type    | Default | Description                                         |
|-----------------|---------|---------|-----------------------------------------------------|
| `all`           | bool    | false   | Show notifications marked as read                   |
| `participating` | bool    | false   | Only notifications user participates in              |
| `since`         | string  | —       | ISO 8601 timestamp; results updated after this time  |
| `before`        | string  | —       | ISO 8601 timestamp; results updated before this time |
| `page`          | integer | 1       | Page number                                          |
| `per_page`      | integer | 50      | Items per page (max 50)                              |

**Response:** `[]`[`NotificationThread`](types.md#notificationthread)

**Client method:** `getNotifications(all, participating, since, before, page, per_page)`

**Export:** `gh_get_notifications(wrapper, all, participating, since, before, page, per_page)` → raw GitHub JSON

---

## PUT /notifications

Mark all notifications as read for the authenticated user.

**Request body:**

```json
{ "last_read_at": "2022-06-10T00:00:00Z" }
```

Omit `last_read_at` to mark all as read.

**Response:** [`MarkNotificationsResponse`](types.md#marknotificationsresponse)

**Client method:** `markNotificationsRead(last_read_at)`

**Export:** `gh_mark_notifications_read(wrapper, last_read_at)` → raw GitHub JSON

---

## GET /notifications/threads/{thread_id}

Get a single notification thread.

**Path params:**

| Param       | Type   | Description             |
|-------------|--------|-------------------------|
| `thread_id` | string | Notification thread ID  |

**Response:** [`NotificationThread`](types.md#notificationthread)

**Client method:** `getThread(thread_id)`

**Export:** `gh_get_thread(wrapper, thread_id)` → raw GitHub JSON

---

## PATCH /notifications/threads/{thread_id}

Mark a thread as read.

**Response:** 205 Reset Content

**Client method:** `markThreadRead(thread_id)`

**Export:** `gh_mark_thread_read(wrapper, thread_id)` → raw GitHub JSON

---

## DELETE /notifications/threads/{thread_id}

Mark a thread as done.

**Response:** 204 No Content

**Client method:** `markThreadDone(thread_id)`

**Export:** `gh_mark_thread_done(wrapper, thread_id)` → raw GitHub JSON

---

## GET /notifications/threads/{thread_id}/subscription

Get the authenticated user's subscription to a thread.

**Response:** [`ThreadSubscription`](types.md#threadsubscription)

**Client method:** `getThreadSubscription(thread_id)`

**Export:** `gh_get_thread_subscription(wrapper, thread_id)` → raw GitHub JSON

---

## PUT /notifications/threads/{thread_id}/subscription

Set a thread subscription (subscribe, ignore, or unignore).

**Request body:**

```json
{ "ignored": false }
```

**Response:** [`ThreadSubscription`](types.md#threadsubscription)

**Client method:** `setThreadSubscription(thread_id, ignored)`

**Export:** `gh_set_thread_subscription(wrapper, thread_id, ignored)` → raw GitHub JSON

---

## DELETE /notifications/threads/{thread_id}/subscription

Delete a thread subscription (mute future notifications).

**Response:** 204 No Content

**Client method:** `deleteThreadSubscription(thread_id)`

**Export:** `gh_delete_thread_subscription(wrapper, thread_id)` → raw GitHub JSON

---

## GET /repos/{owner}/{repo}/notifications

List notifications for the authenticated user in a specific repository.

**Query params:**

| Param           | Type    | Default | Description                                         |
|-----------------|---------|---------|-----------------------------------------------------|
| `all`           | bool    | false   | Show notifications marked as read                   |
| `participating` | bool    | false   | Only notifications user participates in              |
| `since`         | string  | —       | ISO 8601 timestamp; results updated after this time  |
| `before`        | string  | —       | ISO 8601 timestamp; results updated before this time |
| `per_page`      | integer | 30      | Items per page (max 100)                             |
| `page`          | integer | 1       | Page number                                          |

**Response:** `[]`[`NotificationThread`](types.md#notificationthread)

**Client method:** `getRepositoryNotifications(owner, repo, all, participating, since, before, per_page, page)`

**Export:** `gh_get_repo_notifications(wrapper, owner, repo, all, participating, since, before, per_page, page)` → raw GitHub JSON

---

## PUT /repos/{owner}/{repo}/notifications

Mark all notifications in a repository as read.

**Request body:**

```json
{ "last_read_at": "2019-01-01T00:00:00Z" }
```

Omit `last_read_at` to mark all as read.

**Response:** [`MarkNotificationsResponse`](types.md#marknotificationsresponse)

**Client method:** `markRepositoryNotificationsRead(owner, repo, last_read_at)`

**Export:** `gh_mark_repo_notifications_read(wrapper, owner, repo, last_read_at)` → raw GitHub JSON
