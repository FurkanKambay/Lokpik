using UnityEngine;

namespace FK.Lokpik.Data
{
    public interface IPickControlsConfig
    {
        float MaxHeightReach { get; }
        float ChamberRetargetRate { get; }
        bool PreventUnsettingPins { get; }
        bool UseTensionDrift { get; }
        float TensionDrift { get; }
    }

    [CreateAssetMenu(menuName = "Pick/Controls Config")]
    internal sealed class PickControlsConfig : ScriptableObject, IPickControlsConfig
    {
        [Header("Tension Wrench Movement")]
        [SerializeField, Range(0, 1)] private float maxHeightReach = 0.5f;
        [SerializeField, Min(0)] private float chamberRetargetRate = 0.3f;

        [Header("Artificial Skill Hindrance")]
        [SerializeField] private bool preventUnsettingPins;
        [SerializeField] private bool useTensionDrift;
        [SerializeField, Min(0)] private float tensionDrift = 0.008f;

        public float MaxHeightReach => maxHeightReach;
        public float ChamberRetargetRate => chamberRetargetRate;

        public bool PreventUnsettingPins => preventUnsettingPins;
        public bool UseTensionDrift => useTensionDrift;
        public float TensionDrift => tensionDrift;
    }
}
