using System;
using UnityEngine;
using UnityEngine.Assertions;
using Vertx.Attributes;

namespace FK.Lokpik.Locks
{
    using RO = ReadOnlyFieldAttribute;

    /// <summary>
    /// The state of a pin stack chamber.
    /// </summary>
    [Serializable]
    public class Chamber
    {
        public delegate void ChamberStateChangeEvent(Chamber chamber, ChamberState oldState, ChamberState newState);

        public event ChamberStateChangeEvent OnStateChange;

        public ChamberState State => state;

        /// <summary>
        /// The lift amount of the driver pin from the resting line.
        /// </summary>
        public float DriverPinLift => driverPinLift;

        /// <summary>
        /// The lift amount of the key pin from the resting line.
        /// </summary>
        public float KeyPinLift => keyPinLift;

        public int Tension => tension;

        public int ChamberIndex => chamberIndex;
        public float KeyPinLength => Lock.Config.GetKeyPinLength(chamberIndex);
        public float DriverPinLength => Lock.Config.GetDriverPinLength(chamberIndex);
        public bool IsBinding => state.IsBinding();
        public bool IsPicked => state.IsPicked();
        public bool IsFree => state.IsFree();

        internal float MaxLift => TumblerLockConfig.ChamberHeight - DriverPinLength - KeyPinLength;

        [field: NonSerialized]
        public TumblerLock Lock { get; private set; }

        [SerializeField, RO] private int chamberIndex;
        [SerializeField, RO] private ChamberState state;
        [SerializeField, RO] private float driverPinLift;
        [SerializeField, RO] private float keyPinLift;
        [SerializeField, RO] private int tension = -1;

        // TODO: should only be called by TumblerLock
        public void SetTension(int value)
        {
            tension = value;
            LiftTowards(keyPinLift);
        }

        public void LiftTowards(float desiredTarget)
        {
            switch (state)
            {
                case ChamberState.Underset:
                {
                    float maxKeyPinLift = driverPinLift - KeyPinLength;
                    keyPinLift = Mathf.Clamp(desiredTarget, 0, maxKeyPinLift);
                    // The driver pin can't be moved until counter-rotation is applied.
                    // Same logic as below, but in this case, the key pin will still move.
                    // The key pin needs to touch the driver pin before applying heavy pick force.
                    break;
                }
                case ChamberState.Overset:
                    // Both pins are stuck until counter-rotation is applied,
                    // or heavy pick force is used to apply counter-rotation.
                    // (Repeatedly pressing `W` decreases the Torque applied with `Space`.)
                    break;
                case ChamberState.AboveShearLine:
                    keyPinLift = Lock.Config.ShearLine;
                    driverPinLift = keyPinLift + KeyPinLength;
                    break;
                case ChamberState.Set:
                {
                    driverPinLift = Lock.Config.ShearLine;
                    float maxKeyLift = driverPinLift - KeyPinLength;
                    keyPinLift = Mathf.Clamp(desiredTarget, 0, maxKeyLift);
                    break;
                }
                case ChamberState.Free:
                default:
                    // nothing is binding so lift both pins
                    keyPinLift = Mathf.Clamp(desiredTarget, 0, MaxLift);
                    driverPinLift = KeyPinLength + keyPinLift;
                    break;
            }

            UpdateState();
        }

        private void UpdateState()
        {
            float shearLine = Lock.Config.ShearLine;
            bool isPerfect = Math.Abs(driverPinLift - shearLine) < Lock.Config.Tolerance;
            bool isExploited = keyPinLift >= shearLine;
            bool isAbove = driverPinLift >= shearLine;
            bool isUnder = driverPinLift < shearLine;

            ChamberState oldState = state;
            state = tension switch
            {
                // Adequate tension
                1 when isPerfect => ChamberState.Set,
                1 when isExploited => ChamberState.AboveShearLine,

                // Excessive tension: binding
                1 when isAbove => ChamberState.Overset,
                1 when isUnder => ChamberState.Underset,

                // Low tension: blocking, but NOT binding
                _ => ChamberState.Free
            };

            if (state != oldState)
                OnStateChange?.Invoke(this, oldState, state);
        }

        public void StopLifting()
        {
            switch (state)
            {
                case ChamberState.Free:
                default:
                    // drop both pins back down
                    keyPinLift = 0;
                    driverPinLift = KeyPinLength;
                    return;

                case ChamberState.Underset:
                case ChamberState.Set:
                    // driver pin is binding, so only drop key pin
                    keyPinLift = 0;
                    return;

                case ChamberState.Overset:
                case ChamberState.AboveShearLine:
                    // both pins are stuck at/above shear line
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
