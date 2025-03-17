using System;
using System.Linq;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik.TumblerLock
{
    [Serializable]
    public class TumblerLockConfig : ISerializationCallbackReceiver
    {
        public const float ChamberHeight = 1f;

        [LayoutGroup("Tumbler Lock Config", ELayout.TitleBox)]
        [SerializeField, Range(0, 1)] float shearLine = 0.5f;
        [SerializeField, Ordered] float tolerance = 0.01f;

        [LayoutGroup("./Pins", ELayout.TitleOut)]
        [SerializeField] bool uniformDriverPins;

        [SerializeField, Range(0, 1)] float[] driverPinLengths = { 0.5f, 0.5f };
        [SerializeField, Range(0, 1)] float[] keyPinLengths = { 0.2f, 0.4f };

        [ArraySize(nameof(PinCount))]
        [SerializeField, ReadOnly] int[] bindingOrder;

        [LayoutGroup("./Info", ELayout.TitleOut)]
        [ShowInInspector] internal bool IsVulnerableToCombPicking =>
            Enumerable.Range(0, PinCount).All(pin => GetMaxLiftForPin(pin) >= ShearLine);

        public int PinCount => keyPinLengths.Length;
        public int LastPinIndex => PinCount - 1;

        public float ShearLine => shearLine;
        public float Tolerance => tolerance;

        public float[] DriverPinLengths => driverPinLengths;
        public float[] KeyPinLengths => keyPinLengths;
        public int[] BindingOrder => bindingOrder;
        public float LastBindingPin => bindingOrder.Last();

        internal float MaxKeyPinHeight => keyPinLengths.Max();
        internal float MaxDriverPinHeight => driverPinLengths.Max();

        // /// <returns>The index of the binding pin at <paramref name="progress"/>. -1 if not found.</returns>
        // public int GetBindingPinAtPlugRotation(float progress)
        // {
        //     for (int i = 0; i < bindPoints.Length; i++)
        //     {
        //         if (bindPoints[i] > progress)
        //             return i;
        //     }
        //     return -1;
        // }

        /// <summary>
        /// Return the rotation at which the <paramref name="pin"/> can stay <see cref="ChamberState.Set"/>.
        /// </summary>
        internal float GetAdequateRotation(int pin)
        {
            // TODO: define binding order and take the IndexOf in that instead of `pin` directly
            return (pin + 1) * 0.1f;
        }

        internal float GetMaxLiftForPin(int pin)
        {
            if (pin < 0 || pin >= PinCount)
                return 0;

            float driverPinLength = DriverPinLengths[pin];
            float keyPinLength = KeyPinLengths[pin];
            return ChamberHeight - (driverPinLength + keyPinLength);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (uniformDriverPins)
                Array.Fill(driverPinLengths, driverPinLengths[0]);

            // bool allIndicesExist = bindingOrder.OrderBy(i => i).Equals(Enumerable.Range(0, PinCount));

            for (int pin = 0; pin < PinCount; pin++)
            {
                float maxKeyPinLength = ChamberHeight - driverPinLengths[pin];
                keyPinLengths[pin] = Mathf.Clamp(keyPinLengths[pin], 0, maxKeyPinLength);
            }

            // Constrain shear line to valid positions
            shearLine = Mathf.Clamp(shearLine, MaxKeyPinHeight, ChamberHeight - MaxDriverPinHeight);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
