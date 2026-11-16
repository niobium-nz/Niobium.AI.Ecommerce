# Claude Cowork and other task hosts

`AGENT.md` defines portable behavior. `agent.yaml` is descriptive, not a Claude
plugin manifest, and the installation kit is not a universal native import file.
No Cowork deployment or live integration test is claimed by this repository.

1. Bind the complete `ecommerce-product-discoverer` folder as task/project resources
   and use `AGENT.md` as instructions through the host's supported mechanism.
2. Select the single `ecommerce-cluster-ads` skill through that agent/task's skill
   configuration. The repository source and kit keep it separate from agent files.
   Do not create a second nested copy. Resolve its current resources through that
   selection; never persist the host's mount paths or substitute a global namesake.
3. Register `skill-bindings.json` schema URIs against that selected package. A host
   with resource APIs may use an in-memory registry. The optional file-based helper
   accepts a resource directory valid only for the current execution.
4. Supply the existing `discovery_folder_id` and IANA timezone as deployment
   settings. Bind the approved search and Drive actions by actual semantics, not
   ChatGPT namespaces. Read-only Drive or native document creation is insufficient.
5. Run the resource/schema checks in `references/validation.md`. The provided
   offline wheels support the documented Linux Python targets; other hosts need an
   equivalent Draft 2020-12 validator, local references and date/time format checks.
   Do not attempt a blocked network installation or call JSON parsing validation.
6. Apply the same optional Drive metadata policy and mandatory persistence checks.
   Respect the host's approvals and do not run paid/write tests merely for setup.
7. Where mocked host execution is available, run `tests/host-acceptance.md`; otherwise
   report it untested. Keep installation evidence separate from live-run evidence.

For updates, replace agent resources and validation support as a coherent set,
keep the active deployment settings, and change the selected skill only deliberately.
Rerun compatibility checks after a skill update. Wire contracts and business stages
remain unchanged; source examples are not a production execution trace.

Check the target host's current
[skill](https://support.claude.com/en/articles/12512180-use-skills-in-claude) and
[plugin](https://support.claude.com/en/articles/13837440-use-plugins-in-claude)
capabilities rather than assuming a shared vendor import format.
