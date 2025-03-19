namespace Lokpik.Locks
{
    public enum ChamberState
    {
        /// <summary>
        /// Both pins can be moved freely.
        /// </summary>
        Free,
        /// <summary>
        /// The driver pin is blocking the shear line, and is binding. The key pin is free.
        /// </summary>
        Underset,
        /// <summary>
        /// The shear line is unobstructed.
        /// </summary>
        Set,
        /// <summary>
        /// The key pin is blocking the shear line, and is binding. The driver pin is unreachable.
        /// </summary>
        Overset,
        /// <summary>
        /// Both pins are above the shear line.
        /// </summary>
        AboveShearLine
    }

    public static class ChamberStateExtensions
    {
        public static bool IsPicked(this ChamberState state) =>
            state is ChamberState.Set or ChamberState.AboveShearLine;

        public static bool IsBinding(this ChamberState state) =>
            state is ChamberState.Underset or ChamberState.Overset;

        public static bool IsFree(this ChamberState state) =>
            state is ChamberState.Free;
    }
}
