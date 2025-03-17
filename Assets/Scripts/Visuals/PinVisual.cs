using Lokpik.Locks;
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

        [Header("Config")]
        [SerializeField, Min(0)] float chamberWidth;
        [SerializeField, Min(0)] float pinWidth;

        [Header("Info")]
        [SerializeReference] TumblerLock tumblerLock;

        public float ChamberWidth => chamberWidth;

        private float KeyPinLength => tumblerLock.Config.KeyPinLengths[pinIndex];
        private float DriverPinLength => tumblerLock.Config.DriverPinLengths[pinIndex];
        private static float ChamberHeight => TumblerLockConfig.ChamberHeight;

        private int pinIndex;

        private void Update()
        {
            if (tumblerLock == null)
                return;

            backgroundRenderer.transform.localScale = new Vector3(chamberWidth, ChamberHeight, 1);
            keyPinRenderer.transform.localScale = new Vector3(pinWidth, KeyPinLength, 1);
            driverPinRenderer.transform.localScale = new Vector3(pinWidth, DriverPinLength, 1);

            Chamber chamber = tumblerLock.Chambers[pinIndex];
            float driverPinY = chamber.DriverPinLift + KeyPinLength;

            keyPinRenderer.transform.localPosition = new Vector3(0, chamber.KeyPinLift, 0);
            driverPinRenderer.transform.localPosition = new Vector3(0, driverPinY, 0);
        }

        internal void SetLock(TumblerLock value, int pin)
        {
            tumblerLock = value;
            pinIndex = pin;
        }
    }
}
