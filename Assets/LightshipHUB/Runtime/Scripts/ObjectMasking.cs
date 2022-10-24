using UnityEngine;
using Niantic.ARDK.VirtualStudio.AR.Mock;

namespace Niantic.ARDK.Templates
{
  public class ObjectMasking : MonoBehaviour
  {
    [HideInInspector]
    public GameObject Holder;
    public MockSemanticLabel.ChannelName ChannelType;

    void Awake()
    {
      SetLayerToGameObject(this.gameObject, ChannelType.ToString());

      bool hasObject = false;
      if (Holder)
      {
        foreach (Transform child in Holder.transform)
        {
          hasObject = true;
          break;
        }
      }
      else
      {
        hasObject = true;
      }

      if (hasObject)
      {
        ObjectMaskingController controller = (ObjectMaskingController)GameObject.FindObjectOfType(typeof(ObjectMaskingController));
        controller.AllChannels.Add(ChannelType);
      }
    }
    private static void SetLayerToGameObject(GameObject obj, string layerName)
    {
      if (obj == null) return;
      obj.layer = LayerMask.NameToLayer(layerName);

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        SetLayerToGameObject(child.gameObject, layerName);
      }
    }
  }
}