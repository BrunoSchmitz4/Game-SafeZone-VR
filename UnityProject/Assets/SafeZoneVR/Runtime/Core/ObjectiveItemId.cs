using UnityEngine;

namespace SafeZoneVR
{
    /// <summary>
    /// Marker placed on a grabbable/socketable object so validators can identify which
    /// objective item was placed or picked up.
    /// </summary>
    public class ObjectiveItemId : MonoBehaviour
    {
        [SerializeField]
        string m_ItemId;

        public string itemId => m_ItemId;

        public void SetItemId(string id) => m_ItemId = id;
    }
}
