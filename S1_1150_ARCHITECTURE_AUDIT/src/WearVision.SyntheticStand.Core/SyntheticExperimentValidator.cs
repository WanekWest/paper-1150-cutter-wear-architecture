namespace WearVision.SyntheticStand.Core;

public static class SyntheticExperimentValidator
{
    public static IReadOnlyList<string> Validate(SyntheticExperimentConfig config)
    {
        var errors = new List<string>();

        if (config.MasterSeed <= 0)
        {
            errors.Add("MasterSeed must be positive.");
        }

        if (config.UnitsPerScenario < 1)
        {
            errors.Add("UnitsPerScenario must be positive.");
        }

        if (config.StepsPerUnit < 1)
        {
            errors.Add("StepsPerUnit must be positive.");
        }

        var scenarioIds = config.Scenarios.Select(s => s.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in SyntheticStandCatalog.RequiredScenarioIds)
        {
            if (!scenarioIds.Contains(required))
            {
                errors.Add($"Required scenario is missing: {required}.");
            }
        }

        var channels = config.SensorChannels.Select(s => s.ChannelId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in SyntheticStandCatalog.RequiredSensorChannels)
        {
            if (!channels.Contains(required))
            {
                errors.Add($"Required sensor channel is missing: {required}.");
            }
        }

        foreach (var scenario in config.Scenarios)
        {
            if (scenario.ShockProbability is < 0 or > 1)
            {
                errors.Add($"Scenario {scenario.ScenarioId} has invalid shock probability.");
            }

            if (scenario.MissingnessRate is < 0 or > 1)
            {
                errors.Add($"Scenario {scenario.ScenarioId} has invalid missingness rate.");
            }
        }

        foreach (var policy in config.Policies)
        {
            if (policy.HorizonSteps <= 0)
            {
                errors.Add($"Policy {policy.PolicyId} has invalid horizon.");
            }
        }

        return errors;
    }
}
