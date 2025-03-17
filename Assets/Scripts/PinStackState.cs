namespace Lokpik
{
    public enum PinStackState
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
        AboveShearLine
    }
}
