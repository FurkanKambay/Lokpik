using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lokpik.Visuals
{
    public class LockVisual : MonoBehaviour
    {
        [SerializeField] Lock tumblerLock;
        [SerializeField] TensionVisual tensionVisual;
        [SerializeField] PlugVisual plugVisual;
        [SerializeField] PinVisual[] pinVisuals;

        private void Update()
        {
            tensionVisual.Progress = tumblerLock.AppliedTension;
            plugVisual.Progress = tumblerLock.Progress;

            for (int i = 0; i < pinVisuals.Length; i++)
            {
                pinVisuals[i].Progress = tumblerLock.PinRiseAmounts[i];
                // pinVisuals[i].KeyPinHeight = 22;
            }
        }

        private void OnDrawGizmos()
        {
            // Vector3 center = plugVisual.transform.position;
            // Vector3 forward = plugVisual.transform.forward;
            // float plugScale = plugVisual.transform.localScale.x / 2f;

            // Handles.color = Color.red;
            //
            // foreach (float bindPoint in tumblerLock.Config.KeyPinLengths)
            //     Handles.DrawWireDisc(center, forward, bindPoint * plugScale);

            // ^ old code for drawing circles for the Bind Points

            // Shear line
            Handles.color = Color.cyan;

            PinVisual firstPin = pinVisuals.First();
            PinVisual lastPin = pinVisuals.Last();

            float heightScale = firstPin.HoleHeight;
            float shear = (tumblerLock.Config.ShearLine * heightScale) - (heightScale / 2f);

            Vector3 p1 = firstPin.transform.TransformPoint(-firstPin.HoleWidth, shear, 0);
            Vector3 p2 = lastPin.transform.TransformPoint(firstPin.HoleWidth, shear, 0);

            Handles.DrawDottedLine(p1, p2, 1f);
        }
    }
}
