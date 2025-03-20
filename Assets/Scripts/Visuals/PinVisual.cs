using Lokpik.Locks;
using SaintsField;
using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class PinVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform keyPinRenderer;
        [SerializeField] Transform driverPinRenderer;
        [SerializeField] Transform backgroundRenderer;

        [Header("Config")]
        [SerializeField, Min(0)] float chamberWidth;
        [SerializeField, Min(0)] float pinWidth;

        [Header("Info")]
        [SerializeReference, ReadOnly] TumblerLock tumblerLock;

        public float ChamberWidth => chamberWidth;

        private float KeyPinLength => tumblerLock.Config.KeyPinLengths[pinIndex];
        private float DriverPinLength => tumblerLock.Config.DriverPinLengths[pinIndex];
        private static float ChamberHeight => TumblerLockConfig.ChamberHeight;

        private int pinIndex;

        private void Awake()
        {
            backgroundRenderer.localScale = new Vector3(chamberWidth, ChamberHeight, 1);
            keyPinRenderer.localScale = new Vector3(pinWidth, KeyPinLength, 1);
            driverPinRenderer.localScale = new Vector3(pinWidth, DriverPinLength, 1);
        }

        private void Update()
        {
            if (tumblerLock == null)
                return;

            Chamber chamber = tumblerLock.Chamber(pinIndex);
            keyPinRenderer.localPosition = new Vector3(0, chamber.KeyPinLift, 0);
            driverPinRenderer.localPosition = new Vector3(0, chamber.DriverPinLift, 0);
        }

        internal void SetLock(TumblerLock value, int pin)
        {
            tumblerLock = value;
            pinIndex = pin;
        }
    }
}
