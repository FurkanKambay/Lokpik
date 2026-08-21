using UnityEngine;

namespace Lokpik.Data
{
    public interface IPickControlsConfig
    {
    }

    [CreateAssetMenu(menuName = "Pick/Controls Config")]
    internal sealed class PickControlsConfig : ScriptableObject, IPickControlsConfig
    {
    }
}
