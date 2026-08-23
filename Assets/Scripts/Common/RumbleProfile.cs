using System;
using UnityEngine;

namespace FK.Common
{
    [Serializable]
    public struct RumbleProfile
    {
        [Range(0, 1)] public float lowFrequency;
        [Range(0, 1)] public float highFrequency;
    }
}
