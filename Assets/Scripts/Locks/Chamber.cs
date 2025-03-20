using System;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Assertions;

namespace Lokpik.Locks
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

        [ShowInInspector] public int Tension => tension;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        public float KeyPinLength => Lock.Config.KeyPinLengths[chamberIndex];
        public float DriverPinLength => Lock.Config.DriverPinLengths[chamberIndex];
        public bool IsBinding => State.IsBinding();
        public bool IsPicked => State.IsPicked();
        public bool IsFree => State.IsFree();

        internal float MaxLift => TumblerLockConfig.ChamberHeight - DriverPinLength - KeyPinLength;

        [field: NonSerialized]
        public TumblerLock Lock { get; private set; }

        private int chamberIndex;
        private ChamberState state;
        private float driverPinLift;
        private float keyPinLift;
        private int tension = -1;

        // TODO: RESTRICT ACCESS TO ONLY TumblerLock
        public void SetTension(int value)
        {
            tension = value;
            UpdateState();
        }

        public void Lift(float delta)
        {
            // Binding resolutions
            if (state is ChamberState.Underset)
            {
                float maxKeyPinLift = DriverPinLift - KeyPinLength;
                keyPinLift = Mathf.Clamp(KeyPinLift + delta, 0, maxKeyPinLift);
                // The driver pin can't be moved until counter-rotation is applied.
                // Same logic as below, but in this case, the key pin will still move.
                // The key pin needs to touch the driver pin before applying heavy pick force.
            }
            else if (state is ChamberState.Overset)
            {
                // Both pins are stuck until counter-rotation is applied,
                // or heavy pick force is used to apply counter-rotation.
                // (Repeatedly pressing `W` decreases the Torque applied with `Space`.)
            }
            else if (state is ChamberState.AboveShearLine)
            {
                keyPinLift = Lock.Config.ShearLine;
                driverPinLift = keyPinLift + KeyPinLength;
            }
            else if (state is ChamberState.Set)
            {
                driverPinLift = Lock.Config.ShearLine;
                float maxKeyLift = DriverPinLift - KeyPinLength;
                keyPinLift = Mathf.Clamp(KeyPinLift + delta, 0, maxKeyLift);
            }
            else
            {
                // nothing is binding so lift both pins
                keyPinLift = Mathf.Clamp(KeyPinLift + delta, 0, MaxLift);
                driverPinLift = KeyPinLength + KeyPinLift;
            }

            UpdateState();
        }

        private void UpdateState()
        {
            float shearLine = Lock.Config.ShearLine;
            bool isPerfect = Math.Abs(DriverPinLift - shearLine) < Lock.Config.Tolerance;
            bool isExploited = KeyPinLift >= shearLine;
            bool isAbove = DriverPinLift >= shearLine;
            bool isUnder = DriverPinLift < shearLine;

            state = Tension switch
            {
                // Adequate tension
                0 or 1 when isPerfect => ChamberState.Set,
                0 or 1 when isExploited => ChamberState.AboveShearLine,

                // High tension: binding
                1 when isAbove => ChamberState.Overset,
                1 when isUnder => ChamberState.Underset,

                // Low tension: blocking, but not binding
                _ => ChamberState.Free
            };
        }

        public void ResetLift()
        {
            switch (State)
            {
                case ChamberState.Free:
                default:
                    keyPinLift = 0;
                    driverPinLift = KeyPinLength;
                    return;
                case ChamberState.Underset:
                case ChamberState.Set:
                    keyPinLift = 0;
                    return;
                case ChamberState.Overset:
                case ChamberState.AboveShearLine:
                    return;
            }
        }

        internal void SetLock(TumblerLock value, int index)
        {
            Assert.IsTrue(index >= 0);

            Lock = value;
            chamberIndex = index;

            Assert.IsTrue(index < Lock.Config.PinCount);
        }
    }
}
