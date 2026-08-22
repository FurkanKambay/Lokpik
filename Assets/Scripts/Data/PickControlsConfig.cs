using UnityEngine;

namespace Lokpik.Data
{
    public interface IPickControlsConfig
    {
        float ChamberRetargetRate { get; }
        bool UseTensionDrift { get; }
        float TensionDrift { get; }
        float TurnSpeed { get; }
        float PlugGravity { get; }
    }

    [CreateAssetMenu(menuName = "Pick/Controls Config")]
    internal sealed class PickControlsConfig : ScriptableObject, IPickControlsConfig
    {
        [Header("Chamber Switching")]
        [SerializeField, Min(0)] private float chamberRetargetRate = 0.3f;
        // TODO: dynamic pick position with chamber walls

        [Header("Artificial Skill Hindrance")]
        [SerializeField] private bool useTensionDrift;
        [SerializeField, Min(0)] private float tensionDrift = 0.008f;

        [Header("Plug Rotation")]
        [SerializeField, Min(0)] private float turnSpeed = 1f;
        [SerializeField, Min(0)] private float plugGravity = 5f;

        public float ChamberRetargetRate => chamberRetargetRate;
        public bool UseTensionDrift => useTensionDrift;
        public float TensionDrift => tensionDrift;

        public float TurnSpeed => turnSpeed;
        public float PlugGravity => plugGravity;
    }
}
