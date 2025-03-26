using Lokpik.Common;
using Lokpik.Locks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Lokpik.Visuals
{
    public class PinVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform keyPin;
        [SerializeField] Transform driverPin;
        [SerializeField] Transform background;

        [Header("Config")]
        [SerializeField, Min(0)] float chamberWidth;
        [SerializeField, Min(0)] float pinWidth;
        [SerializeField, Min(0)] float decay;

        internal float ChamberWidth => chamberWidth;

        private LockVisual lockVisual;
        private Chamber chamber;
        private int pinIndex;

        private void Awake()
        {
            lockVisual = GetComponentInParent<LockVisual>();
            Assert.IsNotNull(lockVisual, $"{nameof(PinVisual)} doesn't have a {nameof(LockVisual)} as its parent.");
        }

        private void Start()
        {
            keyPin.localPosition = Vector3.up * chamber.KeyPinLift;
            driverPin.localPosition = Vector3.up * chamber.DriverPinLift;
        }

        private void Update()
        {
            if (chamber?.Lock == null)
                return;

            UpdateScales();

            float keyPinLift = keyPin.localPosition.y.ExpDecay(chamber.KeyPinLift, decay, Time.deltaTime);
            float driverPinLift = driverPin.localPosition.y.ExpDecay(chamber.DriverPinLift, decay, Time.deltaTime);

            keyPin.localPosition = Vector3.up * keyPinLift;
            driverPin.localPosition = Vector3.up * driverPinLift;
        }

        internal void SetPin(int pin) =>
            chamber = lockVisual.GetChamber(pin);

        private void UpdateScales()
        {
            if (chamber?.Lock == null)
                return;

            background.localScale = new Vector3(chamberWidth, TumblerLockConfig.ChamberHeight, 1);
            keyPin.localScale = new Vector3(pinWidth, chamber.KeyPinLength, 1);
            driverPin.localScale = new Vector3(pinWidth, chamber.DriverPinLength, 1);
        }

        private void OnValidate() => UpdateScales();
    }
}
