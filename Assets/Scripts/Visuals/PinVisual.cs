using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class PinVisual : MonoBehaviour
    {
        [SerializeField] Transform progressBar;
        [SerializeField, Range(0, 1)] float progress;

        public float Progress
        {
            get => progress;
            set => progress = value;
        }

        private void Update()
        {
            progressBar.localScale = new Vector3(1, progress, 1);
            progressBar.localPosition = Vector3.up * ((progress / 2f) - 0.5f);
        }
    }
}
