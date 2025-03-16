using SaintsField;
using UnityEngine;

namespace Lokpik.Visuals
{
    [ExecuteAlways]
    public class PinVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer keyPinRenderer;
        [SerializeField] SpriteRenderer driverPinRenderer;
        [SerializeField] SpriteRenderer backgroundRenderer;

        [Header("Config")]
        [SerializeField] Color keyPinColor;
        [SerializeField] Color driverPinColor;
        [SerializeField] Color backgroundColor;

        [Header("Values")]
        [SerializeField, Min(0)] float holeWidth;
        [SerializeField, Min(0)] float holeHeight;
        [SerializeField, Min(0)] float pinWidth;
        [SerializeField, Min(0)] float keyPinLength;
        [SerializeField, Min(0)] float driverPinLength;

        [Header("Debug")]
        [ReadOnly, Range(0, 1)] float progress;
        [ReadOnly] private bool isBinding;

        public float HoleHeight => holeHeight;
        public float HoleWidth => holeWidth;

        public float Progress
        {
            get => progress;
            set => progress = value;
        }

        public float KeyPinHeight
        {
            get => keyPinLength;
            set => keyPinLength = value;
        }

        private void Update()
        {
            keyPinRenderer.color = keyPinColor;
            driverPinRenderer.color = driverPinColor;
            backgroundRenderer.color = backgroundColor;

            backgroundRenderer.transform.localScale = new Vector3(holeWidth, holeHeight, 1);
            keyPinRenderer.transform.localScale = new Vector3(pinWidth, keyPinLength, 1);
            driverPinRenderer.transform.localScale = new Vector3(pinWidth, driverPinLength, 1);

            float keyPinY = (keyPinLength / 2f) - (holeHeight / 2f) + (progress / 2f);
            float driverPinY = keyPinY + (keyPinLength / 2f) + (driverPinLength / 2f);

            if (isBinding)
            {
                float maxY = driverPinY - (driverPinLength / 2f) - (keyPinLength / 2f);
                keyPinY = Mathf.Min(keyPinY, maxY);
            }

            keyPinRenderer.transform.localPosition = new Vector3(0, keyPinY, 0);

            if (!isBinding)
            {
                driverPinRenderer.transform.localPosition = new Vector3(0, driverPinY, 0);
            }
        }
    }
}
