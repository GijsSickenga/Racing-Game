using UnityEngine;

// NOTE: To display a WeightedParameter in the inspector, create a subclass of it that defines the generic type.
//       Mark the subclass serializable so it can be displayed in the inspector.
//       Don't forget to also add a subclass of WeightedParameterDrawer for the new class in WeightedParameterDrawer.cs.

namespace WeightedRandomization {
    /// <summary>
    /// Defines a weighted parameter used by WeightedRandom.Get().
    /// </summary>
    /// <typeparam name="T">The parameter returned by WeightedRandom.Get() upon selection.</typeparam>
    [System.Serializable]
    public class WeightedParameter<T> {
        public WeightedParameter(T parameter, float weight) {
            this.weight = Mathf.Max(0, weight);
            this.parameter = parameter;
        }

        [Range(0, Mathf.Infinity), SerializeField] private float weight;
        public float Weight { get { return weight; } }
        public const string WEIGHT_VARIABLE_NAME = nameof(weight); // Used by WeightedParameterDrawer.cs.

        [SerializeField] private T parameter;
        public T Parameter { get { return parameter; } }
        public const string PARAMETER_VARIABLE_NAME = nameof(parameter); // Used by WeightedParameterDrawer.cs.
    }
}
