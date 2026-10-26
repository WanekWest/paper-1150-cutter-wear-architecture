namespace WearVision.SyntheticStand.Core;

public sealed record SyntheticExperimentConfig(
    string GeneratorVersion,
    int MasterSeed,
    int UnitsPerScenario,
    int StepsPerUnit,
    double ReplacementThreshold,
    double DangerousWearThreshold,
    IReadOnlyList<ScenarioConfig> Scenarios,
    IReadOnlyList<SensorChannelConfig> SensorChannels,
    IReadOnlyList<PolicyConfig> Policies,
    MetricsConfig Metrics);

public sealed record ScenarioConfig(
    string ScenarioId,
    string DisplayName,
    double WearRateMultiplier,
    double ShockProbability,
    double ShockIncrement,
    double FalseAlarmProbability,
    double MissingnessRate,
    double SignalDegradationRate,
    double OperatingLoadMultiplier,
    double RockAbrasivityMultiplier,
    int? PlannedResetStep,
    bool MixedRegime);

public sealed record SensorChannelConfig(
    string ChannelId,
    string FeatureName,
    double BaseLevel,
    double WearSensitivity,
    double DamageSensitivity,
    double LoadSensitivity,
    double NoiseScale);

public sealed record PolicyConfig(
    string PolicyId,
    string PolicyFamily,
    int ReplacementIntervalSteps,
    double WearThreshold,
    double UncertaintyThreshold,
    double RiskThreshold,
    int HorizonSteps);

public sealed record MetricsConfig(
    int BootstrapSeed,
    double PredictionIntervalLevel,
    double FalseAlarmWearThreshold,
    double DangerousWearThreshold,
    double PlannedReplacementCost,
    double EmergencyReplacementCost,
    double InspectionCost,
    double DangerousExposureCost);

public sealed record SyntheticGenerationSummary(
    string RunId,
    string ConfigHash,
    int Seed,
    int ScenarioCount,
    int UnitCount,
    int StepCount,
    int SensorChannelCount,
    int PolicyCount,
    string ArtifactLabel);

public static class SyntheticStandCatalog
{
    public static readonly string[] RequiredScenarioIds =
    [
        "normal_wear",
        "accelerated_wear",
        "chipping_shock_event",
        "false_alarm",
        "signal_degradation",
        "mixed_regime",
        "missing_data",
        "replacement_reset"
    ];

    public static readonly string[] RequiredSensorChannels =
    [
        "vibration",
        "acoustic_emission",
        "current_power",
        "force_torque",
        "temperature",
        "image_geometric_wear"
    ];

    public static readonly string[] AllowedActions =
    [
        "continue operation",
        "intensified monitoring",
        "inspection",
        "planned replacement",
        "emergency replacement"
    ];
}
