using System.Linq;
using Lokpik.TumblerLock;
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
            tensionVisual.Progress = tumblerLock.AppliedTorque;
            plugVisual.Progress = tumblerLock.State.PlugRotation;
        }

        private void OnDrawGizmos()
        {
            // Plug rotation per pin
            Vector3 center = plugVisual.transform.position;
            Vector3 forward = plugVisual.transform.forward;

            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                float radius = Config.GetAdequateRotation(pin) / 2f;
                Handles.color = pin == State.PickingPin ? Color.green : Color.red;
                Handles.DrawWireDisc(center, forward, radius);
            }

            // Shear line
            float shearLine = tumblerLock.Config.ShearLine * TumblerLockConfig.ChamberHeight;

            PinVisual firstPin = pinVisuals.First();
            PinVisual lastPin = pinVisuals.Last();
            Vector3 leftPoint = firstPin.transform.TransformPoint(-firstPin.ChamberWidth, shearLine, 0);
            Vector3 rightPoint = lastPin.transform.TransformPoint(firstPin.ChamberWidth, shearLine, 0);

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(leftPoint, rightPoint, 2f);

            // Torque markers
            float minTorqueX = tumblerLock.MinTorque;
            float maxTorqueX = tumblerLock.MaxTorque;

            Vector3 leftTop = tensionVisual.transform.TransformPoint(1, minTorqueX, 0);
            Vector3 leftBottom = tensionVisual.transform.TransformPoint(-1, minTorqueX, 0);
            Vector3 rightTop = tensionVisual.transform.TransformPoint(1, maxTorqueX, 0);
            Vector3 rightBottom = tensionVisual.transform.TransformPoint(-1, maxTorqueX, 0);

            Handles.color = Color.green;
            Handles.DrawLine(leftTop, leftBottom, 1f);
            Handles.color = Color.red;
            Handles.DrawLine(rightTop, rightBottom, 1f);

            // Pin labels
            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                PinVisual pinVisual = pinVisuals[pin];
                (string text, Color color) = ("Pin", Color.white);

                if (pin == State.BindingPin)
                    (color, text) = (Color.red, "Binding");
                else if (pin == State.PickingPin)
                    (color, text) = (Color.blue, State.PinState.ToString());

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
