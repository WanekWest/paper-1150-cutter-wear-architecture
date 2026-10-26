namespace WearVision.SyntheticStand.Core;

public sealed class SyntheticExperimentGenerator
{
    private const string RunId = "wave02_seed_20260616";

    public SyntheticGenerationSummary Generate(SyntheticExperimentConfig config, string outputRoot, string configHash)
    {
        var errors = SyntheticExperimentValidator.Validate(config);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }

        var generatedRoot = Path.Combine(outputRoot, "generated");
        var paper01Root = Path.Combine(generatedRoot, "paper01");
        var paper02Root = Path.Combine(generatedRoot, "paper02");
        var paper03Root = Path.Combine(generatedRoot, "paper03");
        var traceabilityRoot = Path.Combine(outputRoot, "traceability");
        Directory.CreateDirectory(generatedRoot);
        Directory.CreateDirectory(paper01Root);
        Directory.CreateDirectory(paper02Root);
        Directory.CreateDirectory(paper03Root);
        Directory.CreateDirectory(Path.Combine(outputRoot, "metrics"));
        Directory.CreateDirectory(traceabilityRoot);

        var stateCounts = new Dictionary<(string Scenario, string State), long>();
        var observationStats = new Dictionary<(string Scenario, string Channel), ObservationAccumulator>();
        var estimatorStats = new Dictionary<(string Scenario, string Estimator), EstimatorAccumulator>();
        var policyStats = new Dictionary<(string Scenario, string Policy), PolicyAccumulator>();
        var featureTrendStats = new Dictionary<(string Scenario, string Channel), TrendAccumulator>();

        using var runManifest = new CsvSink(Path.Combine(generatedRoot, "run_manifest.csv"), "run_id", "config_hash", "seed", "generator_version", "scenario_id", "scenario_label", "units", "steps", "artifact_label", "limitations");
        using var latentState = new CsvSink(Path.Combine(generatedRoot, "latent_state.csv"), "run_id", "config_hash", "seed", "scenario_id", "unit_id", "step", "wear_state_index", "wear_velocity_state", "damage_mode_index", "health_regime_label", "rul_steps_remaining", "operating_load", "rock_abrasivity", "sensor_quality_score", "replacement_reset_flag", "paper_id");
        using var observations = new CsvSink(Path.Combine(generatedRoot, "observations.csv"), "run_id", "config_hash", "seed", "scenario_id", "unit_id", "step", "channel", "feature_name", "feature_value", "quality_score", "quality_flag", "missing_flag", "paper_id");
        using var events = new CsvSink(Path.Combine(generatedRoot, "events_and_labels.csv"), "run_id", "config_hash", "seed", "scenario_id", "unit_id", "step", "chip_event", "shock_event", "replacement_event", "false_alarm_injected", "sensor_drop", "dangerous_wear_flag", "event_label", "paper_id");
        using var estimates = new CsvSink(Path.Combine(generatedRoot, "filter_estimates.csv"), "run_id", "config_hash", "seed", "scenario_id", "unit_id", "step", "estimator_id", "observed_wear_proxy", "estimated_wear", "estimate_lower", "estimate_upper", "uncertainty", "residual", "paper_id");
        using var rulPredictions = new CsvSink(Path.Combine(generatedRoot, "rul_predictions.csv"), "run_id", "config_hash", "seed", "scenario_id", "unit_id", "step", "estimator_id", "true_rul_steps", "predicted_rul_steps", "signed_rul_error", "absolute_rul_error", "prediction_lower", "prediction_upper", "paper_id");
        using var decisions = new CsvSink(Path.Combine(generatedRoot, "maintenance_decisions.csv"), "run_id", "config_hash", "seed", "scenario_id", "unit_id", "step", "policy_id", "selected_action", "risk_score", "reason_code", "follow_up_action", "trace_row_id", "paper_id");
        using var metricsSummary = new CsvSink(Path.Combine(generatedRoot, "metrics_summary.csv"), "run_id", "config_hash", "paper_id", "scenario_id", "metric_scope", "metric_name", "metric_value", "artifact_label");
        using var figureTraceability = new CsvSink(Path.Combine(generatedRoot, "figure_traceability.csv"), "visual_id", "paper_id", "source_data_file", "source_config", "source_script_or_generator", "generated_or_conceptual", "allowed_claim", "forbidden_claim", "config_hash", "seed");

        using var paper01State = new CsvSink(Path.Combine(paper01Root, "state_scenarios.csv"), "run_id", "config_hash", "scenario_id", "state_label", "row_count", "paper_id", "artifact_label");
        using var paper01Feature = new CsvSink(Path.Combine(paper01Root, "feature_matrix.csv"), "run_id", "config_hash", "scenario_id", "channel", "feature_name", "mean_first_half", "mean_second_half", "trend_delta", "missing_rate", "paper_id", "artifact_label");
        using var paper01Kpi = new CsvSink(Path.Combine(paper01Root, "kpi_summary.csv"), "run_id", "config_hash", "scenario_id", "state_count", "mean_data_quality", "traceability_completeness", "synthetic_dataset_label", "paper_id");
        using var paper01Trace = new CsvSink(Path.Combine(paper01Root, "data_quality_traceability.csv"), "run_id", "config_hash", "scenario_id", "unit_id", "step", "channel", "quality_score", "missing_flag", "source_data_file", "paper_id");

        using var paper02Latent = new CsvSink(Path.Combine(paper02Root, "latent_vs_estimated_wear.csv"), "run_id", "config_hash", "scenario_id", "unit_id", "step", "wear_state_index", "estimated_wear", "estimator_id", "residual", "paper_id");
        using var paper02Rul = new CsvSink(Path.Combine(paper02Root, "rul_prediction_trace.csv"), "run_id", "config_hash", "scenario_id", "unit_id", "step", "true_rul_steps", "predicted_rul_steps", "estimator_id", "paper_id");
        using var paper02Intervals = new CsvSink(Path.Combine(paper02Root, "prediction_intervals.csv"), "run_id", "config_hash", "scenario_id", "unit_id", "step", "estimator_id", "estimate_lower", "estimate_upper", "covered_true_wear", "paper_id");
        using var paper02Metrics = new CsvSink(Path.Combine(paper02Root, "filter_metrics_by_scenario.csv"), "run_id", "config_hash", "scenario_id", "estimator_id", "rmse", "mae", "rul_absolute_error", "signed_rul_error", "false_alarm_rate", "missed_dangerous_wear_rate", "detection_lead_time", "prediction_interval_coverage_probability", "mean_prediction_interval_width", "paper_id");
        using var paper02Ablation = new CsvSink(Path.Combine(paper02Root, "sensor_ablation_metrics.csv"), "run_id", "config_hash", "scenario_id", "channel", "mean_quality", "missing_rate", "assumed_rmse_penalty", "paper_id");

        using var paper03Definitions = new CsvSink(Path.Combine(paper03Root, "policy_definitions.csv"), "run_id", "config_hash", "policy_id", "policy_family", "replacement_interval_steps", "wear_threshold", "uncertainty_threshold", "risk_threshold", "horizon_steps", "paper_id");
        using var paper03Ledger = new CsvSink(Path.Combine(paper03Root, "event_ledger_summary.csv"), "run_id", "config_hash", "scenario_id", "policy_id", "early_replacement_count", "late_replacement_count", "missed_dangerous_wear_count", "false_alarm_count", "dangerous_time_exposure", "paper_id");
        using var paper03Comparison = new CsvSink(Path.Combine(paper03Root, "policy_comparison_metrics.csv"), "run_id", "config_hash", "scenario_id", "policy_id", "mean_utilization_at_replacement", "expected_cost", "expected_risk", "alarm_precision", "alarm_recall", "paper_id");
        using var paper03Risk = new CsvSink(Path.Combine(paper03Root, "risk_curves.csv"), "run_id", "config_hash", "scenario_id", "policy_id", "risk_bucket", "mean_risk", "action_rate", "paper_id");
        using var paper03Frontier = new CsvSink(Path.Combine(paper03Root, "policy_frontier.csv"), "run_id", "config_hash", "scenario_id", "policy_id", "frontier_x_expected_cost", "frontier_y_expected_risk", "utilization", "paper_id");
        using var paper03Timeline = new CsvSink(Path.Combine(paper03Root, "scenario_timeline_actions.csv"), "run_id", "config_hash", "scenario_id", "unit_id", "step", "policy_id", "selected_action", "risk_score", "reason_code", "paper_id");
        using var traceabilityManifest = new CsvSink(Path.Combine(traceabilityRoot, "artifact_manifest.csv"), "artifact_id", "relative_path", "artifact_type", "paper_id", "source_config", "config_hash", "seed", "label");

        foreach (var scenario in config.Scenarios)
        {
            runManifest.Write(RunId, configHash, config.MasterSeed, config.GeneratorVersion, scenario.ScenarioId, scenario.DisplayName, config.UnitsPerScenario, config.StepsPerUnit, "synthetic dataset; computational experiment; model-based evaluation", "No industrial validation; no production deployment; no economic effect.");
        }

        foreach (var policy in config.Policies)
        {
            paper03Definitions.Write(RunId, configHash, policy.PolicyId, policy.PolicyFamily, policy.ReplacementIntervalSteps, policy.WearThreshold, policy.UncertaintyThreshold, policy.RiskThreshold, policy.HorizonSteps, "paper_03");
        }

        for (var scenarioIndex = 0; scenarioIndex < config.Scenarios.Count; scenarioIndex++)
        {
            var scenario = config.Scenarios[scenarioIndex];
            for (var unitIndex = 0; unitIndex < config.UnitsPerScenario; unitIndex++)
            {
                GenerateUnit(config, scenario, scenarioIndex, unitIndex, configHash, latentState, observations, events, estimates, rulPredictions, decisions, paper01Trace, paper02Latent, paper02Rul, paper02Intervals, paper03Timeline, stateCounts, observationStats, estimatorStats, policyStats, featureTrendStats);
            }
        }

        WritePaper01Outputs(config, configHash, stateCounts, observationStats, featureTrendStats, paper01State, paper01Feature, paper01Kpi, metricsSummary);
        WritePaper02Outputs(configHash, estimatorStats, observationStats, paper02Metrics, paper02Ablation, metricsSummary);
        WritePaper03Outputs(configHash, policyStats, paper03Ledger, paper03Comparison, paper03Risk, paper03Frontier, metricsSummary);
        WriteFigureTraceability(config, configHash, figureTraceability, traceabilityManifest);

        return new SyntheticGenerationSummary(RunId, configHash, config.MasterSeed, config.Scenarios.Count, config.Scenarios.Count * config.UnitsPerScenario, config.Scenarios.Count * config.UnitsPerScenario * config.StepsPerUnit, config.SensorChannels.Count, config.Policies.Count, "synthetic dataset; computational experiment; model-based evaluation");
    }

    private static void GenerateUnit(
        SyntheticExperimentConfig config,
        ScenarioConfig scenario,
        int scenarioIndex,
        int unitIndex,
        string configHash,
        CsvSink latentState,
        CsvSink observations,
        CsvSink events,
        CsvSink estimates,
        CsvSink rulPredictions,
        CsvSink decisions,
        CsvSink paper01Trace,
        CsvSink paper02Latent,
        CsvSink paper02Rul,
        CsvSink paper02Intervals,
        CsvSink paper03Timeline,
        Dictionary<(string Scenario, string State), long> stateCounts,
        Dictionary<(string Scenario, string Channel), ObservationAccumulator> observationStats,
        Dictionary<(string Scenario, string Estimator), EstimatorAccumulator> estimatorStats,
        Dictionary<(string Scenario, string Policy), PolicyAccumulator> policyStats,
        Dictionary<(string Scenario, string Channel), TrendAccumulator> featureTrendStats)
    {
        var rng = new DeterministicRandom(config.MasterSeed + scenarioIndex * 10_000 + unitIndex * 101);
        var unitId = $"{scenario.ScenarioId}_unit_{unitIndex + 1:D2}";
        var wear = 0.035 + rng.NextDouble() * 0.030;
        var damage = 0.015 + rng.NextDouble() * 0.020;
        var baseVelocity = 0.0018 * scenario.WearRateMultiplier * (0.90 + rng.NextDouble() * 0.25);
        var estimatorState = new UnitEstimatorState();
        var resetNextStep = false;

        for (var step = 0; step < config.StepsPerUnit; step++)
        {
            if (resetNextStep)
            {
                wear = 0.040 + rng.NextDouble() * 0.020;
                damage = 0.010;
                resetNextStep = false;
            }

            var normalizedStep = step / (double)Math.Max(1, config.StepsPerUnit - 1);
            var mixedMultiplier = scenario.MixedRegime && normalizedStep > 0.50 ? 1.45 : 1.0;
            var operatingLoad = scenario.OperatingLoadMultiplier * mixedMultiplier * (1.0 + 0.10 * Math.Sin(step / 21.0 + unitIndex));
            var rockAbrasivity = scenario.RockAbrasivityMultiplier * (0.95 + 0.10 * rng.NextDouble());
            var shockEvent = rng.NextDouble() < scenario.ShockProbability;
            var chipIncrement = shockEvent ? scenario.ShockIncrement * (0.65 + rng.NextDouble() * 0.70) : 0.0;
            damage = Clamp(damage * 0.998 + chipIncrement, 0.0, 1.0);
            var wearVelocity = baseVelocity * operatingLoad * rockAbrasivity + chipIncrement * 0.18 + rng.Normal(0.0, 0.00035);
            wear = Clamp(wear + Math.Max(0.00005, wearVelocity), 0.0, 1.05);
            var replacementEvent = scenario.PlannedResetStep.HasValue && step == scenario.PlannedResetStep.Value;
            if (replacementEvent)
            {
                resetNextStep = true;
            }

            var signalQuality = Clamp(1.0 - scenario.SignalDegradationRate * normalizedStep - scenario.MissingnessRate * 0.35, 0.15, 1.0);
            var trueRul = CalculateRul(config.ReplacementThreshold, wear, Math.Max(0.0001, wearVelocity));
            var healthLabel = replacementEvent ? "replaced" : ResolveHealthLabel(wear, damage, config.DangerousWearThreshold);
            var dangerousWear = wear >= config.DangerousWearThreshold || damage >= 0.85;
            var falseAlarmInjected = !dangerousWear && rng.NextDouble() < scenario.FalseAlarmProbability;
            var sensorDrop = rng.NextDouble() < scenario.MissingnessRate;

            Increment(stateCounts, (scenario.ScenarioId, healthLabel));
            latentState.Write(RunId, configHash, config.MasterSeed, scenario.ScenarioId, unitId, step, wear, wearVelocity, damage, healthLabel, trueRul, operatingLoad, rockAbrasivity, signalQuality, replacementEvent, "all");
            events.Write(RunId, configHash, config.MasterSeed, scenario.ScenarioId, unitId, step, shockEvent, shockEvent, replacementEvent, falseAlarmInjected, sensorDrop, dangerousWear, ResolveEventLabel(shockEvent, replacementEvent, falseAlarmInjected, sensorDrop, dangerousWear), "all");

            var observedProxy = Clamp(wear + rng.Normal(0.0, 0.035 + (1.0 - signalQuality) * 0.08), 0.0, 1.10);
            foreach (var channel in config.SensorChannels)
            {
                var missing = sensorDrop || rng.NextDouble() < scenario.MissingnessRate;
                var quality = missing ? 0.0 : Clamp(signalQuality - rng.NextDouble() * 0.10, 0.0, 1.0);
                var featureValue = missing ? double.NaN : channel.BaseLevel + channel.WearSensitivity * wear + channel.DamageSensitivity * damage + channel.LoadSensitivity * operatingLoad + rng.Normal(0.0, channel.NoiseScale + (1.0 - quality) * 0.04);
                var qualityFlag = missing ? "missing" : quality < 0.50 ? "low_quality" : "usable";

                observations.Write(RunId, configHash, config.MasterSeed, scenario.ScenarioId, unitId, step, channel.ChannelId, channel.FeatureName, double.IsNaN(featureValue) ? null : featureValue, quality, qualityFlag, missing, "all");
                paper01Trace.Write(RunId, configHash, scenario.ScenarioId, unitId, step, channel.ChannelId, quality, missing, "data/synthetic/generated/observations.csv", "paper_01");

                var obsKey = (scenario.ScenarioId, channel.ChannelId);
                if (!observationStats.TryGetValue(obsKey, out var observationAccumulator))
                {
                    observationAccumulator = new ObservationAccumulator();
                    observationStats[obsKey] = observationAccumulator;
                }

                observationAccumulator.Add(quality, missing);

                if (!double.IsNaN(featureValue))
                {
                    if (!featureTrendStats.TryGetValue(obsKey, out var trendAccumulator))
                    {
                        trendAccumulator = new TrendAccumulator();
                        featureTrendStats[obsKey] = trendAccumulator;
                    }

                    trendAccumulator.Add(step < config.StepsPerUnit / 2, featureValue);
                }
            }

            var estimatorOutputs = estimatorState.Update(observedProxy, wearVelocity, signalQuality);
            foreach (var output in estimatorOutputs)
            {
                estimates.Write(RunId, configHash, config.MasterSeed, scenario.ScenarioId, unitId, step, output.EstimatorId, observedProxy, output.EstimatedWear, output.Lower, output.Upper, output.Uncertainty, output.EstimatedWear - wear, "paper_02");
                paper02Latent.Write(RunId, configHash, scenario.ScenarioId, unitId, step, wear, output.EstimatedWear, output.EstimatorId, output.EstimatedWear - wear, "paper_02");
                paper02Intervals.Write(RunId, configHash, scenario.ScenarioId, unitId, step, output.EstimatorId, output.Lower, output.Upper, output.Lower <= wear && wear <= output.Upper, "paper_02");

                var predictedRul = CalculateRul(config.ReplacementThreshold, output.EstimatedWear, output.EstimatedVelocity);
                var lowerRul = Math.Max(0, predictedRul - output.Uncertainty * 250.0);
                var upperRul = predictedRul + output.Uncertainty * 250.0;
                var signedRulError = predictedRul - trueRul;
                rulPredictions.Write(RunId, configHash, config.MasterSeed, scenario.ScenarioId, unitId, step, output.EstimatorId, trueRul, predictedRul, signedRulError, Math.Abs(signedRulError), lowerRul, upperRul, "paper_02");
                paper02Rul.Write(RunId, configHash, scenario.ScenarioId, unitId, step, trueRul, predictedRul, output.EstimatorId, "paper_02");

                var estimatorKey = (scenario.ScenarioId, output.EstimatorId);
                if (!estimatorStats.TryGetValue(estimatorKey, out var estimatorAccumulator))
                {
                    estimatorAccumulator = new EstimatorAccumulator();
                    estimatorStats[estimatorKey] = estimatorAccumulator;
                }

                estimatorAccumulator.Add(wear, output.EstimatedWear, trueRul, predictedRul, output.Lower, output.Upper, dangerousWear, output.EstimatedWear >= config.Metrics.FalseAlarmWearThreshold);
            }

            var ewma = estimatorOutputs.First(e => e.EstimatorId == "ewma");
            var predictedRulForPolicy = CalculateRul(config.ReplacementThreshold, ewma.EstimatedWear, ewma.EstimatedVelocity);
            foreach (var policy in config.Policies)
            {
                var decision = ResolvePolicyDecision(policy, step, observedProxy, ewma.EstimatedWear, ewma.Upper, ewma.Uncertainty, predictedRulForPolicy, dangerousWear);
                var traceId = $"{scenario.ScenarioId}:{unitId}:{step}:{policy.PolicyId}";
                decisions.Write(RunId, configHash, config.MasterSeed, scenario.ScenarioId, unitId, step, policy.PolicyId, decision.Action, decision.RiskScore, decision.ReasonCode, decision.FollowUpAction, traceId, "paper_03");
                paper03Timeline.Write(RunId, configHash, scenario.ScenarioId, unitId, step, policy.PolicyId, decision.Action, decision.RiskScore, decision.ReasonCode, "paper_03");

                var policyKey = (scenario.ScenarioId, policy.PolicyId);
                if (!policyStats.TryGetValue(policyKey, out var policyAccumulator))
                {
                    policyAccumulator = new PolicyAccumulator();
                    policyStats[policyKey] = policyAccumulator;
                }

                policyAccumulator.Add(decision, wear, dangerousWear, config.Metrics);
            }
        }
    }

    private static void WritePaper01Outputs(SyntheticExperimentConfig config, string configHash, Dictionary<(string Scenario, string State), long> stateCounts, Dictionary<(string Scenario, string Channel), ObservationAccumulator> observationStats, Dictionary<(string Scenario, string Channel), TrendAccumulator> featureTrendStats, CsvSink stateSink, CsvSink featureSink, CsvSink kpiSink, CsvSink metricsSummary)
    {
        foreach (var entry in stateCounts.OrderBy(x => x.Key.Scenario).ThenBy(x => x.Key.State))
        {
            stateSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.State, entry.Value, "paper_01", "synthetic dataset; model-based state count");
        }

        foreach (var scenario in config.Scenarios)
        {
            var scenarioObservationStats = observationStats.Where(x => x.Key.Scenario == scenario.ScenarioId).Select(x => x.Value).ToList();
            var meanQuality = scenarioObservationStats.Count == 0 ? 0.0 : scenarioObservationStats.Average(x => x.MeanQuality);
            var traceabilityCompleteness = scenarioObservationStats.Count == 0 ? 0.0 : scenarioObservationStats.Average(x => 1.0 - x.MissingRate);
            var stateCount = stateCounts.Where(x => x.Key.Scenario == scenario.ScenarioId).Sum(x => x.Value);
            kpiSink.Write(RunId, configHash, scenario.ScenarioId, stateCount, meanQuality, traceabilityCompleteness, "synthetic dataset; computational experiment", "paper_01");
            metricsSummary.Write(RunId, configHash, "paper_01", scenario.ScenarioId, "kpi", "mean_data_quality", meanQuality, "synthetic dataset");
            metricsSummary.Write(RunId, configHash, "paper_01", scenario.ScenarioId, "kpi", "traceability_completeness", traceabilityCompleteness, "synthetic dataset");

            foreach (var channel in config.SensorChannels)
            {
                var key = (scenario.ScenarioId, channel.ChannelId);
                observationStats.TryGetValue(key, out var obs);
                featureTrendStats.TryGetValue(key, out var trend);
                var meanFirst = trend?.MeanFirstHalf ?? 0.0;
                var meanSecond = trend?.MeanSecondHalf ?? 0.0;
                featureSink.Write(RunId, configHash, scenario.ScenarioId, channel.ChannelId, channel.FeatureName, meanFirst, meanSecond, meanSecond - meanFirst, obs?.MissingRate ?? 1.0, "paper_01", "synthetic dataset; feature trend summary");
            }
        }
    }

    private static void WritePaper02Outputs(string configHash, Dictionary<(string Scenario, string Estimator), EstimatorAccumulator> estimatorStats, Dictionary<(string Scenario, string Channel), ObservationAccumulator> observationStats, CsvSink metricsSink, CsvSink ablationSink, CsvSink metricsSummary)
    {
        foreach (var entry in estimatorStats.OrderBy(x => x.Key.Scenario).ThenBy(x => x.Key.Estimator))
        {
            var stats = entry.Value;
            metricsSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.Estimator, stats.Rmse, stats.Mae, stats.MeanAbsoluteRulError, stats.MeanSignedRulError, stats.FalseAlarmRate, stats.MissedDangerousWearRate, stats.MeanDetectionLeadTime, stats.PredictionIntervalCoverage, stats.MeanPredictionIntervalWidth, "paper_02");
            metricsSummary.Write(RunId, configHash, "paper_02", entry.Key.Scenario, entry.Key.Estimator, "rmse", stats.Rmse, "model-based evaluation");
            metricsSummary.Write(RunId, configHash, "paper_02", entry.Key.Scenario, entry.Key.Estimator, "rul_absolute_error", stats.MeanAbsoluteRulError, "model-based evaluation");
        }

        foreach (var entry in observationStats.OrderBy(x => x.Key.Scenario).ThenBy(x => x.Key.Channel))
        {
            var stats = entry.Value;
            ablationSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.Channel, stats.MeanQuality, stats.MissingRate, 1.0 + stats.MissingRate * 0.75 + (1.0 - stats.MeanQuality) * 0.40, "paper_02");
        }
    }

    private static void WritePaper03Outputs(string configHash, Dictionary<(string Scenario, string Policy), PolicyAccumulator> policyStats, CsvSink ledgerSink, CsvSink comparisonSink, CsvSink riskSink, CsvSink frontierSink, CsvSink metricsSummary)
    {
        foreach (var entry in policyStats.OrderBy(x => x.Key.Scenario).ThenBy(x => x.Key.Policy))
        {
            var stats = entry.Value;
            ledgerSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.Policy, stats.EarlyReplacementCount, stats.LateReplacementCount, stats.MissedDangerousWearCount, stats.FalseAlarmCount, stats.DangerousTimeExposure, "paper_03");
            comparisonSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.Policy, stats.MeanUtilizationAtReplacement, stats.ExpectedCost, stats.ExpectedRisk, stats.AlarmPrecision, stats.AlarmRecall, "paper_03");
            riskSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.Policy, "all_steps", stats.ExpectedRisk, stats.ActionRate, "paper_03");
            frontierSink.Write(RunId, configHash, entry.Key.Scenario, entry.Key.Policy, stats.ExpectedCost, stats.ExpectedRisk, stats.MeanUtilizationAtReplacement, "paper_03");
            metricsSummary.Write(RunId, configHash, "paper_03", entry.Key.Scenario, entry.Key.Policy, "expected_cost", stats.ExpectedCost, "model-based evaluation");
            metricsSummary.Write(RunId, configHash, "paper_03", entry.Key.Scenario, entry.Key.Policy, "expected_risk", stats.ExpectedRisk, "model-based evaluation");
        }
    }

    private static void WriteFigureTraceability(SyntheticExperimentConfig config, string configHash, CsvSink trace, CsvSink manifest)
    {
        var rows = new (string VisualId, string Paper, string Source, string Kind, string Allowed, string Forbidden)[]
        {
            ("P01-F01", "paper_01", "data/synthetic/generated/paper01/state_scenarios.csv", "generated synthetic table", "Architecture and state-layer examples are supported by synthetic stand outputs.", "Do not claim industrial validation."),
            ("P01-F02", "paper_01", "data/synthetic/generated/paper01/feature_matrix.csv", "generated synthetic table", "Feature trends are model-based examples under stated assumptions.", "Do not claim measured mine performance."),
            ("P01-F03", "paper_01", "data/synthetic/generated/observations.csv", "generated synthetic table", "Sensor-event schema and quality flags are traceable.", "Do not claim real mining data."),
            ("P01-F04", "paper_01", "data/synthetic/generated/paper01/kpi_summary.csv", "generated synthetic table", "KPI summaries are synthetic demonstrators.", "Do not claim economic effect."),
            ("P01-F05", "paper_01", "data/synthetic/generated/paper01/data_quality_traceability.csv", "generated synthetic table", "Missingness and quality handling are traceable.", "Do not claim production deployment."),
            ("P02-F02", "paper_02", "data/synthetic/generated/paper02/latent_vs_estimated_wear.csv", "generated synthetic plot source", "Estimator errors may be compared inside this synthetic stand.", "Do not claim universal superiority."),
            ("P02-F03", "paper_02", "data/synthetic/generated/paper02/rul_prediction_trace.csv", "generated synthetic plot source", "RUL traces are model-based outputs.", "Do not claim field-validated RUL."),
            ("P02-F04", "paper_02", "data/synthetic/generated/paper02/prediction_intervals.csv", "generated synthetic plot source", "Prediction interval coverage is computed for synthetic trajectories.", "Do not claim calibrated field uncertainty."),
            ("P02-F06", "paper_02", "data/synthetic/generated/paper02/filter_metrics_by_scenario.csv", "generated synthetic table", "Baseline comparison is limited to generated scenarios.", "Do not claim practical superiority in real mining conditions."),
            ("P02-F07", "paper_02", "data/synthetic/generated/paper02/sensor_ablation_metrics.csv", "generated synthetic table", "Sensor ablation is a synthetic sensitivity view.", "Do not claim validated sensor ranking."),
            ("P03-F02", "paper_03", "data/synthetic/generated/paper03/scenario_timeline_actions.csv", "generated synthetic plot source", "Policy actions are traceable by scenario and step.", "Do not claim production deployment."),
            ("P03-F04", "paper_03", "data/synthetic/generated/paper03/policy_frontier.csv", "generated synthetic plot source", "Cost-risk frontier is hypothetical and model-based.", "Do not claim enterprise cost reduction."),
            ("P03-F05", "paper_03", "data/synthetic/generated/paper03/event_ledger_summary.csv", "generated synthetic table", "False-alarm and missed-wear counts are synthetic metrics.", "Do not claim industrial safety evidence."),
            ("P03-F06", "paper_03", "data/synthetic/generated/paper03/policy_comparison_metrics.csv", "generated synthetic table", "Policy comparison is bounded by synthetic assumptions.", "Do not claim real maintenance effectiveness.")
        };

        foreach (var row in rows)
        {
            trace.Write(row.VisualId, row.Paper, row.Source, "data/synthetic/configs/experiment_config.json", "WearVision.SyntheticStand.Cli", row.Kind, row.Allowed, row.Forbidden, configHash, config.MasterSeed);
            manifest.Write(row.VisualId, row.Source, row.Kind, row.Paper, "data/synthetic/configs/experiment_config.json", configHash, config.MasterSeed, "synthetic dataset; computational experiment; model-based evaluation");
        }
    }

    private static PolicyDecision ResolvePolicyDecision(PolicyConfig policy, int step, double observedWear, double estimatedWear, double upperEstimate, double uncertainty, double predictedRul, bool dangerousWear)
    {
        var horizonRisk = Clamp((policy.HorizonSteps - predictedRul) / Math.Max(1.0, policy.HorizonSteps), 0.0, 1.0);
        var risk = Clamp(horizonRisk * 0.55 + uncertainty * 1.20 + Math.Max(0.0, estimatedWear - 0.70) * 0.90, 0.0, 1.0);

        if (dangerousWear && (estimatedWear >= 0.90 || upperEstimate >= 0.95))
        {
            return new PolicyDecision("emergency replacement", risk, "dangerous_wear_or_upper_bound", "stop and replace");
        }

        return policy.PolicyId switch
        {
            "periodic_replacement" when step > 0 && step % policy.ReplacementIntervalSteps == 0 => new PolicyDecision("planned replacement", risk, "periodic_interval", "reset after replacement"),
            "observed_threshold" when observedWear >= policy.WearThreshold => new PolicyDecision("planned replacement", risk, "observed_threshold_crossing", "inspect removed cutter"),
            "latent_state_threshold" when estimatedWear >= policy.WearThreshold => new PolicyDecision("planned replacement", risk, "estimated_wear_threshold", "schedule replacement"),
            "uncertainty_aware_threshold" when upperEstimate >= policy.WearThreshold || uncertainty >= policy.UncertaintyThreshold => new PolicyDecision(risk >= policy.RiskThreshold ? "planned replacement" : "inspection", risk, "uncertainty_upper_bound", "increase observation density"),
            "risk_aware_horizon" when risk >= policy.RiskThreshold => new PolicyDecision(predictedRul <= policy.HorizonSteps / 2.0 ? "planned replacement" : "inspection", risk, "risk_horizon_threshold", "review risk at next event"),
            _ when risk >= 0.45 => new PolicyDecision("intensified monitoring", risk, "moderate_risk", "shorten monitoring interval"),
            _ => new PolicyDecision("continue operation", risk, "below_action_threshold", "continue scheduled monitoring")
        };
    }

    private static int CalculateRul(double threshold, double wear, double velocity)
    {
        if (wear >= threshold)
        {
            return 0;
        }

        return (int)Math.Ceiling((threshold - wear) / Math.Max(0.00005, velocity));
    }

    private static string ResolveHealthLabel(double wear, double damage, double threshold)
    {
        if (wear >= threshold || damage >= 0.85)
        {
            return "critical";
        }

        if (wear >= 0.68 || damage >= 0.55)
        {
            return "warning";
        }

        return "healthy";
    }

    private static string ResolveEventLabel(bool shock, bool replacement, bool falseAlarm, bool sensorDrop, bool dangerous)
    {
        if (replacement)
        {
            return "replacement_reset";
        }

        if (dangerous)
        {
            return "dangerous_wear";
        }

        if (shock)
        {
            return "chipping_or_shock";
        }

        if (falseAlarm)
        {
            return "false_alarm";
        }

        if (sensorDrop)
        {
            return "sensor_drop";
        }

        return "nominal_event_tick";
    }

    private static void Increment<TKey>(Dictionary<TKey, long> dictionary, TKey key)
        where TKey : notnull
    {
        dictionary[key] = dictionary.TryGetValue(key, out var value) ? value + 1 : 1;
    }

    private static double Clamp(double value, double min, double max) => Math.Min(max, Math.Max(min, value));

    private sealed class DeterministicRandom
    {
        private readonly Random _random;

        public DeterministicRandom(int seed)
        {
            _random = new Random(seed);
        }

        public double NextDouble() => _random.NextDouble();

        public double Normal(double mean, double standardDeviation)
        {
            var u1 = Math.Max(1e-12, _random.NextDouble());
            var u2 = Math.Max(1e-12, _random.NextDouble());
            var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return mean + standardDeviation * z0;
        }
    }

    private sealed class UnitEstimatorState
    {
        private readonly Queue<double> _window = new();
        private double? _previousObserved;
        private double _ewma;
        private double _linearEstimate;
        private double _linearVelocity = 0.0018;

        public IReadOnlyList<EstimatorOutput> Update(double observedWear, double trueVelocity, double quality)
        {
            if (_window.Count == 0)
            {
                _ewma = observedWear;
                _linearEstimate = observedWear;
            }

            _window.Enqueue(observedWear);
            while (_window.Count > 5)
            {
                _window.Dequeue();
            }

            var persistence = _previousObserved ?? observedWear;
            var movingAverage = _window.Average();
            _ewma = 0.35 * observedWear + 0.65 * _ewma;
            var observedSlope = _previousObserved.HasValue ? observedWear - _previousObserved.Value : trueVelocity;
            _linearVelocity = Clamp(0.25 * observedSlope + 0.75 * _linearVelocity, 0.00005, 0.020);
            _linearEstimate = Clamp(_linearEstimate + _linearVelocity, 0.0, 1.10);
            _previousObserved = observedWear;
            var uncertaintyBase = 0.035 + (1.0 - quality) * 0.12;

            return
            [
                new EstimatorOutput("persistence_baseline", persistence, Math.Max(0.00005, trueVelocity * 0.80), uncertaintyBase + 0.040),
                new EstimatorOutput("moving_average", movingAverage, Math.Max(0.00005, trueVelocity * 0.92), uncertaintyBase + 0.030),
                new EstimatorOutput("ewma", _ewma, Math.Max(0.00005, trueVelocity), uncertaintyBase + 0.020),
                new EstimatorOutput("linear_trend", _linearEstimate, _linearVelocity, uncertaintyBase + 0.050)
            ];
        }
    }

    private sealed record EstimatorOutput(string EstimatorId, double EstimatedWear, double EstimatedVelocity, double Uncertainty)
    {
        public double Lower => Math.Max(0.0, EstimatedWear - 1.64 * Uncertainty);
        public double Upper => Math.Min(1.20, EstimatedWear + 1.64 * Uncertainty);
    }

    private sealed record PolicyDecision(string Action, double RiskScore, string ReasonCode, string FollowUpAction);

    private sealed class ObservationAccumulator
    {
        private long _count;
        private long _missing;
        private double _qualitySum;

        public double MissingRate => _count == 0 ? 0.0 : _missing / (double)_count;
        public double MeanQuality => _count == 0 ? 0.0 : _qualitySum / _count;

        public void Add(double quality, bool missing)
        {
            _count++;
            if (missing)
            {
                _missing++;
            }

            _qualitySum += quality;
        }
    }

    private sealed class TrendAccumulator
    {
        private long _firstCount;
        private long _secondCount;
        private double _firstSum;
        private double _secondSum;

        public double MeanFirstHalf => _firstCount == 0 ? 0.0 : _firstSum / _firstCount;
        public double MeanSecondHalf => _secondCount == 0 ? 0.0 : _secondSum / _secondCount;

        public void Add(bool firstHalf, double value)
        {
            if (firstHalf)
            {
                _firstCount++;
                _firstSum += value;
            }
            else
            {
                _secondCount++;
                _secondSum += value;
            }
        }
    }

    private sealed class EstimatorAccumulator
    {
        private long _count;
        private double _squareErrorSum;
        private double _absoluteErrorSum;
        private double _rulAbsoluteErrorSum;
        private double _rulSignedErrorSum;
        private long _falseAlarms;
        private long _dangerousRows;
        private long _missedDangerousRows;
        private long _intervalCovered;
        private double _intervalWidthSum;
        private double _leadTimeSum;
        private long _leadTimeCount;

        public double Rmse => _count == 0 ? 0.0 : Math.Sqrt(_squareErrorSum / _count);
        public double Mae => _count == 0 ? 0.0 : _absoluteErrorSum / _count;
        public double MeanAbsoluteRulError => _count == 0 ? 0.0 : _rulAbsoluteErrorSum / _count;
        public double MeanSignedRulError => _count == 0 ? 0.0 : _rulSignedErrorSum / _count;
        public double FalseAlarmRate => _count == 0 ? 0.0 : _falseAlarms / (double)_count;
        public double MissedDangerousWearRate => _dangerousRows == 0 ? 0.0 : _missedDangerousRows / (double)_dangerousRows;
        public double PredictionIntervalCoverage => _count == 0 ? 0.0 : _intervalCovered / (double)_count;
        public double MeanPredictionIntervalWidth => _count == 0 ? 0.0 : _intervalWidthSum / _count;
        public double MeanDetectionLeadTime => _leadTimeCount == 0 ? 0.0 : _leadTimeSum / _leadTimeCount;

        public void Add(double trueWear, double estimatedWear, int trueRul, int predictedRul, double lower, double upper, bool dangerousWear, bool alarm)
        {
            var error = estimatedWear - trueWear;
            _count++;
            _squareErrorSum += error * error;
            _absoluteErrorSum += Math.Abs(error);
            _rulAbsoluteErrorSum += Math.Abs(predictedRul - trueRul);
            _rulSignedErrorSum += predictedRul - trueRul;
            _intervalWidthSum += upper - lower;
            if (lower <= trueWear && trueWear <= upper)
            {
                _intervalCovered++;
            }

            if (dangerousWear)
            {
                _dangerousRows++;
                if (!alarm)
                {
                    _missedDangerousRows++;
                }
            }
            else if (alarm)
            {
                _falseAlarms++;
                _leadTimeSum += trueRul;
                _leadTimeCount++;
            }
        }
    }

    private sealed class PolicyAccumulator
    {
        private long _count;
        private long _actions;
        private long _trueAlarms;
        private long _allAlarms;
        private long _dangerousRows;
        private double _riskSum;
        private double _costSum;
        private double _replacementWearSum;
        private long _replacementCount;

        public long EarlyReplacementCount { get; private set; }
        public long LateReplacementCount { get; private set; }
        public long MissedDangerousWearCount { get; private set; }
        public long FalseAlarmCount { get; private set; }
        public long DangerousTimeExposure { get; private set; }
        public double ExpectedCost => _count == 0 ? 0.0 : _costSum / _count;
        public double ExpectedRisk => _count == 0 ? 0.0 : _riskSum / _count;
        public double MeanUtilizationAtReplacement => _replacementCount == 0 ? 0.0 : _replacementWearSum / _replacementCount;
        public double AlarmPrecision => _allAlarms == 0 ? 0.0 : _trueAlarms / (double)_allAlarms;
        public double AlarmRecall => _dangerousRows == 0 ? 0.0 : _trueAlarms / (double)_dangerousRows;
        public double ActionRate => _count == 0 ? 0.0 : _actions / (double)_count;

        public void Add(PolicyDecision decision, double trueWear, bool dangerousWear, MetricsConfig metrics)
        {
            _count++;
            _riskSum += decision.RiskScore;
            var replacement = decision.Action is "planned replacement" or "emergency replacement";
            var alarm = decision.Action is "inspection" or "planned replacement" or "emergency replacement";

            if (decision.Action != "continue operation")
            {
                _actions++;
            }

            if (dangerousWear)
            {
                _dangerousRows++;
                DangerousTimeExposure++;
                _costSum += metrics.DangerousExposureCost;
                if (!alarm)
                {
                    MissedDangerousWearCount++;
                }
                else
                {
                    _trueAlarms++;
                }
            }
            else if (alarm)
            {
                FalseAlarmCount++;
            }

            if (alarm)
            {
                _allAlarms++;
            }

            if (replacement)
            {
                _replacementCount++;
                _replacementWearSum += trueWear;
                if (trueWear < 0.70)
                {
                    EarlyReplacementCount++;
                }

                if (trueWear >= metrics.DangerousWearThreshold)
                {
                    LateReplacementCount++;
                }

                _costSum += decision.Action == "emergency replacement" ? metrics.EmergencyReplacementCost : metrics.PlannedReplacementCost;
            }
            else if (decision.Action == "inspection")
            {
                _costSum += metrics.InspectionCost;
            }
        }
    }
}
