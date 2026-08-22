using UnityEngine;
using Vertx.Attributes;

namespace Lokpik.Visuals
{
    using RO = ReadOnlyFieldAttribute;

    public class PlugVisual : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Range(0, 360)] private float maxAngle = 90f;

        [Header("Debug")]
        [SerializeField, RO, Range(0, 1)] private float progress;

        private void Update()
        {
            float zAngle = Mathf.Lerp(0, maxAngle, progress);
            transform.localRotation = Quaternion.Euler(0, 0, -zAngle);
        }

        internal void SetProgress(float value) =>
            progress = value;
    }
}
