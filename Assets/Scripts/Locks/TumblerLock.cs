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

        [LayoutGroup("Tumbler Lock State", ELayout.CollapseBox)]
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
        [LayoutGroup("./Binding Pin", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int BindingPin => bindingPin;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;
        public Chamber[] Chambers => chambers;
        public Chamber BindingChamber => chambers[bindingPin];

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

        public void StopPicking()
        {
            foreach (Chamber chamber in Chambers)
                chamber.ResetLift();

            plugRotation = 0f;
            bindingPin = -1;
        }

        // public void StartPicking(int pin)
        // {
        //     bool adequateRotation = plugRotation > Config.GetAdequatePlugRotation(pickingPin);
        //
        //     if (adequateRotation && PickingChamber.State is ChamberState.Underset or ChamberState.Overset)
        //         Bind(pickingPin, PickingChamber.KeyPinLift);
        //
        //     // TODO: don't reset if we're past the needed torque
        //     // reset the pin if it's not binding
        //     if (pickingPin != bindingPin)
        //         PickingChamber.Reset();
        //
        //     pickingPin = ClampPinIndex(pin);
        // }

        public void StopManipulating(int pin) => Chambers[pin].ResetLift();

        public void RotatePlug(float delta)
        {
            float maxRotation = GetMaxPlugRotation();

            // bool allPinsPicked = chambers.All(c => c.IsPicked);
            // float maxRotation = PickingChamber.State switch
            // {
            //     _ when allPinsPicked => 1,
            //     ChamberState.Underset or ChamberState.Overset => Config.GetAdequatePlugRotation(pickingPin),
            //     ChamberState.Set or ChamberState.AboveShearLine => Config.GetAdequatePlugRotation(pickingPin + 1),
            //     _ => 0
            // };

            plugRotation = Mathf.Clamp(PlugRotation + delta, 0, maxRotation);
            IsLocked = PlugRotation < 1;
        }

        public void LiftPin(int pin, float delta) => Chambers[pin].LiftPin(delta);

        public void StopBinding() => bindingPin = -1;
        public void Bind(int pin)
        {
            if (pin < 0 || pin >= Config.PinCount)
                return;

            if (!Chambers[pin].IsBlocking)
                return;

            bindingPin = Config.ClampPinIndex(pin);
            // TODO: figure out where to handle binding point (binding rotation)

            Chambers[pin].Bind();
            // var bindingPoint = Mathf.Clamp(, 0, config.GetMaxLiftForPin(pin));
            // PickingChamber.BindingPoint = point;
        }

        public float GetMaxPlugRotation()
        {
            float maxRotation = 1;

            for (int pin = 0; pin < Chambers.Length; pin++)
            {
                if (Chambers[pin].IsPicked)
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
