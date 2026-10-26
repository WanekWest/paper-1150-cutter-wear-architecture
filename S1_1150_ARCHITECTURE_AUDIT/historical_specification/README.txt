Retained unpublished R3 specification. Source SHA-256: 355bfc0988c8338cf74f8db981788c40a4fc122a343b4881b974cd3a671cc5ae

The companion XML preserves its three native OMML equations. In the earlier specification the diagnostic score was D_t = sigmoid(alpha^T f_t + beta^T e_t - gamma(1-Q_t) - omega_U U_t). This is the object of the conditional sign audit, not an implemented estimator claimed by the revised paper. The revised manuscript supersedes the earlier performance interpretation.

Selected original explanatory paragraphs:
Here, σ is the sigmoid mapping; α and β are feature and event weights; γ is the penalty for low data quality; Qₜ is the aggregate quality index; ωᵁ is the uncertainty-penalty weight; and Uₜ is propagated diagnostic uncertainty. The sign is essential: a higher Qₜ means better observation quality, so the penalty must use 1 − Qₜ.

The resulting relationship supports the internal consistency of the penalty within the synthetic protocol, but it must not be interpreted as physical calibration of wear rate.

The sensitivity plot converts the checks in Tables 3 and 4 into a visual sign test by showing the joint movement of the diagnostic indicator and uncertainty under controlled data degradation. Figure 3 shows the channel-ablation results.

The main result is that Dₜ, Qₜ, and Uₜ must be reported together. A diagnostic indicator without quality and uncertainty can be misleading when it is derived from partially missing or low-quality channels. The proposed model changes the interpretation of a monitoring result: the indicator is a function of features and events, but also of observation reliability and propagated uncertainty.

The synthetic character of the results limits the scope of the conclusions. Tables 3 and 4 support the internal consistency of the 1 − Qₜ penalty and the need to interpret Uₜ jointly with Dₜ, but they do not estimate a physical wear rate. Table 5 records indicator dependence on the channels under this feature mapping without establishing a permanent sensor hierarchy, whereas Table 6 links claim reproducibility to data, figures, and the supporting artifact chain.

The work is limited to model-based synthetic evidence. Observation equations, channel weights, covariance terms, quality aggregation, and the first-order uncertainty approximation require metrological review before external use. The synthetic computational scenario does not replace calibration against controlled laboratory or field measurements.

The proposed model defines the monitored object, latent wear or geometric condition, event information, operating regime, channel observations, feature transformations, and quality descriptors. Distinguishing qₛ,ₜ, qₜ, and Qₜ is essential: qₛ,ₜ describes channel usability, qₜ may be used as a quality state in subsequent models, and Qₜ aggregates observation quality for the diagnostic indicator. The indicator Dₜ uses the penalty 1 − Qₜ and uncertainty Uₜ, while Jₜ makes the uncertainty approximation explicitly inspectable.