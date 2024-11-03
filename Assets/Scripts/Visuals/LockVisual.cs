using UnityEngine;

namespace Lokpik.Visuals
{
    public class LockVisual : MonoBehaviour
    {
        [SerializeField] Lock @lock;
        [SerializeField] PlugVisual plugVisual;
        [SerializeField] PinVisual[] pinVisuals;

        private void Update()
        {
            plugVisual.Progress = @lock.CurrentProgress;

            for (int i = 0; i < pinVisuals.Length; i++)
            {
                pinVisuals[i].Progress = @lock.PinRiseAmounts[i];
            }
        }
    }
}
