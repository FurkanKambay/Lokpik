using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class PlugVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform progressCircle;

        [Header("Debug")]
        [SerializeField, Range(0, 1)] float progress;

        public float Progress { set => progress = value; }

        private void Update() => progressCircle.localScale = Vector3.one * progress;
    }
}
