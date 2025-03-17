using System;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik
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

        [LayoutGroup("./Binding Pin", ELayout.TitleOut)]
        [ShowInInspector, Ordered] public int BindingPin => bindingPin;
        [ShowInInspector, Ordered] public float BindingPoint => bindingPoint;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;

        private int pickingPin = -1;
        private float liftAmount;
        PinStackState pinState;

        private int bindingPin = -1;
        private float bindingPoint;

        public void StopPicking() => pickingPin = -1;
        public void StartPicking(int pin)
        {
            // TODO only when the torque is too much!
            // if (torqueTooMuch && pinState is PinStackState.Underset or PinStackState.Overset)
            //     bindingPin = pickingPin;

            // if the pin is binding, save the binding point
            // if not, reset the pin
            if (pickingPin == bindingPin)
                bindingPoint = liftAmount;
            else
                liftAmount = 0;

            pickingPin = ClampPinIndex(pin);
        }

        public void LiftPin(float newLiftAmount)
        {
            liftAmount = Mathf.Clamp(newLiftAmount, 0, config.GetMaxLiftForPin(pickingPin));

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

        public void StopBinding() => bindingPin = -1;
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
