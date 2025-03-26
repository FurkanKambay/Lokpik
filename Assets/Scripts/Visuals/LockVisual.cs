using System.Linq;
using Lokpik.Locks;
using SaintsField;
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

        [ArraySize(nameof(PinCount))]
        [SerializeField] PinVisual[] pinVisuals;

        private TumblerLock Lock => lockpicker.Lock;
        private TumblerLockConfig LockConfig => Lock.Config;

#if UNITY_EDITOR
        private int PinCount => Lock.PinCount;
#endif

        private void Awake()
        {
            for (int pin = 0; pin < pinVisuals.Length; pin++)
                pinVisuals[pin].SetPin(pin);
        }

        private void Update()
        {
            tensionVisual.Progress = lockpicker.AppliedTorque;
            plugVisual.Progress = Lock.PlugRotation;
        }

        internal Chamber GetChamber(int pin) => Lock.Chamber(pin);

        private void OnDrawGizmos()
        {
            // Shear line
            float shearLine = LockConfig.ShearLine * TumblerLockConfig.ChamberHeight;

            PinVisual firstPin = pinVisuals.First();
            PinVisual lastPin = pinVisuals.Last();
            Vector3 leftPoint = firstPin.transform.TransformPoint(-firstPin.ChamberWidth, shearLine, 0);
            Vector3 rightPoint = lastPin.transform.TransformPoint(firstPin.ChamberWidth, shearLine, 0);

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(leftPoint, rightPoint, 2f);

            // Torque markers
            Vector3 tensionCenter = tensionVisual.transform.position;
            float tensionScale = tensionVisual.transform.localScale.x * 0.5f;

            Handles.color = Color.white;
            Handles.DrawWireDisc(tensionCenter, Vector3.forward, lockpicker.MinTorque * tensionScale);
            Handles.DrawWireDisc(tensionCenter, Vector3.forward, lockpicker.MaxTorque * tensionScale);

            // Pin labels
            if (lockpicker.Lock == null)
                return;

            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                ChamberState pinState = Lock.Chamber(pin).State;
                string text = pinState.ToString();

                Color color = pinState switch
                {
                    _ when pin == lockpicker.PickingPin => Color.blue,
                    _ when pinState.IsBinding() => Color.red,
                    _ when pinState.IsPicked() => Color.green,
                    _ => Color.white,
                };

                var style = new GUIStyle
                {
                    alignment = TextAnchor.UpperCenter,
                    fontSize = 16,
                    normal = { textColor = color }
                };

                PinVisual pinVisual = pinVisuals[pin];
                Handles.Label(pinVisual.transform.position, text, style);
            }
        }
    }
}
