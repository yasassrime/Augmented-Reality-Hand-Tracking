using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Preloading;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.ARDK.Templates
{
  public class PreloadController : MonoBehaviour
  {
    private FeaturePreloadManager _preloadManager;
    [HideInInspector]
    public GameObject ARController;
    [HideInInspector]
    public GameObject Loading;
    [HideInInspector]
    public Text MessageText;

    private void Awake()
    {
      Loading.SetActive(true);
      //Assign vars
      _preloadManager = GetComponent<FeaturePreloadManager>();
      if (ARController != null) ARController.SetActive(false);

      //Init and download context awareness feature
      _preloadManager.Initialize();
      _preloadManager.ProgressUpdated += OnProgressUpdated;
      _preloadManager.StartDownload();
    }

    private void OnProgressUpdated(FeaturePreloadManager.PreloadProgressUpdatedArgs args)
    {
      if (args.PreloadAttemptFinished)
      {
        var success = args.FailedPreloads.Count == 0;
        if (success) 
        {
          Loading.SetActive(false);
          if (ARController != null) ARController.SetActive(true);
          _preloadManager.ProgressUpdated -= OnProgressUpdated;

          //Clean the PreloadController from Scene to not overload hierarchy for devs
          gameObject.transform.SetParent(ARController.transform, false);
          gameObject.SetActive(false);
        }
        else 
        {
          MessageText.text = "ERROR";
        }
      }
    }
  }
}