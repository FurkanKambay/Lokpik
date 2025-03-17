using System;
using SaintsField.Playa;

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
        [ShowInInspector] public float KeyPinLift => keyPinLift;
        [ShowInInspector] public float DriverPinLift => driverPinLift;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        private ChamberState state;
        private float keyPinLift;
        private float driverPinLift;
    }

    public enum ChamberState
    {
        /// <summary>
        /// The driver pin is blocking the shear line.
        /// </summary>
        Underset,
        /// <summary>
        /// The shear line is unobstructed.
        /// </summary>
        Set,
        /// <summary>
        /// The key pin is blocking the shear line.
        /// </summary>
        Overset,
        /// <summary>
        /// The pins are above the shear line.
        /// </summary>
        AboveShearLine,
        /// <summary>
        /// The driver pin is blocking the shear line, and the pin is binding.
        /// </summary>
        UndersetBinding,
        /// <summary>
        /// The key pin is blocking the shear line, and the pin is binding.
        /// </summary>
        OversetBinding
    }
}
