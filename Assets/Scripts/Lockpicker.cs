using Lokpik.Data;
using Lokpik.Locks;
using UnityEngine;

namespace Lokpik
{
    public class Lockpicker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TumblerLock tumblerLock;
        [SerializeField] private PickControlsConfig controls;

        [Header("Debug")]
        [SerializeField, Range(0, 5)] private int targetedPin;
        [SerializeField, Range(0, 1)] private float appliedTorque;
        [SerializeField, Min(0)] private float chamberRetargetTimer;

        public TumblerLock Lock => tumblerLock;
        public int TargetedPin => targetedPin;
        public float MinTorque => controls ? controls.MinTorque : -1f;
        public float MaxTorque => controls ? controls.MaxTorque : -1f;

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        private IPickInput input;

        private void Awake()
        {
            for (int i = 0; i < tumblerLock.Config.PinCount; i++)
                tumblerLock.Chamber(i).SetLock(tumblerLock, i);

            tumblerLock.StopPicking();
        }

        internal void Init(IPickInput input) => this.input = input;

        private void Update()
        {
            if (input is not Object)
                return;

            HandleChangePin();
            ApplyTorque();

            tumblerLock.LiftPinTowards(targetedPin, input.PickHeight);
        }

        private void HandleChangePin()
        {
            chamberRetargetTimer += Time.deltaTime;
            if (chamberRetargetTimer < controls.ChamberRetargetRate)
                return;

            int delta = input.PickMoveDelta;
            if (delta == 0) return;

            chamberRetargetTimer = 0;
            tumblerLock.StopLifting(targetedPin);
            targetedPin = tumblerLock.Config.ClampPinIndex(targetedPin + delta);
        }

        /// <summary>
        /// Apply torque and rotate the plug accordingly.
        /// </summary>
        private void ApplyTorque()
        {
            float tension = input.Tension;
            if (controls.UseTensionDrift && tension > 0)
            {
                float randomDrift = Random.Range(-controls.TensionDrift, controls.TensionDrift);
                tension += randomDrift;
            }

            AppliedTorque = tension;

            // TODO: move this all into TumblerLock?
            bool lowTorque = appliedTorque < controls.MinTorque; // not enough to Set any pin
            bool highTorque = appliedTorque > controls.MaxTorque; // too much for the pin to move

            float turnDelta = lowTorque ? -controls.PlugGravity : controls.TurnSpeed;

            int tensionValue = lowTorque ? -1 : highTorque ? 1 : 0;
            tumblerLock.RotatePlug(turnDelta * Time.deltaTime, tensionValue);
        }
    }
}
