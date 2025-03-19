using Lokpik.Locks;
using SaintsField;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lokpik
{
    // https://newworldfishingguide.com/how-to-fish-in-new-world.html
    public class Lockpicker : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] InputActionReference holdTensionInput;
        [SerializeField] InputActionReference movePickInput;
        [SerializeField] InputActionReference changePinInput;
        // TODO: a "lift pick hard" button/repeated press to apply counter-rotation for unbinding a pin

        [SaintsRow(inline: true)]
        [SerializeField] TumblerLock tumblerLock;

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

        [Header("Debug")]
        [SerializeField, ReadOnly, Range(0, 1)] float appliedTorque;
        [SerializeField, ReadOnly, Range(0, 1)] float progress;
        [SerializeField, ReadOnly] int pickingPin;

        public TumblerLock Lock => tumblerLock;
        public TumblerLockConfig Config => tumblerLock.Config;
        public int PickingPin => pickingPin;

        public float MinTorque => minTorque;
        public float MaxTorque => maxTorque;

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        private void Awake() => Lock.StopPicking();

        private void OnEnable()
        {
            for (int i = 0; i < Lock.Chambers.Length; i++)
                Lock.Chambers[i].SetLock(Lock, i);
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

            // State.StartPicking(State.PickingPin + delta);
            tumblerLock.StopManipulating(pickingPin);
            pickingPin = Config.ClampPinIndex(pickingPin + delta);
        }

        private void TickPinRaise()
        {
            if (pickingPin < 0)
                return;

            float delta = movePickInput.action.ReadValue<float>();

            if (delta == 0)
                return;

            float pickMoveDelta = pickRaiseSpeed * delta;
            Lock.LiftPin(pickingPin, pickMoveDelta * Time.deltaTime);
        }

        /// <summary>
        /// Apply torque and rotate the plug accordingly.
        /// </summary>
        private void ApplyTorque()
        {
            // TODO: use two different curves
            float torque = holdTensionInput.action.inProgress
                ? tensionForce * tensionCurve.Evaluate(appliedTorque)
                : -tensionGravity;

            AppliedTorque += torque * Time.deltaTime;
            RotatePlug();
        }

        // TODO: move this into TumblerLock?
        /// <summary>
        /// Rotate the plug when adequate torque is applied
        /// </summary>
        private void RotatePlug()
        {
            // TODO: constrain plug rotation when pins are binding

            bool lowTorque = AppliedTorque < minTorque;     // not enough to Set any pin
            bool highTorque = AppliedTorque > maxTorque;    // too much for the pin to move

            // TODO: ?
            // if (highTorque && allPinsArePicked)
            // {
            // }

            Lock.Chambers[pickingPin].SetTension(highTorque ? 1 : lowTorque ? -1 : 0);

            if (highTorque)
            {
                // State.Bind(pickingPin);
                Lock.RotatePlug(turnSpeed * Time.deltaTime);
            }
            else if (lowTorque)
                Lock.RotatePlug(-plugGravity * Time.deltaTime);
            else
                Lock.RotatePlug(turnSpeed * Time.deltaTime);
        }
    }
}
