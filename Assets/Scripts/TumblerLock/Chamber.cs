using System;
using NUnit.Framework;
using SaintsField.Playa;
using UnityEngine;

namespace Lokpik.TumblerLock
{
    /// <summary>
    /// The state of a pin stack chamber.
    /// </summary>
    [Serializable]
    public class Chamber
    {
        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        [ShowInInspector] public ChamberState State => state;

        /// <summary>
        /// The lift amount from the top of the key pin.
        /// </summary>
        [ShowInInspector] public float DriverPinLift => driverPinLift;

        /// <summary>
        /// The lift amount from the resting line.
        /// </summary>
        [ShowInInspector] public float KeyPinLift => keyPinLift;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        public float KeyPinLength => Lock.Config.KeyPinLengths[chamberIndex];
        public float DriverPinLength => Lock.Config.DriverPinLengths[chamberIndex];

        internal float MaxLift => TumblerLockConfig.ChamberHeight - DriverPinLength - KeyPinLength;

        [field: NonSerialized]
        public TumblerLock Lock { get; private set; }

        private int chamberIndex;
        private ChamberState state;
        private float driverPinLift;
        private float keyPinLift;

        public void SetState(ChamberState value) => state = value;

        public void LiftPin(float delta)
        {
            // TODO: implement isBinding
            bool isBinding = false;
            float oldKeyPinLift = KeyPinLift;

            // TODO: don't do this yet if it's binding
            keyPinLift = Mathf.Clamp(keyPinLift + delta, 0, MaxLift);

            float correctlySetLift = Lock.Config.ShearLine - KeyPinLength;

            // Set state
            if (Math.Abs(KeyPinLift - correctlySetLift) < Lock.Config.Tolerance)
                state = ChamberState.Set;
            else if (KeyPinLift >= Lock.Config.ShearLine)
                state = ChamberState.AboveShearLine;
            else if (KeyPinLift > correctlySetLift && isBinding)
                state = ChamberState.OversetBinding;
            else if (KeyPinLift > correctlySetLift)
                state = ChamberState.Overset;
            else if (KeyPinLift < correctlySetLift && isBinding)
                state = ChamberState.UndersetBinding;
            else if (KeyPinLift < correctlySetLift)
                state = ChamberState.Underset;

            // Binding resolutions
            if (state is ChamberState.UndersetBinding)
            {
                float maxKeyLift = DriverPinLift - KeyPinLength;
                keyPinLift = Mathf.Min(KeyPinLift, maxKeyLift);

                // TODO: driver binding - driver can't be lifted, but key can
            }
            else if (state is ChamberState.OversetBinding)
            {
                // TODO: key binding - can't lift at all
                keyPinLift = oldKeyPinLift;
            }
            else
            {
                // nothing is binding so lift both pins
                driverPinLift = KeyPinLift;
            }
        }

        public void Reset()
        {
            // BUG: what if it's binding even after being reset?
            state = ChamberState.Underset;
            keyPinLift = 0;
            driverPinLift = 0;
        }

        internal void SetLock(TumblerLock value, int index)
        {
            Assert.GreaterOrEqual(index, 0);

            Lock = value;
            chamberIndex = index;

            Assert.Less(index, Lock.Config.PinCount);
        }
    }
}
