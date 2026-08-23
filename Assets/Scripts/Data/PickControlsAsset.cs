using FK.Common;
using UnityEngine;

namespace FK.Lokpik.Data
{
    public interface IPickControlsConfig
    {
        float MaxPickReachHeight { get; }
        float ChamberRetargetRate { get; }

        bool PreventUnsettingPins { get; }
        bool UseTensionDrift { get; }
        float TensionDrift { get; }

        float RumbleDuration { get; }
        RumbleProfile PinSetRumble { get; }
        RumbleProfile PinFreedRumble { get; }
        RumbleProfile PinBindRumble { get; }
    }

    [CreateAssetMenu(menuName = "Pick/Controls Config")]
    internal sealed class PickControlsAsset : ScriptableObject, IPickControlsConfig
    {
        [Header("Tension Wrench Movement")]
        [SerializeField, Range(0, 1)] private float maxPickReachHeight = 0.5f;
        [SerializeField, Min(0)] private float chamberRetargetRate = 0.3f;

        [Header("Difficulty")]
        [SerializeField] private bool preventUnsettingPins;
        [SerializeField] private bool useTensionDrift;
        [SerializeField, Min(0)] private float tensionDrift = 0.008f;

        [Header("Rumble Feedback")]
        [SerializeField, Min(0)] private float rumbleDuration = 0.1f;
        [SerializeField] private RumbleProfile pinSetRumble;
        [SerializeField] private RumbleProfile pinFreedRumble;
        [SerializeField] private RumbleProfile pinBindRumble;

        public float MaxPickReachHeight => maxPickReachHeight;
        public float ChamberRetargetRate => chamberRetargetRate;

        public bool PreventUnsettingPins => preventUnsettingPins;
        public bool UseTensionDrift => useTensionDrift;
        public float TensionDrift => tensionDrift;

        public float RumbleDuration => rumbleDuration;
        public RumbleProfile PinSetRumble => pinSetRumble;
        public RumbleProfile PinFreedRumble => pinFreedRumble;
        public RumbleProfile PinBindRumble => pinBindRumble;
    }
}
