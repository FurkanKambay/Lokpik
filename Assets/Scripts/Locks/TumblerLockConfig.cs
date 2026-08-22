using System;
using System.Linq;
using UnityEngine;

namespace FK.Lokpik.Locks
{
    [Serializable]
    public class TumblerLockConfig : ISerializationCallbackReceiver
    {
        public const float ChamberHeight = 1f;

        [SerializeField, Range(0, 1)] float shearLine = 0.5f;
        [SerializeField] float tolerance = 0.04f;

        [Header("Pins")]
        [SerializeField, Min(0)] int pinCount = 2;
        [SerializeField] bool uniformDriverPins;

        [SerializeField, Range(0, 1)] float[] driverPinLengths = { 0.5f, 0.5f };
        [SerializeField, Range(0, 1)] float[] keyPinLengths = { 0.2f, 0.4f };
        [SerializeField, Range(0, 1)] float[] bindingRotations;

        // TODO: min binding separation angle (the less the more difficult + drift is more manageable)
        // + maybe an Unlock Angle to Remap 0-1 to 0-90 with bindings at angles 10, 15, 18, 23, 29 deg

        // [ReadOnly]
        [SerializeField] int[] bindingOrder;

        [Header("Info")]
        internal bool IsVulnerableToCombPicking =>
            Enumerable.Range(0, PinCount).All(pin => GetMaxLiftForPin(pin) >= ShearLine);

        public int PinCount => pinCount;
        public int LastPinIndex => pinCount - 1;

        public float ShearLine => shearLine;
        public float Tolerance => tolerance;

        public float[] DriverPinLengths => driverPinLengths;
        public float[] KeyPinLengths => keyPinLengths;
        public float[] BindingRotations => bindingRotations;
        public int[] BindingOrder => bindingOrder;

        internal float MaxKeyPinHeight => keyPinLengths.Max();
        internal float MaxDriverPinHeight => driverPinLengths.Max();

        /// <summary>
        /// Find the next binding pin at <paramref name="plugRotation"/>. -1 if all pins are set.
        /// </summary>
        public int FindNextPinAt(float plugRotation)
        {
            for (int i = 0; i < PinCount; i++)
            {
                int pin = BindingOrder[i];

                if (plugRotation <= BindingRotations[pin])
                    return pin;
            }

            return -1;
        }

        /// <summary>
        /// Find the last set pin at <paramref name="plugRotation"/>. -1 if none are set.
        /// </summary>
        public int FindPreviousPinAt(float plugRotation)
        {
            for (int i = PinCount - 1; i >= 0; i--)
            {
                int pin = BindingOrder[i];

                if (plugRotation > BindingRotations[pin])
                    return pin;
            }

            return -1;
        }

        public int ClampPinIndex(int pin) => Math.Clamp(pin, 0, LastPinIndex);

        /// <summary>
        /// Get the plug rotation at which the <paramref name="pin"/> can stay <see cref="ChamberState.Set"/>,
        /// or starts binding.
        /// </summary>
        internal float GetAdequatePlugRotation(int pin)
        {
            if (pin < 0 || pin >= PinCount)
                return -1;

            return BindingRotations[pin];
        }

        internal float GetMaxLiftForPin(int pin)
        {
            if (pin < 0 || pin >= PinCount)
                return 1;

            float driverPinLength = DriverPinLengths[pin];
            float keyPinLength = KeyPinLengths[pin];
            return ChamberHeight - (driverPinLength + keyPinLength);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            Array.Resize(ref driverPinLengths, PinCount);
            Array.Resize(ref keyPinLengths, PinCount);
            Array.Resize(ref bindingRotations, PinCount);

            // if (BindingRotations.Distinct().Count() != BindingRotations.Length)
            //     EditorGUILayout.HelpBox("Some pins bind together", MessageType.Error, true);

            // Binding order
            if (BindingRotations != null)
            {
                bindingOrder = BindingRotations
                    .Select((rotation, index) => (rotation, index))
                    .OrderBy(t => t.rotation)
                    .Select(t => t.index)
                    .ToArray();
            }

            // Uniform driver pins
            if (uniformDriverPins && driverPinLengths.Any())
                Array.Fill(driverPinLengths, driverPinLengths.First());

            // Make sure key pins fit in chambers
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
