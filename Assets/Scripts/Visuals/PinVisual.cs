using Lokpik.TumblerLock;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;

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
        // [SerializeField, ReadOnly, Range(0, 1)] float liftAmount;
        [SerializeField, ReadOnly] bool isBinding;

        [Header("Info")]
        [ShowInInspector] private float KeyPinLength => tumblerLock.Config.KeyPinLengths[pinIndex];
        [ShowInInspector] private float DriverPinLength => tumblerLock.Config.DriverPinLengths[pinIndex];
        [ShowInInspector] private float ChamberHeight => TumblerLockConfig.ChamberHeight;

        [FormerlySerializedAs("lock")]
        [FormerlySerializedAs("lockState")]
        [SerializeReference] TumblerLock.TumblerLock tumblerLock;
        private int pinIndex;

        public float ChamberWidth => chamberWidth;
        public float PinWidth => pinWidth;

        internal void SetLockState(TumblerLock.TumblerLock state, int pin)
        {
            tumblerLock = state;
            pinIndex = pin;
        }

        private void Update()
        {
            if (tumblerLock == null)
                return;

            backgroundRenderer.color = backgroundColor;
            keyPinRenderer.color = keyPinColor;
            driverPinRenderer.color = driverPinColor;

            backgroundRenderer.transform.localScale = new Vector3(chamberWidth, ChamberHeight, 1);
            keyPinRenderer.transform.localScale = new Vector3(pinWidth, KeyPinLength, 1);
            driverPinRenderer.transform.localScale = new Vector3(pinWidth, DriverPinLength, 1);

            Chamber chamber = tumblerLock.Chambers[pinIndex];
            float keyPinY = chamber.KeyPinLift;
            float driverPinY = chamber.DriverPinLift + KeyPinLength;

            keyPinRenderer.transform.localPosition = new Vector3(0, keyPinY, 0);

            // if (!isBinding)
                driverPinRenderer.transform.localPosition = new Vector3(0, driverPinY, 0);
        }
    }
}
