using System.Linq;
using Lokpik.Locks;
using UnityEditor;
using UnityEngine;

namespace Lokpik.Visuals
{
    public class LockVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Lockpicker lockpicker;
        [SerializeField] TensionVisual tensionVisual;
        [SerializeField] PlugVisual plugVisual;
        [SerializeField] PinVisual[] pinVisuals;

        private TumblerLockConfig Config => lockpicker.Config;
        private TumblerLock Lock => lockpicker.State;

        private void OnEnable()
        {
            for (int pin = 0; pin < pinVisuals.Length; pin++)
                pinVisuals[pin].SetLock(lockpicker.State, pin);
        }

        private void Update()
        {
            tensionVisual.Progress = lockpicker.AppliedTorque;
            plugVisual.Progress = lockpicker.State.PlugRotation;
        }

        private void OnDrawGizmos()
        {
            // Required plug rotation per pin
            Vector3 center = plugVisual.transform.position;
            Vector3 forward = plugVisual.transform.forward;

            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                float radius = Config.GetAdequateRotation(pin) / 2f;
                Handles.color = pin == Lock.PickingPin ? Color.green : Color.red;
                Handles.DrawWireDisc(center, forward, radius);
            }

            // Shear line
            float shearLine = lockpicker.Config.ShearLine * TumblerLockConfig.ChamberHeight;

            PinVisual firstPin = pinVisuals.First();
            PinVisual lastPin = pinVisuals.Last();
            Vector3 leftPoint = firstPin.transform.TransformPoint(-firstPin.ChamberWidth, shearLine, 0);
            Vector3 rightPoint = lastPin.transform.TransformPoint(firstPin.ChamberWidth, shearLine, 0);

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(leftPoint, rightPoint, 2f);

            // Torque markers
            float minTorqueX = lockpicker.MinTorque;
            float maxTorqueX = lockpicker.MaxTorque;

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

                if (pin == Lock.BindingPin)
                    (color, text) = (Color.red, "Binding");
                else if (pin == Lock.PickingPin)
                    (color, text) = (Color.blue, Lock.PickingChamber.State.ToString());

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
