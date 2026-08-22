using System.Threading;
using FK.Common.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FK.Common
{
    public class RumbleService : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float zeroThreshold = 0.001f;
        [SerializeField] private float defaultWaveDecay = 5f;

        [Header("State")]
        [SerializeField, Range(0, 1)] private float lowFrequency;
        [SerializeField, Range(0, 1)] private float highFrequency;

        public async Awaitable SendWaveLowFull() => await SendWaveLow(1f, destroyCancellationToken);
        public async Awaitable SendWaveHighFull() => await SendWaveHigh(1f, destroyCancellationToken);

        public async Awaitable SendWaveLow(float initialValue, CancellationToken token = default) =>
            await SendWaveLow(initialValue, defaultWaveDecay, token);

        public async Awaitable SendWaveHigh(float initialValue, CancellationToken token = default) =>
            await SendWaveHigh(initialValue, defaultWaveDecay, token);

        public async Awaitable SendWaveLow(float initialValue, float decay, CancellationToken token = default)
        {
            if (initialValue > zeroThreshold)
            {
                SetLowFrequency(initialValue);

                while (lowFrequency > zeroThreshold)
                {
                    await Awaitable.NextFrameAsync(token);
                    SetLowFrequency(lowFrequency.ExpDecay(0f, decay, Time.deltaTime));
                }
            }

            SetLowFrequency(0);
        }

        public async Awaitable SendWaveHigh(float initialValue, float decay, CancellationToken token = default)
        {
            if (initialValue > zeroThreshold)
            {
                SetHighFrequency(initialValue);

                while (highFrequency > zeroThreshold)
                {
                    await Awaitable.NextFrameAsync(token);
                    SetHighFrequency(highFrequency.ExpDecay(0f, decay, Time.deltaTime));
                }
            }

            SetHighFrequency(0);
        }

        public void SetLowFrequencySolo(float frequency) => SetFrequencies(frequency, 0f);
        public void SetHighFrequencySolo(float frequency) => SetFrequencies(0f, frequency);

        public void SetLowFrequency(float frequency) => SetFrequencies(frequency, highFrequency);
        public void SetHighFrequency(float frequency) => SetFrequencies(lowFrequency, frequency);

        public void StopRumble() => SetFrequencies(0f, 0f);

        internal bool SetFrequencies(float lowFrequency, float highFrequency)
        {
            if (Gamepad.current is not { } gamepad)
                return false;

            this.lowFrequency = lowFrequency;
            this.highFrequency = highFrequency;

            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            return true;
        }

#if UNITY_EDITOR
        [ContextMenu("Send Wave: Low")]
        public async Awaitable ContextItem_SendWaveLow()
        {
            // wait a frame for context menu to close and stop blocking
            await Awaitable.NextFrameAsync();

            await SendWaveLowFull();
        }

        [ContextMenu("Send Wave: High")]
        public async Awaitable ContextItem_SendWaveHigh()
        {
            // wait a frame for context menu to close and stop blocking
            await Awaitable.NextFrameAsync();

            await SendWaveHighFull();
        }
#endif
    }
}
