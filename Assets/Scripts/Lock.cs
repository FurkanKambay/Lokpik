using System;
using SaintsField;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lokpik
{
    // https://newworldfishingguide.com/how-to-fish-in-new-world.html
    public class Lock : MonoBehaviour
    {
        public event Action OnLocked;
        public event Action OnUnlocked;

        [Header("Input")]
        [SerializeField] InputActionReference holdTensionAction;
        [SerializeField] InputActionReference pickRaiseAction;
        [SerializeField] InputActionReference pickLowerAction;
        [SerializeField] InputActionReference nextPinAction;

        [SaintsRow(inline: true)]
        [SerializeField] TumblerLockState state;

        [Header("Tension Wrench")]
        [SerializeField] AnimationCurve tensionCurve;
        [SerializeField, Min(0)] float tensionForce = 0.2f;
        [SerializeField, Min(0)] float tensionGravity = 0.2f;

        [Tooltip("Minimum tension required to turn plug.")]
        [SerializeField, Range(0, 1)] float minTension = 0.5f;

        [Tooltip("Maximum tension the plug can handle.")]
        [SerializeField, Range(0, 1)] float maxTension = 0.8f;

        [Header("Plug Rotation")]
        [SerializeField, Min(0)] float turnSpeed = 1f;
        [SerializeField, Min(0)] float plugGravity = 5f;

        [Header("Pin Raise")]
        [SerializeField, Min(0)] float pinRaiseSpeed = 2f;
        // [SerializeField] float coyoteTime = 0.5f;

        [Header("Debug")]
        [SerializeField, ReadOnly] bool isLocked;
        [SerializeField, ReadOnly, Range(0, 1)] float appliedTension;
        [SerializeField, ReadOnly, Range(0, 1)] float progress;
        [SerializeField, ReadOnly, Range(0, 2)] int currentPin;
        [SerializeField, ReadOnly, Range(0, 1)] float[] pinRises;

        public TumblerLockState State => state;
        public TumblerLockConfig Config => state.Config;

        public float MinTension => minTension;
        public float MaxTension => maxTension;

        public float[] PinRiseAmounts => pinRises;

        public float AppliedTension
        {
            get => appliedTension;
            private set => appliedTension = Mathf.Clamp(value, 0, 1);
        }

        public float Progress
        {
            get => progress;
            private set
            {
                // constrain plug rotation
                progress = Mathf.Clamp(value, GetMinRise(currentPin), GetMaxRise(currentPin));
                // progress = Mathf.Clamp(value, 0, 1);

                // does this make sense?
                // IsLocked = value < config.LastBindPoint;
                IsLocked = progress < 1;
            }
        }

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

        private void Awake() => ResetProgress();

        private void Update()
        {
            // move to the next pin
            if (nextPinAction.action.triggered)
                currentPin = (currentPin + 1) % (Config.PinCount - 1);

            ApplyTension();
            TickPinRaise();
            // TickPlugRotation();
        }

        private void TickPinRaise()
        {
            float pinMoveDelta = 0;

            if (pickRaiseAction.action.inProgress)
                pinMoveDelta = pinRaiseSpeed;
            else if (pickLowerAction.action.inProgress)
                pinMoveDelta = -pinRaiseSpeed;

            float minRise = GetMinRise(currentPin);
            float maxRise = GetMaxRise(currentPin);

            float bindRisePoint = Config.KeyPinLengths[currentPin];
            float rise = pinRises[currentPin];

            // if (CurrentProgress < bindRisePoint)
            // {
            //     minRise = 0;
            //     maxRise = 1;
            // }
            // else if (CurrentProgress >= bindRisePoint && rise >= bindRisePoint)
            // {
            //     minRise = bindRisePoint;
            //     maxRise = 1;
            // }
            // else if (CurrentProgress >= bindRisePoint && rise < bindRisePoint)
            // {
            //     minRise = 0;
            //     maxRise = bindRisePoint;
            // }

            float newRiseValue = pinRises[currentPin] + (pinMoveDelta * Time.deltaTime);
            // pinRises[currentPin] = Mathf.Clamp(newRiseValue, minRise, maxRise);
            pinRises[currentPin] = Mathf.Clamp(newRiseValue, minRise, 1);
        }

        /// <summary>
        /// Apply tension and rotate the plug accordingly.
        /// </summary>
        private void ApplyTension()
        {
            float tension = holdTensionAction.action.inProgress ? tensionForce : -tensionGravity;
            tension *= tensionCurve.Evaluate(AppliedTension);

            AppliedTension += tension * Time.deltaTime;
            RotatePlug();
        }

        /// <summary>
        /// Rotate the plug when adequate tension is applied
        /// </summary>
        private void RotatePlug()
        {
            // TODO: constrain plug rotation when pins are binding

            bool lowTension = AppliedTension < minTension;
            bool highTension = AppliedTension > maxTension;

            float turnAmount = (lowTension, highTension) switch
            {
                (_, true) when !IsLocked => turnSpeed,  // unlocked: free rotation
                (_, true) => 0,                         // tension is too high: get stuck
                (true, _) => -plugGravity,              // tension is too low
                (false, false) => turnSpeed             // tension is adequate
            };

            Progress += turnAmount * Time.deltaTime;
        }

        private float GetMinRise(int pin)
        {
            float bindingPoint = Config.KeyPinLengths[pin];
            return Progress < bindingPoint ? 0f : bindingPoint;
        }

        private float GetMaxRise(int pin)
        {
            float bindingPoint = Config.KeyPinLengths[pin];
            return Progress < bindingPoint ? bindingPoint : 1f;
        }

        private void ResetProgress()
        {
            Progress = 0;
            currentPin = 0;
        }

        private void OnValidate()
        {
            Progress = progress;

            Array.Resize(ref pinRises, Config.PinCount);

            for (int i = 0; i < pinRises.Length; i++)
                pinRises[i] = 0;
        }

        // /// <param name="progress">Normalized [0, 1]</param>
        // /// <returns>Angle in range [0, <see cref="UnlockAngle"/>]</returns>
        // public float ProgressToAngle(float progress) => Mathf.LerpUnclamped(0, unlockAngle, progress);
    }
}
