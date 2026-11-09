# Input contract

## Accepted forms

Accept current conversation photo attachments directly. Accept an explicit list
of photo URLs or accessible paths. Accept a JSON array of source descriptors, or
an object with `reference_images` and optional constraints. Normalize these forms
into the same source set without requiring the user to reformat valid attachments.
The schema covers structured input, not attachment binary transport.

A string source is an HTTP(S) image URL or a local path. A typed source object is:

```json
{
  "source_type": "attachment",
  "source": "front.jpg",
  "view_hint": "front",
  "notes": "The product is the hinged object, not the stand."
}
```

Use `source_type` values `attachment`, `local_path`, or `url`. The `source` for an
attachment must identify an actually supplied image without ambiguity. A filename
can identify a unique attachment, but cannot establish filesystem access. A host
may resolve its own attachment handle; do not fabricate or guess one. In a string
list, a filename is a path unless it actually resolves to a supplied attachment.
Relative paths resolve only against a known, authorized working directory. Never
search unrelated files to locate a guessed source.

Use URL sources that lead to an actual image. A webpage, login page, missing-file
response, or bytes from a non-image file does not satisfy the requirement. Do not
infer media type from a filename alone. Allow ordinary supplied-image redirects
when the host authorizes them; do not crawl for alternative product evidence.
Never execute downloaded content. Do not bypass access controls or expose signed
URLs, account credentials or local operational paths inside the board.

## Optional constraints

| Field | Default | Interpretation |
| --- | --- | --- |
| `product_notes` | Empty string | Identify the intended product, SKU, state, or an ambiguous prop. Text does not prove hidden visual details. |
| `required_views` | Empty list | User-required views. A missing required view requires more evidence or explicit revision, even in incomplete-coverage mode. |
| `critical_features` | Empty list | Prioritize clearly showing these evidenced features; do not manufacture them. |
| `allow_incomplete_coverage` | `false` | Explicitly permit a board of supported views only, with no invented sides. Does not waive SKU, debranding, truth or minimum multi-view requirements. |

All JSON keys use lower snake case. Reject unknown structured fields that would
silently change behavior; explain the supported fields. Treat view hints as
routing aids, not evidence that a side was actually photographed. Duplicate source
references are permitted but deduplicated before evaluation; repeated views never
increase coverage. One source may be a real photographic collage with several
observed views, so do not require two input files solely to achieve two views.

If direct attachments are mixed with JSON, combine only photos the user actually
selected for this task. Never include saved personal portraits, unrelated older
images, or every file in the workspace. Resolve which batch is intended when it
is unclear. Preserve original files and retain a stable mapping from each candidate
board view back to its supporting source region.

## Examples and limits

See [the mixed-source example](../examples/input.mixed.json) and
[the path-list example](../examples/input.paths.json). They are fictional reference
locations, not usable images. Direct attachments need no artificial JSON wrapper.

Schema validation checks syntax and data types only. It cannot establish file
existence, access, a real image, SKU consistency, angle coverage, or editing
capability. Perform those checks using the actual supplied images and host tools.
