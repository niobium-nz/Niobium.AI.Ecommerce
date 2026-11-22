# Checkpoint runner

The portable kit includes `agent-files/ecommerce-discovery-looper/run_looper.py`.
Run it with Python 3.11 or newer and `jsonschema` 4.x with its format dependencies.
The adjacent contract-resources and offline-validation-support folders provide the
contract definitions and resolver. Use an authenticated Google Drive access token
in `GOOGLE_DRIVE_ACCESS_TOKEN` scoped for read-only access. The configured folder
and researcher binding must refer to the same deployment.

Place the SQLite journal on durable local storage owned by the coordinator. Use
one journal for every process in that coordinator domain. The database retains
the full source and child results. The external coordinator must enforce the
registry's writer policy for other hosts and manual researcher invocations.

From the kit root, the first command pins the source bytes and returns a loop ID.

```sh
python3 agent-files/ecommerce-discovery-looper/run_looper.py \
  --journal /path/to/private/ecommerce.sqlite --binding /path/to/researcher-host \
  start --config agent-files/deployment-config.json --job-id JOB_ID
```

Run for a bounded window. Repeat using the same loop ID and journal after a pause.
Each window can submit and poll multiple products, with one child in flight. It
returns compact progress. A running child stays attached to its saved invocation
ID when the window ends. The next window polls that ID before dispatching another
product. `advance` performs one submit or poll. `status` reads the checkpoint.
Once terminal, `result` returns the full batch result.

```sh
python3 agent-files/ecommerce-discovery-looper/run_looper.py \
  --journal /path/to/private/ecommerce.sqlite --binding /path/to/researcher-host \
  run --loop-id LOOP_ID --time-budget 300
python3 agent-files/ecommerce-discovery-looper/run_looper.py \
  --journal /path/to/private/ecommerce.sqlite result --loop-id LOOP_ID
```

A submitting checkpoint without an invocation ID or a recovery hold requires
operator reconciliation with the host using the saved intent ID. An ambiguous
submit is never sent again. Keep the journal and host records for reconciliation.

## Host binding

Configure `--binding` to point to an installed, trusted executable. Each call
receives one JSON object on stdin and writes one JSON object on stdout. Give the
adapter a short read/submit timeout. Long-running work belongs to the child host.

| Action | Request fields after action | Required reply |
| --- | --- | --- |
| preflight | config | Exact config echo, registry_file_id, serialized_writers=true, terminal_results=true, recovery_clear=true. Assert these only after checking the deployed researcher, invocation transport and shared coordinator. |
| submit | loop_id, intent_id, source_index, registry_file_id, config, input | intent_id and a stable, distinct invocation_id. Correlate the single isolated researcher call to the pre-existing intent. |
| poll | Same dispatch identifiers and config, plus invocation_id | intent_id, invocation_id and state=pending, or state=settled with quiescent=true, recovery_clear and the exact terminal researcher result. |

The adapter must enforce single submission, honor transport approvals, and
leave the full researcher as the only writer of candidates and processed IDs.
The production guard cannot depend on a self-reported model flag. Keyword
subagents within the researcher can run under its own supported limits.

For ChatGPT, the official Workspace Agents API can start a run and poll its
status. The structured terminal result and a shared writer/coordinator binding
still need to be supplied by the deployment. A run status or candidate filename
alone is insufficient to advance.
[OpenAI Workspace Agents trigger API](https://developers.openai.com/workspace-agents/trigger-runs)
