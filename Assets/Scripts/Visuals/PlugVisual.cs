using UnityEngine;

namespace Lokpik.Visuals
{
    public class PlugVisual : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Range(0, 360)] private float maxAngle = 90f;

        [Header("Debug")]
        [SerializeField, Range(0, 1)] private float fillValue;

        private void Update()
        {
            float zAngle = Mathf.Lerp(0, maxAngle, fillValue);
            transform.localRotation = Quaternion.Euler(0, 0, -zAngle);
        }

        internal void SetFillValue(float value) =>
            fillValue = value;
    }
}
