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

        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        [ShowInInspector, Ordered] public int BindingPin => bindingPin;
        [ShowInInspector, Ordered] public float BindingPoint => bindingPoint;
        [ShowInInspector, Ordered] public int PickingPin => pickingPin;
        [ShowInInspector, Ordered] public float LiftAmount => liftAmount;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        [SaintsRow(inline: true)]
        [SerializeField, Ordered] TumblerLockConfig config;

        public TumblerLockConfig Config => config;

        private int bindingPin = -1;
        private float bindingPoint;
        private int pickingPin = -1;
        private float liftAmount;

        public void Bind(int pin, float point)
        {
            bindingPin = ClampPinIndex(pin);
            bindingPoint = Mathf.Clamp(point, 0, config.GetMaxLiftForPin(pin));
        }

        public void StopBinding() => bindingPin = -1;

        public void StartPicking(int pin) => pickingPin = ClampPinIndex(pin);
        public void StopPicking() => pickingPin = -1;

        private int ClampPinIndex(int pin) => Math.Clamp(pin, 0, Config.PinCount - 1);

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
