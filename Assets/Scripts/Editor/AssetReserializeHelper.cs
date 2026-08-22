using UnityEditor;
using UnityEngine;

namespace Lokpik.Editor
{
    public sealed class AssetReserializeHelper : MonoBehaviour
    {
        [MenuItem("Tools/Force Reserialize Assets")]
        private static void ForceReserializeAssets() =>
            AssetDatabase.ForceReserializeAssets();
    }
}
