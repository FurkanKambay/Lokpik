using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class TensionVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer innerBar;
        [SerializeField] SpriteRenderer outerBar;

        [Header("Debug")]
        [SerializeField, Range(0, 1)] float progress;

        public float Progress { set => progress = value; }

        private void Update() =>
            innerBar.transform.localScale = new Vector3(1, progress, 1);
    }
}
