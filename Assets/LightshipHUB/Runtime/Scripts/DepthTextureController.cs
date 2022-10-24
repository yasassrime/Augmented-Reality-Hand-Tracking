using UnityEngine;
using Niantic.ARDK.Extensions;

namespace Niantic.ARDK.Templates
{
  public class DepthTextureController : MonoBehaviour
  {
    [HideInInspector]
    public ARDepthManager DepthManager;
    public bool ShowDepthTexture;

    void Update()
    {
      DepthManager.ToggleDebugVisualization(ShowDepthTexture);
    }
  }
}
