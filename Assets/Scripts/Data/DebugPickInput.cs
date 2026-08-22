using UnityEngine;

namespace FK.Lokpik.Data
{
    [CreateAssetMenu(menuName = "Pick/Debug Input")]
    internal sealed class DebugPickInput : ScriptableObject, IPickInput
    {
        [SerializeField, Range(0, 1)] private float tensionInput;
        [SerializeField, Range(0, 1)] private float pickHeightInput;
        [SerializeField, Range(-1, 1)] private int changePinInput;

        public float Tension => tensionInput;
        public float PickHeight => pickHeightInput;
        public int PickMoveDelta => changePinInput;
    }
}
