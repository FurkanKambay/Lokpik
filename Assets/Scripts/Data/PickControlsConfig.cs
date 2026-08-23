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
    }

    [CreateAssetMenu(menuName = "Pick/Controls Config")]
    internal sealed class PickControlsConfig : ScriptableObject, IPickControlsConfig
    {
        [Header("Tension Wrench Movement")]
        [SerializeField, Range(0, 1)] private float maxPickReachHeight = 0.5f;
        [SerializeField, Min(0)] private float chamberRetargetRate = 0.3f;

        [Header("Difficulty")]
        [SerializeField] private bool preventUnsettingPins;
        [SerializeField] private bool useTensionDrift;
        [SerializeField, Min(0)] private float tensionDrift = 0.008f;

        public float MaxPickReachHeight => maxPickReachHeight;
        public float ChamberRetargetRate => chamberRetargetRate;

        public bool PreventUnsettingPins => preventUnsettingPins;
        public bool UseTensionDrift => useTensionDrift;
        public float TensionDrift => tensionDrift;
    }
}
