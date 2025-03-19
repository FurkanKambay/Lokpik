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

        [LayoutGroup("Tumbler Lock State", ELayout.CollapseBox)]
        [SerializeField, Ordered, ReadOnly] bool isLocked;

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        /// <summary>
        /// The pin currently being manipulated by a tool.
        /// </summary>
        [ShowInInspector, Ordered] public int PickingPin => pickingPin;

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
        public Chamber PickingChamber => chambers[pickingPin];
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
        private int pickingPin;
        private int bindingPin = -1;

        public void StopPicking()
        {
            foreach (Chamber chamber in Chambers)
                chamber.Reset();

            pickingPin = 0;
            plugRotation = 0f;
        }

        public void StartPicking(int pin)
        {
            bool adequateRotation = plugRotation > Config.GetAdequatePlugRotation(pickingPin);

            if (adequateRotation && PickingChamber.State is ChamberState.Underset or ChamberState.Overset)
                Bind(pickingPin, PickingChamber.KeyPinLift);

            // TODO: don't reset if we're past the needed torque
            // reset the pin if it's not binding
            if (pickingPin != bindingPin)
                PickingChamber.Reset();

            pickingPin = ClampPinIndex(pin);
        }

        public void RotatePlug(float delta)
        {
            bool allPinsPicked = chambers.All(c => c.IsPicked);

            float maxRotation = PickingChamber.State switch
            {
                _ when allPinsPicked => 1,
                ChamberState.Underset or ChamberState.Overset => Config.GetAdequatePlugRotation(pickingPin),
                ChamberState.Set or ChamberState.AboveShearLine => Config.GetAdequatePlugRotation(pickingPin + 1),
                _ => 0
            };

            plugRotation = Mathf.Clamp(plugRotation + delta, 0, maxRotation);
            IsLocked = plugRotation < 1;
        }

        public void LiftPin(float delta) => PickingChamber.LiftPin(delta);

        // public void StopBinding() => (bindingPin, bindingPoint) = (-1, 0);
        public void Bind(int pin, float point)
        {
            bindingPin = ClampPinIndex(pin);
            // TODO: figure out where to handle binding point (binding rotation)
            // bindingPoint = Mathf.Clamp(point, 0, config.GetMaxLiftForPin(pin));
            // PickingChamber.BindingPoint = point;
        }

        private int ClampPinIndex(int pin) => Math.Clamp(pin, 0, Config.LastPinIndex);

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (Config != null)
                Array.Resize(ref chambers, Config.PinCount);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
