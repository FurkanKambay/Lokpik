using UnityEngine;

namespace Lokpik.Common
{
    public static class VectorMathExtensions
    {
        /// Exponential decay function from Freya Holmér.
        public static float ExpDecay(this float from, float to, float decay, float deltaTime) =>
            to + ((from - to) * Mathf.Exp(-decay * deltaTime));
    }
}
