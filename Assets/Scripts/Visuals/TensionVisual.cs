using Lokpik.Common;
using SaintsField;
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
        [SerializeField] float decay;

        [Header("Debug")]
        [SerializeField, ReadOnly, Range(0, 1)] float progress;

        public float Progress { set => progress = value; }

        private void Update()
        {
            float yScale = innerBar.transform.localScale.y.ExpDecay(progress, decay, Time.deltaTime);
            innerBar.transform.localScale = new Vector3(1, yScale, 1);
        }
    }
}
