namespace WearVision.SyntheticStand.Core;

public static class DefaultSyntheticConfigFactory
{
    public static SyntheticExperimentConfig CreateDefault(
        int masterSeed = 20260616,
        int unitsPerScenario = 30,
        int stepsPerUnit = 300)
    {
        var scenarios = new[]
        {
            new ScenarioConfig("normal_wear", "Normal wear", 1.00, 0.003, 0.050, 0.010, 0.020, 0.020, 1.00, 1.00, null, false),
            new ScenarioConfig("accelerated_wear", "Accelerated wear", 1.65, 0.006, 0.060, 0.015, 0.025, 0.030, 1.20, 1.35, null, false),
            new ScenarioConfig("chipping_shock_event", "Chipping / shock event", 1.15, 0.035, 0.120, 0.015, 0.025, 0.030, 1.10, 1.15, null, false),
            new ScenarioConfig("false_alarm", "False alarm", 0.95, 0.004, 0.045, 0.090, 0.020, 0.025, 1.00, 1.00, null, false),
            new ScenarioConfig("signal_degradation", "Signal degradation", 1.05, 0.005, 0.050, 0.025, 0.080, 0.350, 1.05, 1.10, null, false),
            new ScenarioConfig("mixed_regime", "Mixed regime", 1.25, 0.012, 0.090, 0.030, 0.040, 0.080, 1.15, 1.25, null, true),
            new ScenarioConfig("missing_data", "Missing data", 1.05, 0.006, 0.050, 0.020, 0.180, 0.080, 1.00, 1.05, null, false),
            new ScenarioConfig("replacement_reset", "Replacement / reset", 1.35, 0.010, 0.070, 0.020, 0.030, 0.050, 1.10, 1.15, 190, false)
        };

        var sensors = new[]
        {
            new SensorChannelConfig("vibration", "vibration_rms", 0.35, 1.30, 0.70, 0.25, 0.060),
            new SensorChannelConfig("acoustic_emission", "ae_rms", 0.28, 1.10, 1.10, 0.20, 0.070),
            new SensorChannelConfig("current_power", "power_proxy", 0.45, 0.85, 0.35, 0.40, 0.050),
            new SensorChannelConfig("force_torque", "force_torque_proxy", 0.42, 0.95, 0.45, 0.55, 0.055),
            new SensorChannelConfig("temperature", "temperature_proxy", 0.30, 0.55, 0.25, 0.60, 0.045),
            new SensorChannelConfig("image_geometric_wear", "geometric_wear_proxy", 0.05, 1.00, 0.30, 0.05, 0.035)
        };

        var policies = new[]
        {
            new PolicyConfig("periodic_replacement", "periodic replacement", 240, 0.82, 0.12, 0.70, 80),
            new PolicyConfig("observed_threshold", "observed-threshold policy", 240, 0.78, 0.12, 0.70, 80),
            new PolicyConfig("latent_state_threshold", "latent-state threshold policy", 240, 0.80, 0.10, 0.68, 90),
            new PolicyConfig("uncertainty_aware_threshold", "uncertainty-aware threshold policy", 240, 0.78, 0.16, 0.62, 100),
            new PolicyConfig("risk_aware_horizon", "risk-aware horizon policy", 240, 0.76, 0.14, 0.58, 120)
        };

        var metrics = new MetricsConfig(20260617, 0.90, 0.65, 0.85, 5.0, 18.0, 1.0, 0.08);

        return new SyntheticExperimentConfig(
            "wave02-synthetic-stand-v1",
            masterSeed,
            unitsPerScenario,
            stepsPerUnit,
            0.85,
            0.85,
            scenarios,
            sensors,
            policies,
            metrics);
    }
}
