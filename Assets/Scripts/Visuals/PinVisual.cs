using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class PinVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer keyPinRenderer;
        [SerializeField] SpriteRenderer driverPinRenderer;
        [SerializeField] SpriteRenderer backgroundRenderer;

        [Header("Colors")]
        [SerializeField] Color keyPinColor;
        [SerializeField] Color driverPinColor;
        [SerializeField] Color backgroundColor;

        [Header("Config")]
        [SerializeField, Min(0)] float chamberWidth;
        [SerializeField, Min(0)] float pinWidth;

        [Header("Debug")]
        [SerializeField, ReadOnly, Range(0, 1)] float progress;
        [SerializeField, ReadOnly] bool isBinding;

        [Header("Info")]
        [ShowInInspector] private float KeyPinLength => lockState.Config.KeyPinLengths[pinIndex];
        [ShowInInspector] private float DriverPinLength => lockState.Config.DriverPinLengths[pinIndex];
        [ShowInInspector] private float ChamberHeight => TumblerLockConfig.ChamberHeight;
        [SerializeReference] TumblerLockState lockState;
        private int pinIndex;

        public float ChamberWidth => chamberWidth;
        public float PinWidth => pinWidth;

        public float Progress
        {
            get => progress;
            set => progress = value;
        }

        internal void SetLockState(TumblerLockState state, int pin)
        {
            lockState = state;
            pinIndex = pin;
        }

        private void Update()
        {
            if (lockState == null)
                return;

            keyPinRenderer.color = keyPinColor;
            driverPinRenderer.color = driverPinColor;
            backgroundRenderer.color = backgroundColor;

            backgroundRenderer.transform.localScale = new Vector3(chamberWidth, ChamberHeight, 1);
            keyPinRenderer.transform.localScale = new Vector3(pinWidth, KeyPinLength, 1);
            driverPinRenderer.transform.localScale = new Vector3(pinWidth, DriverPinLength, 1);

            float keyPinY = (KeyPinLength / 2f) - (ChamberHeight / 2f) + (progress / 2f);
            float driverPinY = keyPinY + (KeyPinLength / 2f) + (DriverPinLength / 2f);

            if (isBinding)
            {
                float maxY = driverPinY - (DriverPinLength / 2f) - (KeyPinLength / 2f);
                keyPinY = Mathf.Min(keyPinY, maxY);
            }

            keyPinRenderer.transform.localPosition = new Vector3(0, keyPinY, 0);

            if (!isBinding)
            {
                driverPinRenderer.transform.localPosition = new Vector3(0, driverPinY, 0);
            }
        }
    }
}
