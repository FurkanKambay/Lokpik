using System;
using System.Linq;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik.Locks
{
    [Serializable]
    public class TumblerLock : ISerializationCallbackReceiver
    {
        public event Action OnLocked;
        public event Action OnUnlocked;

        [LayoutGroup("Tumbler Lock State", ELayout.FoldoutBox)]
        [SerializeField, Ordered, ReadOnly] bool isLocked;

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        /// <summary>
        /// The progression of the plug rotation normalized in the range of [0,1].
        /// </summary>
        /// <remarks>Use <see cref="RotatePlug"/> to modify.</remarks>
        [ShowInInspector, Ordered] public float PlugRotation => plugRotation;

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] Chamber[] chambers;

        /// <summary>
        /// The pin currently binding due to <see cref="PlugRotation"/> and Chamber <see cref="Chamber.State"/>.
        /// </summary>
        [LayoutGroup("./Binding", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int Tension => tension;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;
        public int PinCount => Config.PinCount;

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

        private int tension = -1;
        private float plugRotation;
        private int previousPin = -1;
        private int nextPin = -1;

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

        public void RotatePlug(float delta, int tensionValue)
        {
            tension = tensionValue;
            previousPin = Config.FindPreviousPinAt(PlugRotation);
            nextPin = Config.FindNextPinAt(PlugRotation);

            switch (delta)
            {
                case > 0: NextChamber?.SetTension(1); break;
                case < 0: PreviousChamber?.SetTension(-1); break;
            }

            NextChamber?.SetTension(Tension);

            plugRotation = Mathf.Clamp(PlugRotation + delta, 0, GetMaxPlugRotation());
            IsLocked = PlugRotation < 1;
        }

        public void LiftPin(int pin, float delta) =>
            Chamber(pin).Lift(delta);

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
