# Supplementary material S1: 1150 architecture audit

This release accompanies “Separating observation quality and evidence provenance in cutter-wear monitoring: a reproducible architecture audit”. All observations are retained synthetic computational records. No industrial dataset, calibration or physical-unit conversion is supplied or claimed.

## Reproduce the new analysis

Requirements: Python 3.10 or later. The audit, file verification and algebra checks use only the Python standard library. Unzip into a writable folder and run from its root:

    python scripts/verify_release.py
    python scripts/audit_retained_data.py --root . --out reproduced/scientific_audit
    python scripts/verify_algebra.py

Compare the CSV/JSON in reproduced/scientific_audit with qa/scientific_audit. The algebra script writes qa/ALGEBRA_CHECKS.json; it exhaustively sums a finite probability space and checks selected sigmoid cases without generating observations. Floating-point results should be compared to 1e-12 where relevant; no cross-platform byte identity of floating-point formatting is promised.

Optional graphic reproduction:

    python -m pip install -r requirements-figures.txt
    python scripts/make_figures.py

Graphics are regenerated from qa/scientific_audit/scenario_accounting.csv into working/figures. To use newly recomputed results, replace that CSV with the reproduced counterpart first. Figure 1 is a conceptual evidence-contract diagram; Figures 2 and 3 are data-derived. The manuscript uses the supplied 300-dpi PNGs; vector PDFs are also supplied.

## Inputs and boundaries

- data/synthetic/generated: 72,000 latent state epochs, 432,000 channel records and 72,000 event records; 8 scenarios x 30 trajectories x 300 steps x 6 channels.
- data/synthetic/configs: retained generation assumptions and settings. These are software parameters, not physically identified sensor characteristics.
- generated/paper01: old aggregate outputs retained for exact reconciliation. The old traceability_completeness column is channel availability.
- reviewer_repair_extended/paper01_measurement_tests: six historical summary CSVs. Their producing extension generator and row-level predictions are not established. Reaggregating these inputs is not reproducing a prediction/ablation experiment.
- src/WearVision.SyntheticStand.Core: original C# source for inspection, including the shared-drop and channel-specific-drop expressions. It targets .NET 8.0. It is not executed by the new audit and no reconstruction of the historical extension generator is claimed.
- historical_specification: the earlier unpublished diagnostic equations and selected explanatory paragraphs, retained to identify exactly what the conditional sign check audits.
- qa: expected audit outputs and deterministic algebra checks.

The new audit checks a specified set of keys, joins, masks and quality conditions. It does not establish physical accuracy, calibration, reference wear, metrological traceability, valid uncertainty coverage, network timing, maintenance effectiveness or field generalization. SHA-256 checks establish consistency with this manifest; they do not authenticate physical origins.

## Evidence identity and use

SHA256SUMS.csv records every payload file except the manifest itself. Keep it unchanged when checking the supplied payload; regenerated figures or altered analysis inputs will intentionally differ. No public repository URL or DOI is asserted. The release is supplied for scholarly inspection with the manuscript; no new open-source or data licence is imposed on behalf of the authors. Attribution and any wider redistribution terms should be agreed with the corresponding author, Ivan V. Brovchenko (broviva@mail.ru).
