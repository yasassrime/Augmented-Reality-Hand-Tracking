using System;
using System.Collections.Generic;
using System.Linq;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities.Input.Legacy;

using UnityEngine;
using UnityEngine.UI;

namespace Niantic.ARDK.Templates
{
  public class WayspotAnchorTemplateController : MonoBehaviour
  {
    [HideInInspector]
    public ObjectHolderController OHcontroller;
    [HideInInspector]
    public Text StatusLog;
    [HideInInspector]
    public Text LocalizationStatus;

    private WayspotAnchorService _wayspotAnchorService;
    private IARSession _arSession;
    private LocalizationState _localizationState;

    private readonly Dictionary<Guid, GameObject> _wayspotAnchorGameObjects =
    new Dictionary<Guid, GameObject>();

    private void Awake()
    {
      StatusLog.text = "Initializing Session.";
    }

    private void OnEnable()
    {
      ARSessionFactory.SessionInitialized += HandleSessionInitialized;
    }

    private void Update()
    {
      if (_wayspotAnchorService == null)
      {
        return;
      }
      //Get the pose where you tap on the screen
      var success = TryGetTouchInput(out Matrix4x4 localPose);
      if (_wayspotAnchorService.LocalizationState == LocalizationState.Localized)
      {
        if (success) //Check is screen tap was a valid tap
        {
          PlaceAnchor(localPose); //Create the Wayspot Anchor and place the GameObject
        }
      }
      else
      {
        if (success) //Check is screen tap was a valid tap
        {
          StatusLog.text = "Must localize before placing anchor.";
        }
        if (_localizationState != _wayspotAnchorService.LocalizationState)
        {
          _wayspotAnchorGameObjects.Values.ToList().ForEach(a => a.SetActive(false));
        }
      }
      LocalizationStatus.text = _wayspotAnchorService.LocalizationState.ToString();
      _localizationState = _wayspotAnchorService.LocalizationState;
    }

    private void OnDisable()
    {
      ARSessionFactory.SessionInitialized -= HandleSessionInitialized;
    }

    private void OnDestroy()
    {
      _wayspotAnchorService.Dispose();
    }

    /// Saves all of the existing wayspot anchors
    public void SaveWayspotAnchors()
    {
      if (_wayspotAnchorGameObjects.Count > 0)
      {
        var wayspotAnchors = _wayspotAnchorService.GetAllWayspotAnchors();
        var payloads = wayspotAnchors.Select(a => a.Payload);
        WayspotAnchorTemplateDataUtility.SaveLocalPayloads(payloads.ToArray());
      }
      else
      {
        WayspotAnchorTemplateDataUtility.SaveLocalPayloads(Array.Empty<WayspotAnchorPayload>());
      }

      StatusLog.text = "Saved Wayspot Anchors.";
    }

    /// Loads all of the saved wayspot anchors
    public void LoadWayspotAnchors()
    {
      if (_wayspotAnchorService.LocalizationState != LocalizationState.Localized)
      {
        StatusLog.text = "Must localize before loading anchors.";
        return;
      }

      var payloads = WayspotAnchorTemplateDataUtility.LoadLocalPayloads();
      if (payloads.Length > 0)
      {
        var wayspotAnchors = _wayspotAnchorService.RestoreWayspotAnchors(payloads);
        CreateAnchorGameObjects(wayspotAnchors);
        StatusLog.text = "Loaded Wayspot Anchors.";
      }
      else
      {
        StatusLog.text = "No anchors to load.";
      }
    }

    /// Clears all of the active wayspot anchors
    public void ClearAnchorGameObjects()
    {
      if (_wayspotAnchorGameObjects.Count == 0)
      {
        StatusLog.text = "No anchors to clear.";
        return;
      }

      foreach (var anchor in _wayspotAnchorGameObjects)
      {
        Destroy(anchor.Value);
      }

      _wayspotAnchorService.DestroyWayspotAnchors(_wayspotAnchorGameObjects.Keys.ToArray());
      _wayspotAnchorGameObjects.Clear();
      StatusLog.text = "Cleared Wayspot Anchors.";
    }

    /// Pauses the AR Session
    public void PauseARSession()
    {
      if (_arSession.State == ARSessionState.Running)
      {
        _arSession.Pause();
        StatusLog.text = $"AR Session Paused.";
      }
      else
      {
        StatusLog.text = $"Cannot pause AR Session.";
      }
    }

    /// Resumes the AR Session
    public void ResumeARSession()
    {
      if (_arSession.State == ARSessionState.Paused)
      {
        _arSession.Run(_arSession.Configuration);
        StatusLog.text = $"AR Session Resumed.";
      }
      else
      {
        StatusLog.text = $"Cannot resume AR Session.";
      }
    }

    private void HandleSessionInitialized(AnyARSessionInitializedArgs anyARSessionInitializedArgs)
    {
      StatusLog.text = "Running Session...";
      _arSession = anyARSessionInitializedArgs.Session;
      _arSession.Ran += HandleSessionRan;
    }

    private void HandleSessionRan(ARSessionRanArgs arSessionRanArgs)
    {
      _arSession.Ran -= HandleSessionRan;
      _wayspotAnchorService = CreateWayspotAnchorService();
      StatusLog.text = "Session Initialized.";
    }

    private void PlaceAnchor(Matrix4x4 localPose)
    {
      var anchors = _wayspotAnchorService.CreateWayspotAnchors(localPose);
      CreateAnchorGameObjects(anchors);
      StatusLog.text = "Anchor placed.";
    }

    private WayspotAnchorService CreateWayspotAnchorService()
    {
      var wayspotAnchorsConfiguration = WayspotAnchorsConfigurationFactory.Create();

      var locationService = LocationServiceFactory.Create(_arSession.RuntimeEnvironment);
      locationService.Start();

      var wayspotAnchorService = new WayspotAnchorService(_arSession, locationService, wayspotAnchorsConfiguration);
      return wayspotAnchorService;
    }

    private void CreateAnchorGameObjects(IWayspotAnchor[] wayspotAnchors)
    {
      foreach (var wayspotAnchor in wayspotAnchors)
      {
        if (_wayspotAnchorGameObjects.ContainsKey(wayspotAnchor.ID))
        {
          continue;
        }
        wayspotAnchor.TransformUpdated += HandleWayspotAnchorTrackingUpdated;
        var id = wayspotAnchor.ID;
        var anchor = Instantiate(OHcontroller.ObjectHolder);
        anchor.SetActive(false);
        anchor.name = $"Anchor {id}";
        _wayspotAnchorGameObjects.Add(id, anchor);
      }
    }

    private void HandleWayspotAnchorTrackingUpdated(WayspotAnchorResolvedArgs wayspotAnchorResolvedArgs)
    {
      var anchor = _wayspotAnchorGameObjects[wayspotAnchorResolvedArgs.ID].transform;
      anchor.position = wayspotAnchorResolvedArgs.Position;
      anchor.rotation = wayspotAnchorResolvedArgs.Rotation;
      anchor.gameObject.SetActive(true);
    }

    private bool TryGetTouchInput(out Matrix4x4 localPose)
    {
      if (_arSession == null || PlatformAgnosticInput.touchCount <= 0)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      var touch = PlatformAgnosticInput.GetTouch(0);
      if (touch.IsTouchOverUIObject())
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      if (touch.phase != TouchPhase.Began)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      var currentFrame = _arSession.CurrentFrame;
      if (currentFrame == null)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      var results = currentFrame.HitTest
      (
          OHcontroller.Camera.pixelWidth,
          OHcontroller.Camera.pixelHeight,
          touch.position,
          ARHitTestResultType.ExistingPlane
      );

      int count = results.Count;
      if (count <= 0)
      {
        localPose = Matrix4x4.zero;
        return false;
      }

      var result = results[0];
      localPose = result.WorldTransform;
      return true;
    }
  }
}
