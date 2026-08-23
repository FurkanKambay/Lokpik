using FK.Common;
using FK.Lokpik.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FK.Lokpik
{
    internal sealed class WorkshopManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Lockpicker lockpicker;
        [SerializeField] private RumbleService rumbleService;
        [SerializeField] private PickInputAsset input;
        [SerializeField] private DebugPickInputAsset debugInput;

        [Header("Config")]
        [SerializeField] private bool useDebugInput;

        private void Start()
        {
            InputSystem.actions.Enable();
            // Cursor.lockState = CursorLockMode.Locked;

            InjectDependencies();
        }

        private void InjectDependencies()
        {
            lockpicker.Init(useDebugInput ? debugInput : input);
            lockpicker.Init(rumbleService);
        }

        private void OnValidate() => InjectDependencies();
    }
}
