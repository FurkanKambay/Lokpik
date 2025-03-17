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

        private TumblerLockConfig Config => tumblerLock.Config;
        private TumblerLockState State => tumblerLock.State;

        private void OnEnable()
        {
            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                PinVisual pinVisual = pinVisuals[pin];
                pinVisual.SetLockState(tumblerLock.State, pin);
            }
        }

        private void Update()
        {
            tensionVisual.Progress = tumblerLock.AppliedTension;
            plugVisual.Progress = tumblerLock.Progress;

            for (int i = 0; i < pinVisuals.Length; i++)
            {
                // pinVisuals[i].Progress = tumblerLock.PinRiseAmounts[i];
                // pinVisuals[i].KeyPinLength = Config.KeyPinLengths[i];
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
            // replace with maybe an actual turning plug?

            // Shear line
            float shearLine = tumblerLock.Config.ShearLine * TumblerLockConfig.ChamberHeight;

            PinVisual firstPin = pinVisuals.First();
            PinVisual lastPin = pinVisuals.Last();
            Vector3 leftPoint = firstPin.transform.TransformPoint(-firstPin.ChamberWidth, shearLine, 0);
            Vector3 rightPoint = lastPin.transform.TransformPoint(firstPin.ChamberWidth, shearLine, 0);

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(leftPoint, rightPoint, 1f);

            // Pin labels
            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                PinVisual pinVisual = pinVisuals[pin];
                (string text, Color color) = ("Pin", Color.white);

                if (pin == State.BindingPin)
                    (color, text) = (Color.red, "Binding");
                else if (pin == State.PickingPin)
                    (color, text) = (Color.blue, "Picking");

                var style = new GUIStyle
                {
                    alignment = TextAnchor.UpperCenter,
                    fontSize = 16,
                    normal = { textColor = color }
                };

                Handles.Label(pinVisual.transform.position, text, style);
            }
        }
    }
}
