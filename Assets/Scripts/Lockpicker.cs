using Lokpik.Data;
using Lokpik.Locks;
using UnityEngine;

namespace Lokpik
{
    public class Lockpicker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] TumblerLock tumblerLock;

        [Header("Input Config")]
        [SerializeField, Min(0)] private float chamberRetargetRate = 0.3f;

        [Header("Artificial Skill Hindrance")]
        [SerializeField] bool useTensionDrift;
        [SerializeField, Min(0)] private float tensionKeepDrift = 0.5f;

        [Header("Pin Setting")]
        [Tooltip("Minimum torque required to turn plug.")]
        [SerializeField, Range(0, 1)] float minTorque = 0.5f;
        [Tooltip("Maximum torque the plug can handle before binding the pin.")]
        [SerializeField, Range(0, 1)] float maxTorque = 0.8f;

        [Header("Plug Rotation")]
        [SerializeField, Min(0)] float turnSpeed = 1f;
        [SerializeField, Min(0)] float plugGravity = 5f;

        [Header("Debug")]
        [SerializeField, Range(0, 5)] private int pickingPin;
        [SerializeField, Range(0, 1)] private float appliedTorque;
        [SerializeField, Min(0)] private float chamberRetargetTimer;

        public TumblerLock Lock => tumblerLock;
        public int PickingPin => pickingPin;
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
            for (int i = 0; i < Lock.PinCount; i++)
                Lock.Chamber(i).SetLock(Lock, i);

            Lock.StopPicking();
        }

        internal void Init(IPickInput input) => this.input = input;

        private void Update()
        {
            if (input is not Object)
                return;

            HandleChangePin();
            ApplyTorque();
            TickPinRaise();
        }

        private void HandleChangePin()
        {
            chamberRetargetTimer += Time.deltaTime;
            if (chamberRetargetTimer < chamberRetargetRate)
                return;

            int delta = input.PickMoveDelta;
            if (delta == 0) return;

            chamberRetargetTimer = 0;
            tumblerLock.StopLifting(pickingPin);
            pickingPin = Lock.Config.ClampPinIndex(pickingPin + delta);
        }

        private void TickPinRaise() =>
            Lock.LiftPinTowards(pickingPin, input.PickHeight);

        /// <summary>
        /// Apply torque and rotate the plug accordingly.
        /// </summary>
        private void ApplyTorque()
        {
            AppliedTorque = input.Tension;

            if (useTensionDrift)
            {
                float randomDrift = Random.Range(-tensionKeepDrift, tensionKeepDrift);
                AppliedTorque += randomDrift;
            }


            // TODO: move this all into TumblerLock?

            bool lowTorque = AppliedTorque < MinTorque; // not enough to Set any pin
            bool highTorque = AppliedTorque > MaxTorque; // too much for the pin to move
            int tension = lowTorque ? -1 : highTorque ? 1 : 0;

            float turnDelta =
                highTorque ? turnSpeed
                : lowTorque ? -plugGravity
                : turnSpeed;

            Lock.RotatePlug(turnDelta * Time.deltaTime, tension);
        }
    }
}
