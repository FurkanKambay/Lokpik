using UnityEngine;

namespace Lokpik.Visuals
{
    public class PlugVisual : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Range(0, 360)] float maxAngle = 90f;

        [Header("Debug")]
        [SerializeField, Range(0, 1)] float progress;

        public float Progress { set => progress = value; }

        private void Update()
        {
            float zAngle = Mathf.Lerp(0, maxAngle, progress);
            transform.localRotation = Quaternion.Euler(0, 0, -zAngle);
        }
    }
}
