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

        [SaintsRow(inline: true)]
        [SerializeField] TumblerLock state;

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

        public TumblerLock State => state;
        public TumblerLockConfig Config => state.Config;

        public float MinTorque => minTorque;
        public float MaxTorque => maxTorque;

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        private void Awake() => State.StopPicking();

        private void OnEnable()
        {
            for (int i = 0; i < State.Chambers.Length; i++)
                State.Chambers[i].SetLock(State, i);
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

            State.StartPicking(State.PickingPin + delta);
        }

        private void TickPinRaise()
        {
            if (State.PickingPin == -1)
                return;

            float delta = movePickInput.action.ReadValue<float>();

            if (delta == 0)
                return;

            float pickMoveDelta = pickRaiseSpeed * delta;
            State.LiftPin(pickMoveDelta * Time.deltaTime);
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

            bool lowTorque = AppliedTorque < minTorque;
            bool highTorque = AppliedTorque > maxTorque;

            bool isPicked = State.PickingChamber.State.IsPicked();
            bool isLastPin = State.PickingPin == Config.LastPinIndex;

            float adequateRotation = isLastPin && isPicked ? 1 : Config.GetAdequatePlugRotation(
                isPicked ? State.PickingPin + 1 : State.PickingPin);

            float deltaForBinding = adequateRotation + 0.05f - State.PlugRotation;

            float turnDelta = (lowTorque, highTorque) switch
            {
                (_, true) when !State.IsLocked => turnSpeed,    // unlocked: free rotation
                (_, true) => deltaForBinding,                   // tension is too high: binding
                (true, _) => -plugGravity,                      // tension is too low
                (false, false) => turnSpeed                     // tension is adequate
            };

            State.RotatePlug(turnDelta * Time.deltaTime);
        }
    }
}
