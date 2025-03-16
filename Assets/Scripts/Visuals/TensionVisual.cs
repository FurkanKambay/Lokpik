using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class TensionVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer innerBar;
        [SerializeField] SpriteRenderer outerBar;

        [Header("Config")]
        [SerializeField, Range(0, 1)] float progress;
        [SerializeField] Color innerColor;
        [SerializeField] Color outerColor;

        public float Progress
        {
            get => progress;
            set => progress = value;
        }

        private void Update()
        {
            innerBar.transform.localScale = new Vector3(1, progress, 1);
            innerBar.transform.localPosition = Vector3.up * ((progress / 2f) - 0.5f);

            innerBar.color = innerColor;
            outerBar.color = outerColor;
        }
    }
}
