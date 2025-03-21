using Lokpik.Locks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Lokpik.Visuals
{
    public class PinVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform keyPinRenderer;
        [SerializeField] Transform driverPinRenderer;
        [SerializeField] Transform backgroundRenderer;

        [Header("Config")]
        [SerializeField, Min(0)] float chamberWidth;
        [SerializeField, Min(0)] float pinWidth;

        internal float ChamberWidth => chamberWidth;

        private LockVisual lockVisual;
        private Chamber chamber;
        private int pinIndex;

        private void Awake()
        {
            lockVisual = GetComponentInParent<LockVisual>();
            Assert.IsNotNull(lockVisual, $"{nameof(PinVisual)} doesn't have a {nameof(LockVisual)} as its parent.");
        }

        private void Update()
        {
            if (chamber?.Lock == null)
                return;

            UpdateScales();

            keyPinRenderer.localPosition = new Vector3(0, chamber.KeyPinLift, 0);
            driverPinRenderer.localPosition = new Vector3(0, chamber.DriverPinLift, 0);
        }

        internal void SetLock(LockVisual value, int pin)
        {
            lockVisual = value;
            pinIndex = pin;
            chamber = lockVisual.GetChamber(pinIndex);
        }

        private void UpdateScales()
        {
            if (chamber?.Lock == null)
                return;

            backgroundRenderer.localScale = new Vector3(chamberWidth, TumblerLockConfig.ChamberHeight, 1);
            keyPinRenderer.localScale = new Vector3(pinWidth, chamber.KeyPinLength, 1);
            driverPinRenderer.localScale = new Vector3(pinWidth, chamber.DriverPinLength, 1);
        }

        private void OnValidate() => UpdateScales();
    }
}
