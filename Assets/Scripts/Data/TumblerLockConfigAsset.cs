using Lokpik.Locks;
using UnityEngine;
using Vertx.Attributes;

namespace Lokpik.Data
{
    [CreateAssetMenu(menuName = "Lock/Tumbler Lock Config")]
    public class TumblerLockConfigAsset : ScriptableObject
    {
        [SerializeField, Inline] private TumblerLockConfig lockConfig;

        public TumblerLockConfig LockConfig => lockConfig;
    }
}
