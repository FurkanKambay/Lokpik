using System;
using System.Linq;
using FK.Lokpik.Locks;
using UnityEditor;
using UnityEngine;

namespace FK.Lokpik.Visuals
{
    public class LockVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Lockpicker lockpicker;
        [SerializeField] TensionVisual tensionVisual;
        [SerializeField] PlugVisual plugVisual;

        [SerializeField] PinVisual[] pinVisuals;

        private TumblerLock Lock => lockpicker.Lock;
        private TumblerLockConfig LockConfig => Lock.Config;

        private void Awake()
        {
            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                Chamber chamber = GetChamber(pin);
                pinVisuals[pin].Init(chamber);
            }
        }

        private void Update()
        {
            tensionVisual.SetFillValue(lockpicker.AppliedTorque);
            plugVisual.SetProgress(Lock.PlugTurnProgress);
        }

        internal Chamber GetChamber(int pin) => Lock.Chamber(pin);

#if UNITY_EDITOR
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

            // Handles.color = Color.white;

            // Pin labels
            if (lockpicker.Lock == null)
                return;

            for (int pin = 0; pin < pinVisuals.Length; pin++)
            {
                ChamberState pinState = Lock.Chamber(pin).State;
                string text = pinState.ToString();

                Color color = pinState switch
                {
                    _ when pin == lockpicker.TargetedPin => Color.blue,
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

        private void OnValidate()
        {
            Array.Resize(ref pinVisuals, Lock.Config.PinCount);
        }
#endif
    }
}
