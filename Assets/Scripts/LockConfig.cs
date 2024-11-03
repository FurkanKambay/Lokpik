using System;
using System.Linq;
using UnityEngine;

namespace Lokpik
{
    [Serializable]
    public class LockConfig : ISerializationCallbackReceiver
    {
        [SerializeField, Range(0, 1)] float[] bindPoints = { 0.2f, 0.3f, 0.5f, 0.7f };

        public float[] BindPoints => bindPoints;
        public int PinCount => bindPoints.Length;
        public float LastBindPoint => bindPoints.Last();

        // /// <returns>The index of the binding pin at <paramref name="progress"/>. -1 if not found.</returns>
        // public int GetBindingPinAtPlugRotation(float progress)
        // {
        //     for (int i = 0; i < bindPoints.Length; i++)
        //     {
        //         if (bindPoints[i] > progress)
        //             return i;
        //     }
        //
        //     return -1;
        // }

        void ISerializationCallbackReceiver.OnBeforeSerialize() =>
            bindPoints = bindPoints.OrderBy(b => b).ToArray();

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
