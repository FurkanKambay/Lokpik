using System;
using UnityEditor;
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
        [SerializeField, Min(0)] float keyPinHeight;
        [SerializeField, Min(0)] float driverPinHeight;

        [Header("Debug")]
        [SerializeField, Range(0, 1)] float progress;
        [SerializeField] private bool isBinding;

        public float HoleHeight => holeHeight;
        public float HoleWidth => holeWidth;

        public float Progress
        {
            get => progress;
            set => progress = value;
        }

        public float KeyPinHeight
        {
            get => keyPinHeight;
            set => keyPinHeight = value;
        }

        private void Update()
        {
            keyPinRenderer.color = keyPinColor;
            driverPinRenderer.color = driverPinColor;
            backgroundRenderer.color = backgroundColor;

            backgroundRenderer.transform.localScale = new Vector3(holeWidth, holeHeight, 1);
            keyPinRenderer.transform.localScale = new Vector3(pinWidth, keyPinHeight, 1);
            driverPinRenderer.transform.localScale = new Vector3(pinWidth, driverPinHeight, 1);

            float keyPinY = (keyPinHeight / 2f) - (holeHeight / 2f) + (progress / 2f);
            float driverPinY = keyPinY + (keyPinHeight / 2f) + (driverPinHeight / 2f);

            if (isBinding)
            {
                float maxY = driverPinY - (driverPinHeight / 2f) - (keyPinHeight / 2f);
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
