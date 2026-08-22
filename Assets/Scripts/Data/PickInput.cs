using UnityEngine;
using UnityEngine.InputSystem;
using Vertx.Attributes;

namespace FK.Lokpik.Data
{
    using RO = ReadOnlyFieldAttribute;

    public interface IPickInput
    {
        float Tension { get; }
        float PickHeight { get; }
        int PickMoveDelta { get; }
    }

    [CreateAssetMenu(menuName = "Pick/Input")]
    internal sealed class PickInput : ScriptableObject, IPickInput
    {
        [SerializeField] private InputActionReference tensionInput;
        [SerializeField] private InputActionReference pickHeightInput;
        [SerializeField] private InputActionReference changePinInput;
        // TODO: a "lift pick hard" button/repeated press to apply counter-rotation for unbinding a pin

        public float Tension => tensionValue = tensionInput.action.ReadValue<float>();
        public float PickHeight => pickHeightValue = pickHeightInput.action.ReadValue<float>();
        public int PickMoveDelta => changePinValue = (int)changePinInput.action.ReadValue<float>();

        [Header("Debug")]
        [SerializeField, RO, Range(0, 1)] private float tensionValue;
        [SerializeField, RO, Range(0, 1)] private float pickHeightValue;
        [SerializeField, RO, Range(-1, 1)] private int changePinValue;
    }
}
