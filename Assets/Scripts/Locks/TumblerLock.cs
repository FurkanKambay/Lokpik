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
        [SerializeField, RO] private float minPlugProgress;

        public void SubscribeToChamberStateChanges(Chamber.ChamberStateChangeEvent handler)
        {
            foreach (Chamber chamber in chambers)
                chamber.OnStateChange += handler;
        }

        public void TurnPlugTowards(float desiredRotation, bool preventUnsettingPins = false)
        {
            minPlugProgress = preventUnsettingPins ? Config.GetAdequatePlugRotation(previousPin) + 0.05f : 0f;
            plugRotation = Mathf.Clamp(desiredRotation, minPlugProgress, FindMaxPlugRotation());

            previousPin = Config.FindPreviousPinAt(plugRotation);

            for (int i = 0; i < chambers.Length; i++)
            {
                Chamber chamber = chambers[i];
                float bindRotation = Config.GetAdequatePlugRotation(i);

                if (plugRotation < bindRotation)
                    chamber.SetTension(-1);
                else if (plugRotation >= bindRotation)
                    chamber.SetTension(1);
            }

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
        }

        public void StopLifting(int pin) =>
            Chamber(pin).StopLifting();

        public Chamber Chamber(int pin) =>
            chambers.ElementAtOrDefault(Config.ClampPinIndex(pin));

        public float FindMaxPlugRotation()
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
