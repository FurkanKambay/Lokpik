using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vertx.Attributes;

namespace FK.Lokpik.Locks
{
    [Serializable]
    public struct PinData
    {
        [ReadOnlyField] public int chamberIndex;
        [Range(0.1f, 0.5f)] public float pinLengthDriver;
        [Range(0.1f, 0.5f)] public float pinLengthKey;

        [Range(0.1f, 0.9f)] public float bindRotation;
        // [Min(0)] public int bindIndex; // replace with bindAngle?
    }

    [Serializable]
    public class TumblerLockConfig : ISerializationCallbackReceiver
    {
        public const float ChamberHeight = 1f;

        [SerializeField, Range(0, 1)] float shearLine = 0.5f;
        [SerializeField] float tolerance = 0.04f;

        [Header("Pins")]
        [SerializeField, Min(0)] int pinCount = 5;
        [SerializeField] bool uniformDriverPins;
        [SerializeField, Inline] private PinData[] pins = new PinData[1];

        // TODO: min binding separation angle (the less, the more difficult + drift is less manageable)
        // + maybe an Unlock Angle to Remap 0-1 to 0-90, e.g. with bindings at angles 10, 15, 18, 23, 29 deg

        [Header("Debug")]
        [SerializeField, ReadOnlyField] int[] bindingOrder;

        internal bool IsVulnerableToCombPicking =>
            Enumerable.Range(0, PinCount).All(pin => GetMaxLiftForPin(pin) >= ShearLine);

        public int PinCount => pinCount;
        public int LastPinIndex => pinCount - 1;

        public float ShearLine => shearLine;
        public float Tolerance => tolerance;

        internal float MaxKeyPinHeight => pins.Max(p => p.pinLengthKey);
        internal float MaxDriverPinHeight => pins.Max(p => p.pinLengthDriver);

        public float GetKeyPinLength(int pin) => pin < 0 || pin >= PinCount ? -1 : pins[pin].pinLengthKey;
        public float GetDriverPinLength(int pin) => pin < 0 || pin >= PinCount ? -1 : pins[pin].pinLengthDriver;

        /// <summary>
        /// Find the next binding pin at <paramref name="plugRotation"/>. -1 if all pins are set.
        /// </summary>
        public int FindNextPinAt(float plugRotation)
        {
            for (int i = 0; i < bindingOrder.Length; i++)
            {
                int chamberIndex = bindingOrder[i];

                if (plugRotation <= pins[chamberIndex].bindRotation)
                    return chamberIndex;
            }

            return -1;
        }

        /// <summary>
        /// Find the last set pin at <paramref name="plugRotation"/>. -1 if none are set.
        /// </summary>
        public int FindPreviousPinAt(float plugRotation)
        {
            for (int i = bindingOrder.Length - 1; i >= 0; i--)
            {
                int chamberIndex = bindingOrder[i];

                if (plugRotation > pins[chamberIndex].bindRotation)
                    return chamberIndex;
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

            return pins[pin].bindRotation;
        }

        internal float GetMaxLiftForPin(int pin)
        {
            if (pin < 0 || pin >= PinCount)
                return 1;

            float driverPinLength = pins[pin].pinLengthDriver;
            float keyPinLength = pins[pin].pinLengthKey;
            return ChamberHeight - (driverPinLength + keyPinLength);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            Array.Resize(ref pins, PinCount);
            for (int i = 0; i < pins.Length; i++)
                pins[i].chamberIndex = i;

            // if (BindingRotations.Distinct().Count() != BindingRotations.Length)
            //     EditorGUILayout.HelpBox("Some pins bind together", MessageType.Error, true);

            // Binding order
            if (pins != null)
            {
                bindingOrder = pins
                    .OrderBy(pin => pin.bindRotation)
                    .Select(pin => pin.chamberIndex)
                    .ToArray();
            }

            // Uniform driver pins
            if (uniformDriverPins && pins.Length > 1)
            {
                float uniformLength = pins[0].pinLengthDriver;
                for (int i = 1; i < pins.Length; i++)
                    pins[i].pinLengthDriver = uniformLength;
            }

            // Make sure key pins fit in chambers
            for (int pin = 0; pin < PinCount; pin++)
            {
                float maxKeyPinLength = ChamberHeight - pins[pin].pinLengthDriver;
                pins[pin].pinLengthKey = Mathf.Clamp(pins[pin].pinLengthKey, 0, maxKeyPinLength);
            }

            // Constrain shear line to valid positions
            shearLine = Mathf.Clamp(shearLine, MaxKeyPinHeight, ChamberHeight - MaxDriverPinHeight);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
