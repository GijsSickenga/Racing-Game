using UnityEngine;
using System.Linq;

namespace WeightedRandomization {
    public static class WeightedRandom {
        /// <summary>
        /// Returns a parameter based on weighted randomization.
        /// Chance is relative to the combined weight of all passed in parameters.
        /// </summary>
        public static T Get<T>(params WeightedParameter<T>[] weightedParameters) {
            float? randomChance = weightedParameters?.Sum(x => x?.Weight) * Random.Range(0f, 1f);
            if (randomChance == null) { return default(T); }

            foreach (WeightedParameter<T> weightedParameter in weightedParameters) {
                randomChance -= weightedParameter.Weight;
                if (randomChance > 0) { continue; }
                return weightedParameter.Parameter;
            }

            // Only reachable if no parameters are passed in.
            return default(T);
        }
    }
}
