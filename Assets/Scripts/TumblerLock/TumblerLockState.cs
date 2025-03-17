using System;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik.TumblerLock
{
    [Serializable]
    public class TumblerLockState : ISerializationCallbackReceiver
    {
        // public event Action OnLocked;
        // public event Action OnUnlocked;

        [LayoutGroup("Tumbler Lock State", ELayout.TitleBox)]
        [SerializeField, Ordered] float tolerance;

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        [LayoutGroup("./Manipulated Pin", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int PickingPin => pickingPin;
        [ShowInInspector, Ordered] public float LiftAmount => liftAmount;
        [ShowInInspector, Ordered] public PinStackState PinState => pinState;
        [ShowInInspector, Ordered] public float PlugRotation => plugRotation;

        [LayoutGroup("./Binding Pin", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int BindingPin => bindingPin;
        [ShowInInspector, Ordered] public float BindingPoint => bindingPoint;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;

        private int pickingPin = -1;
        private float liftAmount;
        private PinStackState pinState;
        private float plugRotation;

        private int bindingPin = -1;
        private float bindingPoint;

        public void StopPicking()
        {
            (pickingPin, plugRotation) = (-1, 0f);
            StopBinding();
        }

        public void StartPicking(int pin)
        {
            bool adequateRotation = plugRotation > Config.GetAdequateRotation(pickingPin);

            if (adequateRotation && pinState is PinStackState.Underset or PinStackState.Overset)
                Bind(pickingPin, liftAmount);

            // reset the pin if it's not binding
            if (pickingPin != bindingPin)
                liftAmount = 0;

            pickingPin = ClampPinIndex(pin);
        }

        public void RotatePlug(float delta)
        {
            float maxRotation = PinState switch
            {
                PinStackState.Underset or PinStackState.Overset => Config.GetAdequateRotation(pickingPin),
                PinStackState.Set or PinStackState.AboveShearLine => Config.GetAdequateRotation(pickingPin + 1),
                _ => 0
            };
            plugRotation = Mathf.Clamp(plugRotation + delta, 0, maxRotation);
        }

        public void LiftPin(float delta)
        {
            liftAmount = Mathf.Clamp(liftAmount + delta, 0, config.GetMaxLiftForPin(pickingPin));

            float keyPinLength = Config.KeyPinLengths[pickingPin];
            float correctLift = Config.ShearLine - keyPinLength;

            if (Math.Abs(liftAmount - correctLift) < tolerance)
                pinState = PinStackState.Set;
            else if (liftAmount >= Config.ShearLine)
                pinState = PinStackState.AboveShearLine;
            else if (liftAmount > correctLift)
                pinState = PinStackState.Overset;
            else if (liftAmount < correctLift)
                pinState = PinStackState.Underset;
        }

        public void StopBinding() => (bindingPin, bindingPoint) = (-1, 0);
        public void Bind(int pin, float point)
        {
            bindingPin = ClampPinIndex(pin);
            bindingPoint = Mathf.Clamp(point, 0, config.GetMaxLiftForPin(pin));
        }

        private int ClampPinIndex(int pin) => Math.Clamp(pin, 0, Config.PinCount - 1);

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
