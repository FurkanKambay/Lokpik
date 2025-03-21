using Lokpik.Locks;
using SaintsField;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lokpik
{
    public class Lockpicker : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] InputActionReference holdTensionInput;
        [SerializeField] InputActionReference movePickInput;
        [SerializeField] InputActionReference changePinInput;
        // TODO: a "lift pick hard" button/repeated press to apply counter-rotation for unbinding a pin

        [SaintsRow(inline: true)]
        [SerializeField] TumblerLock tumblerLock;

        // TODO: use two different curves?
        [Header("Tension Wrench")]
        [SerializeField] AnimationCurve tensionCurve;
        [SerializeField, Min(0)] float tensionForce = 0.2f;
        [SerializeField, Min(0)] float tensionGravity = 0.2f;

        [Tooltip("Minimum torque required to turn plug.")]
        [SerializeField, Range(0, 1)] float minTorque = 0.5f;

        [Tooltip("Maximum torque the plug can handle before binding the pin.")]
        [SerializeField, Range(0, 1)] float maxTorque = 0.8f;

        [Header("Plug Rotation")]
        [SerializeField, Min(0)] float turnSpeed = 1f;
        [SerializeField, Min(0)] float plugGravity = 5f;

        [Header("Pick")]
        [SerializeField, Min(0)] float pickRaiseSpeed = 2f;
        // [SerializeField] float coyoteTime = 0.5f;

        public TumblerLock Lock => tumblerLock;

        public float MinTorque => minTorque;
        public float MaxTorque => maxTorque;

        public int PickingPin { get; private set; }

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        private float appliedTorque;

        private void Awake()
        {
            for (int i = 0; i < Lock.PinCount; i++)
                Lock.Chamber(i).SetLock(Lock, i);

            Lock.StopPicking();
        }

        private void Update()
        {
            HandleChangePin();
            ApplyTorque();
            TickPinRaise();
        }

        private void HandleChangePin()
        {
            int delta = (int)changePinInput.action.ReadValue<float>();

            if (!changePinInput.action.triggered || delta == 0)
                return;

            tumblerLock.StopManipulating(PickingPin);
            PickingPin = Lock.Config.ClampPinIndex(PickingPin + delta);
        }

        private void TickPinRaise()
        {
            if (PickingPin < 0)
                return;

            float delta = movePickInput.action.ReadValue<float>();

            if (delta == 0)
                return;

            float pickMoveDelta = pickRaiseSpeed * delta;
            Lock.LiftPin(PickingPin, pickMoveDelta * Time.deltaTime);
        }

        /// <summary>
        /// Apply torque and rotate the plug accordingly.
        /// </summary>
        private void ApplyTorque()
        {
            float torque = holdTensionInput.action.inProgress
                ? tensionForce * tensionCurve.Evaluate(AppliedTorque)
                : -tensionGravity;

            AppliedTorque += torque * Time.deltaTime;

            // TODO: move this all into TumblerLock?

            bool lowTorque = AppliedTorque < MinTorque;     // not enough to Set any pin
            bool highTorque = AppliedTorque > MaxTorque;    // too much for the pin to move
            int tension = lowTorque ? -1 : highTorque ? 1 : 0;

            float turnDelta =
                highTorque ? turnSpeed :
                lowTorque ? -plugGravity
                : turnSpeed;

            Lock.RotatePlug(turnDelta * Time.deltaTime, tension);
        }
    }
}
