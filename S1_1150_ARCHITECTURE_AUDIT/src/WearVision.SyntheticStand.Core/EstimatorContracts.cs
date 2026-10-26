namespace WearVision.SyntheticStand.Core;

public enum EstimatorKind
{
    PersistenceBaseline,
    MovingAverage,
    Ewma,
    LinearTrend,
    KalmanFilter,
    ExtendedKalmanFilter,
    UnscentedKalmanFilter,
    ParticleFilter
}

public sealed record WearEstimatorInput(
    string RunId,
    string ConfigHash,
    int Seed,
    string ScenarioId,
    string UnitId,
    int Step,
    double TrueWear,
    int TrueRul,
    bool DangerousWear,
    double? ObservedWearProxy);

public sealed record WearEstimatorResult(
    EstimatorKind Kind,
    double WearEstimate,
    double WearLower,
    double WearUpper,
    int RulEstimate,
    int RulLower,
    int RulUpper,
    double AlarmScore,
    double DangerProbability,
    double EffectiveSampleSize,
    string EstimationStatus);

public sealed record EstimatorConfiguration(
    int MovingAverageWindow,
    double EwmaAlpha,
    double KalmanProcessNoise,
    double KalmanObservationNoise,
    double EkfProcessNoise,
    double EkfObservationNoise,
    double UkfAlpha,
    double UkfBeta,
    double UkfKappa,
    int ParticleCount,
    double ParticleProcessNoise,
    double ParticleObservationNoise,
    double ParticleResamplingThreshold,
    double AlarmThreshold,
    double RulThreshold,
    IReadOnlyList<EstimatorKind> Estimators);

public interface IWearEstimator
{
    EstimatorKind Kind { get; }

    WearEstimatorResult Update(WearEstimatorInput input);
}
