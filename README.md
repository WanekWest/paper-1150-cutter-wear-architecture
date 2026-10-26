# Paper 1150 cutter wear monitoring architecture audit

Supporting data and code for **Separating observation quality and evidence provenance in cutter-wear monitoring: a reproducible architecture audit**, by Ivan V. Brovchenko, Evgeny B. Mazakov and Maria R. Kiba.

This repository preserves an existing synthetic archive and the deterministic audit supporting the manuscript. It supplies observation records, latent states, event records, configurations, audit scripts, retained generator source, derived results, figures and a SHA-256 manifest. No new experiment, industrial validation, measurement calibration or diagnostic superiority is claimed. Historical extension summaries are distinguished from the reproducible deterministic audit in the retained documentation.

## Complete release

The canonical, unchanged supplementary archive is available under [Releases](https://github.com/WanekWest/paper-1150-cutter-wear-architecture/releases/tag/1150-2026-10-21-audit1). Download `1150_S1_DATA_CODE_2026-10-21.zip` and `SHA256SUMS.txt`. The archive contains the same 52 retained files supplied in `S1_1150_ARCHITECTURE_AUDIT/` here; its nested manifest verifies 51 payload files. `RELEASE_METADATA.json` records the archive hash and size. No DOI is claimed.

The frozen S1 README describes the earlier manuscript-supplied package before this public deposit. Its historical statement that no repository URL is asserted is retained to preserve the original manifest. The current public deposit and release are identified by this top-level README and `RELEASE_METADATA.json`.

## Inspect and reproduce

Start with [the retained S1 README](S1_1150_ARCHITECTURE_AUDIT/README.md) and [reanalysis protocol](S1_1150_ARCHITECTURE_AUDIT/REANALYSIS_PROTOCOL.md). Run the documented commands from that folder. The portable Python audit inspects retained inputs; original C# generator source is included for inspection and does not need to be executed to verify the audit. Do not regenerate the retained archive as part of verification.

## Scientific limits and rights

All observations in this release are synthetic. Computational consistency and file provenance do not establish physical measurement accuracy or metrological traceability. See the manuscript and retained protocol for the full limitations.

Original copyright and licence notices, where present, remain in force. Public availability is not a new permission to reuse the code or data under MIT, CC BY or any other licence. This packaging operation adds no such licence. The journal article licence, if the manuscript is accepted, is separate.

The repository is a public supporting-material deposit for a manuscript in preparation; acceptance or publication by a journal is not asserted.
