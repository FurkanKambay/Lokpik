using System;
using Lokpik.TumblerLock;
using SaintsField;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Lokpik
{
    // https://newworldfishingguide.com/how-to-fish-in-new-world.html
    public class Lock : MonoBehaviour
    {
        public event Action OnLocked;
        public event Action OnUnlocked;

        [Header("Input")]
        [SerializeField] InputActionReference holdTensionInput;
        [SerializeField] InputActionReference movePickInput;
        [SerializeField] InputActionReference changePinInput;

        [SaintsRow(inline: true)]
        [SerializeField] TumblerLock.TumblerLock state;

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
        [SerializeField, ReadOnly] bool isLocked;
        [SerializeField, ReadOnly, Range(0, 1)] float appliedTorque;
        [SerializeField, ReadOnly, Range(0, 1)] float progress;

        public TumblerLock.TumblerLock State => state;
        public TumblerLockConfig Config => state.Config;

        public float MinTorque => minTorque;
        public float MaxTorque => maxTorque;

        public float AppliedTorque
        {
            get => appliedTorque;
            private set => appliedTorque = Mathf.Clamp(value, 0, 1);
        }

        // public float Progress
        // {
        //     get => progress;
        //     private set
        //     {
        //         progress = Mathf.Clamp(value, 0, 1);
        //         IsLocked = progress < 1;
        //     }
        // }

        public bool IsLocked
        {
            get => isLocked;
            private set
            {
                if (isLocked == value)
                    return;

                isLocked = value;
                (value ? OnLocked : OnUnlocked)?.Invoke();
            }
        }

        private void Awake() => State.StopPicking();

        private void Update()
        {
            HandleChangePin();
            ApplyTorque();
            TickPinRaise();
        }

        private void HandleChangePin()
        {
            int delta = (int)changePinInput.action.ReadValue<float>();

            if (delta == 0)
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

        /// <summary>
        /// Rotate the plug when adequate torque is applied
        /// </summary>
        private void RotatePlug()
        {
            // TODO: constrain plug rotation when pins are binding

            bool lowTorque = AppliedTorque < minTorque;
            bool highTorque = AppliedTorque > maxTorque;

            float adequateRotation = Config.GetAdequateRotation(State.PickingPin);
            float deltaForBinding = adequateRotation - State.PlugRotation + 0.05f;

            // TODO: move all this into TumblerLockState?
            float turnDelta = (lowTorque, highTorque) switch
            {
                (_, true) when !IsLocked => turnSpeed,  // unlocked: free rotation
                (_, true) => deltaForBinding,           // tension is too high: binding
                (true, _) => -plugGravity,              // tension is too low
                (false, false) => turnSpeed             // tension is adequate
            };

            State.RotatePlug(turnDelta * Time.deltaTime);
        }

        private void OnValidate()
        {
            // Progress = progress;

            // Array.Resize(ref pinRises, Config.PinCount);
            // for (int i = 0; i < pinRises.Length; i++)
            //     pinRises[i] = 0;
        }
    }
}
