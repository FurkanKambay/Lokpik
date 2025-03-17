namespace Lokpik.TumblerLock
{
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

    public static class ChamberStateExtensions
    {
        public static bool IsPicked(this ChamberState state) =>
            state is ChamberState.Set or ChamberState.AboveShearLine;

        public static bool IsBlocking(this ChamberState state) =>
            !state.IsPicked();

        public static bool IsBinding(this ChamberState state) =>
            state is ChamberState.UndersetBinding or ChamberState.OversetBinding;
    }
}
