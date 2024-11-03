using System;
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
        [SerializeField] InputActionReference addTensionAction;
        [SerializeField] InputActionReference holdTensionAction;
        [SerializeField] InputActionReference raisePickAction;
        [SerializeField] InputActionReference nextPinAction;

        [Header("Initial Config")]
        [SerializeField] LockConfig config;

        [Header("Plug Rotation")]
        [SerializeField, Min(0)] float turnSpeed = 1f;
        [SerializeField, Min(0)] float plugGravity = 5f;

        [Header("Pin Raise")]
        [SerializeField, Min(0)] float pinRaiseSpeed = 2f;
        [SerializeField, Min(0)] float pinGravity = 5f;
        // [SerializeField] float coyoteTime = 0.5f;

        [Header("Debug")]
        [SerializeField] bool isLocked;
        [SerializeField, Range(0, 1)] float currentProgress;
        [SerializeField, Range(0, 2)] int currentPin;
        [SerializeField, Range(0, 1)] float[] pinRises;

        public float CurrentProgress
        {
            get => currentProgress;
            set
            {
                currentProgress = Mathf.Clamp(value, GetMinRise(currentPin), GetMaxRise(currentPin));
                IsLocked = value < config.LastBindPoint;
            }
        }

        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (isLocked == value)
                    return;

                isLocked = value;
                (value ? OnLocked : OnUnlocked)?.Invoke();
            }
        }

        public float[] PinRiseAmounts => pinRises;

        private void Awake()
        {
            CurrentProgress = 0;
            currentPin = 0;
        }

        private void Update()
        {
            if (nextPinAction.action.triggered)
            {
                currentPin = (currentPin + 1) % (config.PinCount - 1);
                return;
            }

            TickPinRaise();
            TickPlugRotation();
        }

        private void TickPinRaise()
        {
            float riseAmount = -pinGravity;

            if (raisePickAction.action.inProgress)
                riseAmount = pinRaiseSpeed;

            float riseDelta = riseAmount * Time.deltaTime;

            float minRise = GetMinRise(currentPin);
            float maxRise = GetMaxRise(currentPin);

            float bindRisePoint = config.BindPoints[currentPin];
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

            pinRises[currentPin] = Mathf.Clamp(pinRises[currentPin] + riseDelta, minRise, maxRise);
        }

        private void TickPlugRotation()
        {
            // TODO: add coyote time
            float tension = -plugGravity;

            if (addTensionAction.action.inProgress)
                tension = turnSpeed;
            else if (holdTensionAction.action.inProgress)
            {
                // TODO: random drift in tension
                tension = 0f;
            }

            CurrentProgress += tension * Time.deltaTime;
        }

        private float GetMinRise(int pin)
        {
            float shear = config.BindPoints[pin];
            return CurrentProgress < shear ? 0f : shear;
        }

        private float GetMaxRise(int pin)
        {
            float shear = config.BindPoints[pin];
            return CurrentProgress < shear ? shear : 1f;
        }

        private void OnValidate()
        {
            CurrentProgress = currentProgress;

            Array.Resize(ref pinRises, config.PinCount);

            for (int i = 0; i < pinRises.Length; i++)
                pinRises[i] = 0;
        }

        // /// <param name="progress">Normalized [0, 1]</param>
        // /// <returns>Angle in range [0, <see cref="UnlockAngle"/>]</returns>
        // public float ProgressToAngle(float progress) => Mathf.LerpUnclamped(0, unlockAngle, progress);
    }
}
