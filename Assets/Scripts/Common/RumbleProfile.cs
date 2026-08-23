using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FK.Common
{
    [Serializable]
    public struct RumbleProfile
    {
        [Range(0, 1)] public float lowFrequency;
        [Range(0, 1)] public float highFrequency;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RumbleProfile operator *(RumbleProfile profile, float scalar)
        {
            return new RumbleProfile
            {
                lowFrequency = profile.lowFrequency * scalar,
                highFrequency = profile.highFrequency * scalar
            };
        }
    }
}
