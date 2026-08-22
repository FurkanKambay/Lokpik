using UnityEngine;
using Vertx.Attributes;

namespace Lokpik.Visuals
{
    using RO = ReadOnlyFieldAttribute;

    public class TensionVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform innerCircle;

        [Header("Debug")]
        [SerializeField, RO, Range(0, 1)] private float fillValue;

        private void Update()
        {
            innerCircle.localScale = new Vector3(fillValue, fillValue, 1);
        }

        internal void SetFillValue(float value) =>
            fillValue = value;
    }
}
