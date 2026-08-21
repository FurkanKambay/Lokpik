using UnityEngine;

namespace Lokpik.Visuals
{
    public class TensionVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform innerCircle;

        [Header("Debug")]
        [SerializeField, Range(0, 1)] private float fillValue;

        private void Update()
        {
            innerCircle.localScale = new Vector3(fillValue, fillValue, 1);
        }

        internal void SetFillValue(float value) =>
            fillValue = value;
    }
}
