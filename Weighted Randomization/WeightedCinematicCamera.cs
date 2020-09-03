using WeightedRandomization;

namespace CinematicCameraSystem {
    /// <summary>
    /// A serializable CinematicCamera with a corresponding weight for weighted randomization.
    /// </summary>
    [System.Serializable]
    public class WeightedCinematicCamera : WeightedParameter<CinematicCamera> {
        public WeightedCinematicCamera(CinematicCamera parameter, float weight) : base(parameter, weight) { }
    }
}
