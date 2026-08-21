using Lokpik.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lokpik
{
    internal sealed class WorkshopManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Lockpicker lockpicker;
        [SerializeField] private PickInput input;
        [SerializeField] private DebugPickInput debugInput;

        [Header("Config")]
        [SerializeField] private bool useDebugInput;

        private void Start()
        {
            InputSystem.actions.Enable();
            // Cursor.lockState = CursorLockMode.Locked;

            lockpicker.Init(input);
        }

        private void OnValidate()
        {
            lockpicker.Init(useDebugInput ? debugInput : input);
        }
    }
}
