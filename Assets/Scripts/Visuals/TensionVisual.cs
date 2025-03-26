using Lokpik.Common;
using SaintsField;
using UnityEngine;

namespace Lokpik.Visuals
{
    public class TensionVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform innerCircle;

        [Header("Config")]
        [SerializeField] float decay;

        [Header("Debug")]
        [SerializeField, ReadOnly, Range(0, 1)] float progress;

        public float Progress { set => progress = value; }

        private void Update()
        {
            float scale = innerCircle.localScale.y.ExpDecay(progress, decay, Time.deltaTime);
            innerCircle.localScale = new Vector3(scale, scale, 1);
        }
    }
}
