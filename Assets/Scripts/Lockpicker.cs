using Lokpik.Data;
using Lokpik.Locks;
using UnityEngine;

namespace Lokpik
{
    public class Lockpicker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TumblerLock tumblerLock;

        [Header("Input Config")]
        [SerializeField, Min(0)] private float chamberRetargetRate = 0.3f;

        [Header("Artificial Skill Hindrance")]
        [SerializeField] private bool useTensionDrift;
        [SerializeField, Min(0)] private float tensionDrift = 0.008f;

        [Header("Pin Setting")]
        [Tooltip("Minimum torque required to turn plug.")]
        [SerializeField, Range(0, 1)] private float minTorque = 0.5f;
        [Tooltip("Maximum torque the plug can handle before binding the pin.")]
        [SerializeField, Range(0, 1)] private float maxTorque = 0.8f;

        [Header("Plug Rotation")]
        [SerializeField, Min(0)] private float turnSpeed = 1f;
        [SerializeField, Min(0)] private float plugGravity = 5f;

        [Header("Debug")]
        [SerializeField, Range(0, 5)] private int targetedPin;
        [SerializeField, Range(0, 1)] private float appliedTorque;
        [SerializeField, Min(0)] private float chamberRetargetTimer;

        public TumblerLock Lock => tumblerLock;
        public int TargetedPin => targetedPin;
        public float MinTorque => minTorque;
        public float MaxTorque => maxTorque;

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        private IPickInput input;

        private void Awake()
        {
            for (int i = 0; i < tumblerLock.PinCount; i++)
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
            if (chamberRetargetTimer < chamberRetargetRate)
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
            if (useTensionDrift && tension > 0)
            {
                float randomDrift = Random.Range(-tensionDrift, tensionDrift);
                tension += randomDrift;
            }

            AppliedTorque = tension;

            // TODO: move this all into TumblerLock?
            bool lowTorque = appliedTorque < minTorque; // not enough to Set any pin
            bool highTorque = appliedTorque > maxTorque; // too much for the pin to move
            int tensionValue = lowTorque ? -1 : highTorque ? 1 : 0;

            float turnDelta =
                highTorque ? turnSpeed
                : lowTorque ? -plugGravity
                : turnSpeed;

            tumblerLock.RotatePlug(turnDelta * Time.deltaTime, tensionValue);
        }
    }
}
