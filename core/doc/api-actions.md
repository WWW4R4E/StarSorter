# Actions API

Base URL: `https://api.github.com`

---

## GET /repos/{owner}/{repo}/actions/workflows

List all GitHub Actions workflows in a repository.

**Response:** Wrapped in GitHub API envelope:

```json
{
  "total_count": u32,
  "workflows": [WorkflowDefinition, ...]
}
```

The client unwraps and returns `[]`[`WorkflowDefinition`](types.md#workflowdefinition) directly.

**Client method:** `getRepositoryWorkflows(owner, repo)`

**Export:** `gh_get_workflows(wrapper, owner, repo)` → serialized JSON array (unwrapped)

---

## POST /repos/{owner}/{repo}/actions/workflows/{workflow_path}/dispatches

Trigger a workflow dispatch event.

**Request body:**

```json
{ "ref": "main" }
```

**Response:** `void` (204 No Content)

**Client method:** `triggerWorkflowRun(owner, repo, workflow_path, ref)`

**Export:** `gh_trigger_workflow(wrapper, owner, repo, workflow_path, ref)` → 204 No Content
