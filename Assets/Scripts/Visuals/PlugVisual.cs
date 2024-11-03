using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class PlugVisual : MonoBehaviour
    {
        [SerializeField] Transform progressCircle;
        [SerializeField, Range(0, 1)] float progress;

        public float Progress
        {
            get => progress;
            set => progress = value;
        }

        private void Update() => progressCircle.localScale = Vector3.one * progress;
    }
}
