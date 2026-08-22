using System;
using System.Linq;
using FK.Lokpik.Data;
using UnityEngine;
using Vertx.Attributes;

namespace FK.Lokpik.Locks
{
    using RO = ReadOnlyFieldAttribute;

    [Serializable]
    public class TumblerLock : ISerializationCallbackReceiver
    {
        public event Action OnLocked;
        public event Action OnUnlocked;

        [SerializeField] private TumblerLockConfigAsset configAsset;
        [SerializeField, Inline] private Chamber[] chambers;

        public TumblerLockConfig Config => configAsset.LockConfig;

        /// <summary>
        /// The progression of the plug rotation normalized in the range of [0,1].
        /// </summary>
        /// <remarks>Use <see cref="TurnPlugTowards"/> to manipulate.</remarks>
        public float PlugTurnProgress => plugRotation;

        public Chamber PreviousChamber => previousPin < 0 ? null : Chamber(previousPin);
        public Chamber NextChamber => nextPin < 0 ? null : Chamber(nextPin);

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

        [Header("Debug")]
        [SerializeField, RO] private bool isLocked;
        [SerializeField, RO] private float plugRotation;
        [SerializeField, RO] private int previousPin = -1;
        [SerializeField, RO] private int nextPin = -1;

        public void TurnPlugTowards(float desiredRotation)
        {
            previousPin = Config.FindPreviousPinAt(plugRotation);
            nextPin = Config.FindNextPinAt(plugRotation);
            plugRotation = Mathf.Clamp(desiredRotation, 0, GetMaxPlugRotation());

            float prevRotation = Config.GetAdequatePlugRotation(previousPin);
            float nextRotation = Config.GetAdequatePlugRotation(nextPin);

            if (desiredRotation < prevRotation)
            {
                PreviousChamber?.SetTension(-1);
                NextChamber?.SetTension(-1);
            }
            else if (desiredRotation < nextRotation)
                NextChamber?.SetTension(-1);
            else if (desiredRotation >= nextRotation)
                NextChamber?.SetTension(1);

            IsLocked = plugRotation < 1;
        }

        public void LiftPinTowards(int pin, float desiredTarget) =>
            Chamber(pin).LiftTowards(desiredTarget);

        public void StopPicking()
        {
            foreach (Chamber chamber in chambers)
                chamber.StopLifting();

            plugRotation = 0f;
            previousPin = -1;
            nextPin = Config.FindNextPinAt(0);
        }

        public void StopLifting(int pin) =>
            Chamber(pin).StopLifting();

        public Chamber Chamber(int pin) =>
            chambers.ElementAtOrDefault(Config.ClampPinIndex(pin));

        public float GetMaxPlugRotation()
        {
            float maxRotation = 1;

            for (int pin = 0; pin < chambers.Length; pin++)
            {
                if (chambers[pin].IsPicked)
                    continue;

                maxRotation = Mathf.Min(maxRotation, Config.GetAdequatePlugRotation(pin));
            }

            return maxRotation;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (Config != null)
                Array.Resize(ref chambers, Config.PinCount);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
