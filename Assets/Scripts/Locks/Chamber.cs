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
        public bool IsBinding => state.IsBinding();
        public bool IsPicked => state.IsPicked();
        public bool IsBlocking => state.IsBlocking();

        internal float MaxLift => TumblerLockConfig.ChamberHeight - DriverPinLength - KeyPinLength;

        [field: NonSerialized]
        public TumblerLock Lock { get; private set; }

        private int chamberIndex;
        private ChamberState state;
        private float driverPinLift;
        private float keyPinLift;
        private int tension;

        public void SetState(ChamberState value) => state = value;

        public void SetTension(int value)
        {
            tension = value;

            if (Tension > 0)
                Bind();
            else if (Tension < 0)
            {
                // we could be manipulating the pin right now
                // so can't just reset
            }
        }

        public void LiftPin(float delta)
        {
            // Binding resolutions
            if (state is ChamberState.UndersetBinding)
            {
                float maxKeyLift = DriverPinLift;
                keyPinLift = Mathf.Min(KeyPinLift, maxKeyLift);
                // The driver pin can't be moved until counter-rotation is applied.
                // Same logic as below, but in this case, the key pin will still move.
                // The key pin needs to touch the driver pin before applying heavy pick force.
                // return;
            }
            else if (state is ChamberState.OversetBinding)
            {
                // Both pins are stuck until counter-rotation is applied,
                // Or more heavy pick force is used to force a counter-rotation.
                // (Repeatedly pressing `W` decreases the Torque applied with `Space`.)
                // return;
            }
            else
            {
                // nothing is binding so lift both pins
                driverPinLift = KeyPinLift;
                keyPinLift = Mathf.Clamp(keyPinLift + delta, 0, MaxLift);

                float perfectLift = Lock.Config.ShearLine - KeyPinLength;

                bool isSet = Math.Abs(KeyPinLift - perfectLift) < Lock.Config.Tolerance;
                bool isAboveShearLine = KeyPinLift >= Lock.Config.ShearLine;
                bool isOverset = KeyPinLift > perfectLift;
                bool isUnderset = KeyPinLift < perfectLift;

                state = Tension switch
                {
                    > 0 when isOverset => ChamberState.OversetBinding,
                    > 0 when isUnderset => ChamberState.UndersetBinding,
                    _ when isSet => ChamberState.Set,
                    _ when isAboveShearLine => ChamberState.AboveShearLine,
                    _ when isOverset => ChamberState.Overset,
                    _ when isUnderset => ChamberState.Underset,
                    _ => default
                };
            }
        }

        public void ResetLift()
        {
            switch (State)
            {
                case ChamberState.OversetBinding: return;
                case ChamberState.UndersetBinding:
                    keyPinLift = 0;
                    return;
                case ChamberState.Underset:
                case ChamberState.Set:
                case ChamberState.Overset:
                case ChamberState.AboveShearLine:
                default:
                    // TODO: shouldn't drop when the pin is picked
                    state = ChamberState.Underset;
                    keyPinLift = 0;
                    driverPinLift = 0;
                    return;
            }
        }

        // TODO: maybe this shouldn't be called externally
        // but instead an "overtension" flag could be set, then this logic becomes internal?
        public void Bind()
        {
            state = State switch
            {
                ChamberState.Underset => ChamberState.UndersetBinding,
                ChamberState.Overset => ChamberState.OversetBinding,
                _ => State
            };
        }

        public void Unbind()
        {
            state = State switch
            {
                ChamberState.UndersetBinding => ChamberState.Underset,
                ChamberState.OversetBinding => ChamberState.Overset,
                _ => State
            };

            // TODO: what happens to PinLifts when unbinded
            // while pin being manipulated vs when it's not
            if (State is ChamberState.UndersetBinding)
            {
            }
            else if (State is ChamberState.OversetBinding)
            {
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
