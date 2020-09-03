using WeightedRandomization;

namespace CinematicCameraSystem {
    /// <summary>
    /// A serializable CinematicShotType with a corresponding weight for weighted randomization.
    /// </summary>
    [System.Serializable]
    public class WeightedCinematicShotType : WeightedParameter<CinematicShotType> {
        public WeightedCinematicShotType(CinematicShotType parameter, float weight) : base(parameter, weight) { }
    }
}
