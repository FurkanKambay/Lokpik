using System;
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
        [ShowInInspector, Ordered] public int BindingPin => bindingPin;
        [ShowInInspector, Ordered] public int NextBindingPin => nextBindingPin;
        [ShowInInspector, Ordered] public int Tension => tension;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;
        public int PinCount => Config.PinCount;
        public Chamber BindingChamber => Chamber(BindingPin);
        public Chamber NextBindingChamber => Chamber(NextBindingPin);

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

        private float plugRotation;
        private int bindingPin = -1;
        private int tension = -1;
        private int nextBindingPin = -1;

        public void StopPicking()
        {
            foreach (Chamber chamber in chambers)
                chamber.ResetLift();

            plugRotation = 0f;
            bindingPin = -1;
            nextBindingPin = Config.FindBindingPin(0);
        }

        public void StopManipulating(int pin) =>
            Chamber(pin).ResetLift();

        public void SetTension(int value)
        {
            tension = value;

            if (NextBindingPin != -1)
                NextBindingChamber.SetTension(Tension);
        }

        public void RotatePlug(float delta)
        {
            if (NextBindingPin != -1 && delta > 0)
                NextBindingChamber.SetTension(1);
            else if (NextBindingPin != -1 && delta < 0)
                Chamber(nextBindingPin - 1).SetTension(-1);

            nextBindingPin = Config.FindBindingPin(PlugRotation);

            if (nextBindingPin != -1)
                NextBindingChamber.SetTension(Tension);

            plugRotation = Mathf.Clamp(PlugRotation + delta, 0, GetMaxPlugRotation());
            IsLocked = PlugRotation < 1;
        }

        public void LiftPin(int pin, float delta) =>
            Chamber(pin).Lift(delta);

        public Chamber Chamber(int pin) =>
            chambers[Config.ClampPinIndex(pin)];

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
