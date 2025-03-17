using System;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik.TumblerLock
{
    [Serializable]
    public class TumblerLock : ISerializationCallbackReceiver
    {
        // public event Action OnLocked;
        // public event Action OnUnlocked;

        [LayoutGroup("Tumbler Lock State", ELayout.TitleBox)]

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] Chamber[] chambers;

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        [LayoutGroup("./Manipulated Pin", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int PickingPin => pickingPin;
        // [ShowInInspector, Ordered] public float LiftAmount => liftAmount;
        // [ShowInInspector, Ordered] public ChamberState PinState => pinState;
        [ShowInInspector, Ordered] public float PlugRotation => plugRotation;

        [LayoutGroup("./Binding Pin", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int BindingPin => bindingPin;
        // [ShowInInspector, Ordered] public float BindingPoint => bindingPoint;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;
        public Chamber[] Chambers => chambers;
        public Chamber PickingChamber => chambers[pickingPin];
        public Chamber BindingChamber => chambers[bindingPin];

        private float plugRotation;
        private int pickingPin = 0;
        private int bindingPin = -1;
        // private float liftAmount;
        // private ChamberState pinState;
        // private float bindingPoint;

        public void StopPicking()
        {
            foreach (Chamber chamber in Chambers)
                chamber.Reset();

            (pickingPin, plugRotation) = (0, 0f);
        }

        public void StartPicking(int pin)
        {
            bool adequateRotation = plugRotation > Config.GetAdequateRotation(pickingPin);

            if (adequateRotation && PickingChamber.State is ChamberState.Underset or ChamberState.Overset)
            {

                Bind(pickingPin, PickingChamber.KeyPinLift);
            }

            // reset the pin if it's not binding
            if (pickingPin != bindingPin)
                PickingChamber.Reset();

            pickingPin = ClampPinIndex(pin);
        }

        public void RotatePlug(float delta)
        {
            // TODO: this would be "isLastBindingPin"
            bool isLastPin = PickingPin == Config.LastPinIndex;

            bool isUnlocking = isLastPin && PickingChamber.State.IsPicked();
            float maxRotation = PickingChamber.State switch
            {
                ChamberState.Underset or ChamberState.Overset => Config.GetAdequateRotation(pickingPin),
                ChamberState.Set or ChamberState.AboveShearLine => Config.GetAdequateRotation(pickingPin + 1),
                _ => 0
            };
            plugRotation = Mathf.Clamp(plugRotation + delta, 0, maxRotation);
        }

        public void LiftPin(float delta) => PickingChamber.LiftPin(delta);

        // public void StopBinding() => (bindingPin, bindingPoint) = (-1, 0);
        public void Bind(int pin, float point)
        {
            bindingPin = ClampPinIndex(pin);
            // PickingChamber.
            // bindingPoint = Mathf.Clamp(point, 0, config.GetMaxLiftForPin(pin));
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
