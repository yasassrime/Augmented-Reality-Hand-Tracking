#if (UNITY_EDITOR)
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using System.IO;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Permissions;
using Niantic.ARDK.Extensions.Meshing;
using Niantic.ARDK.VirtualStudio.AR.Mock;
using Niantic.ARDK.VirtualStudio.VpsCoverage;
using Niantic.ARDK.Utilities.Preloading;

namespace Niantic.ARDK.Templates
{
  public class TemplateFactory
  {
    private static GameObject _ARSceneManager
    {
      get
      {
        return LightshipCommon.ARSceneManager;
      }
    }

    private static GameObject _target
    {
      get
      {
        return LightshipCommon.Target;
      }
      set
      {
        LightshipCommon.Target = value;
      }
    }

    private static Camera _camera
    {
      get
      {
        return LightshipCommon.Camera;
      }
    }

    private static bool requestPreloadManager = true;

    public static GameObject CreateTemplate_AnchorPlacement()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddPlaneManager(_ARSceneManager);
      //LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Interior);

      ObjectHolderController controller = LightshipCommon.CheckPrefab<ObjectHolderController>("Assets/LightshipHUB/Runtime/Prefabs/ARController.prefab");

      try
      {
        PrefabUtility.UnpackPrefabInstance(controller.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      controller.Camera = _camera;
      LightshipCommon.ObjectHolder = controller.ObjectHolder;

      LightshipCommon.Change3DModel(LightshipCommon.Model3D.GIFT);

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }
    
    // public static GameObject CreateTemplate_AnchorInteraction() 
    // {
    //   if (LightshipCommon.CheckARDK()) return null;

    //   GameObject controllerGO = CreateTemplate_AnchorPlacement();
    //   InteractionController controller = LightshipCommon.AddComponentToGameObject<InteractionController>(controllerGO);
    //   controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

    //   GameObject holder = controllerGO.GetComponent<ObjectHolderController>().ObjectHolder;
    //   ObjectAnimation anchorController = holder.GetComponent<ObjectAnimation>();

    //   ObjectInteraction objectAnimation = LightshipCommon.AddComponentToGameObject<ObjectInteraction>(holder);
    //   objectAnimation.InteractionController = controller;
    //   UnityEventTools.AddPersistentListener(objectAnimation.OnClick, anchorController.ScaleInOut);
    //   UnityEventTools.AddPersistentListener(objectAnimation.OnDistance, anchorController.Rotate);

    //   UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());

    //   GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.SKYROCKET);
    //   newObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

    //   _target = GameObject.Find("[REPLACE ME]");
    //   LightshipCommon.ShowTarget();

    //   return controllerGO;
    // }
    //
    public static GameObject CreateTemplate_PlaneTracker(bool preloadManagerNeeded)
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_AnchorPlacement();
      PlaneTrackerController controller = LightshipCommon.AddComponentToGameObject<PlaneTrackerController>(controllerGO);
      controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      ARPlaneManager planeManager = _ARSceneManager.GetComponent<ARPlaneManager>();
      controller.PlaneManager = planeManager;
      LightshipCommon.AddPrefabToPlaneManager(planeManager);

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.CAR);
      newObj.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      if (preloadManagerNeeded) LightshipCommon.AddPreloadManager();

      return controllerGO;
    }

    public static GameObject CreateTemplate_DepthTextureOcclusion()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_PlaneTracker(requestPreloadManager);
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      ARDepthManager depthManager = LightshipCommon.AddDepthManager(_camera.gameObject);
      DepthTextureController controller = LightshipCommon.AddComponentToGameObject<DepthTextureController>(controllerGO);
      controller.DepthManager = depthManager;

      return controllerGO;
    }

    public static GameObject CreateTemplate_MeshOcclusion()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_PlaneTracker(requestPreloadManager);
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      MeshController controller = LightshipCommon.AddComponentToGameObject<MeshController>(controllerGO);
      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      controller.WorldMeshMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARDK/Extensions/Meshing/Materials/MeshNormalFresnel.mat");

      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      controller.ARMeshManager = arMesh;
      controller.InvisibleMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ARDK/Extensions/Meshing/Materials/MeshInvisible.mat");

      return controllerGO;
    }

    public static GameObject CreateTemplate_RealtimeMeshing()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_MeshOcclusion();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      GameObject meshPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");

      LightshipCommon.SetupComponentProperty(arMesh, "_meshPrefab", meshPref);

      MeshPlacementController meshPController = LightshipCommon.AddComponentToGameObject<MeshPlacementController>(controllerGO);
      meshPController.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlaneTrackerController>());
      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<MeshController>().ARSessionManager.gameObject.GetComponent<ARPlaneManager>());

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.MUSHROOM);
      newObj.transform.localPosition = Vector3.zero;
      newObj.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.AddComponentToGameObject<MushroomAnimation>(_target);
      LightshipCommon.ShowTarget();

      return controllerGO;
    }

    public static GameObject CreateTemplate_MeshCollider()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_MeshOcclusion();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      LightshipCommon.PreloadController.ARController = controllerGO;
      ARMeshManager arMesh = LightshipCommon.CheckPrefab<ARMeshManager>("Assets/ARDK/Extensions/Meshing/ARMesh.prefab");
      GameObject meshPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ARDK/Extensions/Meshing/MeshColliderChunk.prefab");

      LightshipCommon.SetupComponentProperty(arMesh, "_meshPrefab", meshPref);

      MeshColliderController colliderController = LightshipCommon.AddComponentToGameObject<MeshColliderController>(controllerGO);
      colliderController.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      GameObject newObj = LightshipCommon.Change3DModel(LightshipCommon.Model3D.GIFT);
      newObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

      LightshipCommon.AddComponentToGameObject<Rigidbody>(controllerGO.transform.GetChild(0).gameObject);

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlaneTrackerController>());
      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<MeshController>().ARSessionManager.gameObject.GetComponent<ARPlaneManager>());

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controllerGO;
    }

    public static GameObject CreateTemplate_SemanticSegmentation()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddSemanticSegmentation(_camera.gameObject);

      //GameObject mockup = LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Exterior);
      GameObject controllerGO = LightshipCommon.CheckSceneObjectComponent<SegmentationController>("ARController");
      _target = controllerGO;
      SegmentationController controller = controllerGO.GetComponent<SegmentationController>();

      GameObject canvas = LightshipCommon.CheckSceneObjectComponent<Canvas>("Canvas");
      canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
      LightshipCommon.AddComponentToGameObject<CanvasScaler>(canvas);
      canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      controller.SemanticSegmentationManager = _camera.gameObject.GetComponent<ARSemanticSegmentationManager>();
      controller.Canvas = canvas.GetComponent<Canvas>();
      controller.CustomShader = Shader.Find("Custom/Segmentation");

      int index = 0;
      controller.Segmentations = new SegmentationController.Segmentation[6];

      foreach (MockSemanticLabel.ChannelName channel in Enum.GetValues(typeof(MockSemanticLabel.ChannelName)))
      {
        if (channel == MockSemanticLabel.ChannelName.grass) continue;
        if (!File.Exists("Assets/LightshipHUB/Runtime/Textures/SegmentationExamples/" + channel.ToString().ToLower() + ".png")) continue;
        controller.Segmentations[index].ChannelType = channel;
        controller.Segmentations[index].Texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LightshipHUB/Runtime/Textures/SegmentationExamples/" + channel.ToString().ToLower() + ".png");
        index++;
      }

      LightshipCommon.ShowTarget();

      PreloadController preloader = LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      preloader.ARController = controllerGO;

      return controllerGO;
    }

    public static GameObject CreateTemplate_ObjectMasking()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddSemanticSegmentation(_camera.gameObject);

      //GameObject mockup = LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Exterior);
      GameObject controllerGO = LightshipCommon.CheckSceneObjectComponent<ObjectMaskingController>("ARController");
      GameObject prefabOH = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/MaskedObjectsHolder.prefab");
      GameObject objectsHolder = (GameObject)PrefabUtility.InstantiatePrefab(prefabOH, controllerGO.transform);
      PrefabUtility.UnpackPrefabInstance(objectsHolder.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      _target = objectsHolder.transform.GetChild(0).GetChild(0).gameObject;

      ObjectMaskingController controller = controllerGO.GetComponent<ObjectMaskingController>();

      foreach (MockSemanticLabel.ChannelName channel in Enum.GetValues(typeof(MockSemanticLabel.ChannelName)))
      {
        LightshipCommon.CreateLayer(channel.ToString().ToLower());
      }

      _camera.cullingMask = (1 << 0 |
                          1 << 1 |
                          1 << 2 |
                          1 << 3 |
                          1 << 4 |
                          1 << 5);

      GameObject canvas = LightshipCommon.CheckSceneObjectComponent<Canvas>("Canvas");
      canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
      LightshipCommon.AddComponentToGameObject<CanvasScaler>(canvas);
      canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

      controller.Camera = _camera;
      controller.ARSessionManager = _ARSceneManager.GetComponent<ARSessionManager>();
      controller.SemanticSegmentationManager = _camera.gameObject.GetComponent<ARSemanticSegmentationManager>();
      controller.Canvas = canvas.GetComponent<Canvas>();
      controller.CustomShader = Shader.Find("Custom/ObjectMasking");

      GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/SegmentationCameras.prefab");
      GameObject cameras = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
      controller.SegmentationCamerasGO = cameras;

      LightshipCommon.ShowTarget();

      PreloadController preloader = LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.ContextAwareness);
      preloader.ARController = controllerGO;

      return controllerGO;
    }

    public static GameObject CreateTemplate_OptimizedObjectMasking()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_ObjectMasking();
      ObjectMaskingController controller = controllerGO.GetComponent<ObjectMaskingController>();
      controller.CustomShader = Shader.Find("Custom/OptimizedObjectMasking");

      return controllerGO;
    }

    public static GameObject CreateTemplate_SharedObjectInteraction()
    {
      if (LightshipCommon.CheckARDK()) return null;

      LightshipCommon.SetupARSceneManager();
      LightshipCommon.SetupARCamera();
      LightshipCommon.AddPlaneManager(_ARSceneManager);
      //LightshipCommon.AddMockupWorld(LightshipCommon.MockupKind.Interior);

      NetworkSessionManager nSession = LightshipCommon.AddComponentToGameObject<NetworkSessionManager>(_ARSceneManager);
      ARNetworkingManager ARnetworking = LightshipCommon.AddComponentToGameObject<ARNetworkingManager>(_ARSceneManager);

      SharedSession controller = LightshipCommon.CheckPrefab<SharedSession>("Assets/LightshipHUB/Runtime/Prefabs/SharedARController.prefab");

      try
      {
        PrefabUtility.UnpackPrefabInstance(controller.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
      }
      catch { }

      LightshipCommon.SetupComponentProperty(_ARSceneManager.GetComponent<ARSessionManager>(), "_manageUsingUnityLifecycle", false);
      LightshipCommon.SetupComponentProperty(_ARSceneManager.GetComponent<ARSessionManager>(), "_useWithARNetworkingSession", true);

      LightshipCommon.SetupComponentProperty(nSession, "_manageUsingUnityLifecycle", false);
      LightshipCommon.SetupComponentProperty(nSession, "_inputField", GameObject.Find("SessionIDField"));
      LightshipCommon.SetupComponentProperty(nSession, "_useWithARNetworkingSession", true);

      controller._camera = _camera;
      controller._arManager = ARnetworking;

      ARnetworking.enabled = false;

      PreloadController preloader = LightshipCommon.AddPreloadManager();
      LightshipCommon.PreloadManager.AddFeature(Feature.Dbow);
      preloader.ARController = controller.gameObject;

      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_VPSCoverage()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controller = LightshipCommon.CreateSceneObject("Controller");
      VPSCoverageController vpsCoverage = LightshipCommon.AddComponentToGameObject<VPSCoverageController>(controller);
      vpsCoverage.MockResponses = AssetDatabase.LoadAssetAtPath<VpsCoverageResponses>("Assets/ARDK/VirtualStudio/VpsCoverage/VPSCoverageResponses.asset");

      LightshipCommon.AddAndroidTools(ARDKPermission.Camera, controller);

      GameObject canvas = LightshipCommon.CheckSceneObjectComponent<Canvas>("Canvas");
      canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
      CanvasScaler canvasScaler = LightshipCommon.AddComponentToGameObject<CanvasScaler>(canvas);
      canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
      LightshipCommon.AddComponentToGameObject<GraphicRaycaster>(canvas);

      GameObject targetImage = LightshipCommon.CreateSceneObject("TargetImage");
      targetImage.transform.parent = canvas.transform;
      RawImage rawImage = LightshipCommon.AddComponentToGameObject<RawImage>(targetImage);
      vpsCoverage.TargetImage = rawImage;
      rawImage.rectTransform.anchoredPosition = new Vector2(15, -470);
      rawImage.rectTransform.sizeDelta = new Vector2(777, 662);

      LightshipCommon.SetLayerToGameObject(canvas, "UI");

      return controller;
    }

    public static GameObject CreateTemplate_VPSCoverageList()
    {
      if (LightshipCommon.CheckARDK()) return null;

      VPSCoverageListController controller = LightshipCommon.CheckPrefab<VPSCoverageListController>("Assets/LightshipHUB/Runtime/Prefabs/ListController.prefab");
      controller.MockResponses = AssetDatabase.LoadAssetAtPath<VpsCoverageResponses>("Assets/ARDK/VirtualStudio/VpsCoverage/VPSCoverageResponses.asset");

      return controller.gameObject;
    }

    public static GameObject CreateTemplate_WayspotAnchors()
    {
      if (LightshipCommon.CheckARDK()) return null;

      GameObject controllerGO = CreateTemplate_AnchorPlacement();
      WayspotAnchorTemplateController controller = LightshipCommon.AddComponentToGameObject<WayspotAnchorTemplateController>(controllerGO);
      controller.OHcontroller = controllerGO.GetComponent<ObjectHolderController>();

      ARPlaneManager planeManager = _ARSceneManager.GetComponent<ARPlaneManager>();

      UnityEngine.Object.DestroyImmediate(controllerGO.GetComponent<PlacementController>());

      GameObject canvasPref = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LightshipHUB/Runtime/Prefabs/WayspotAnchorsCanvas.prefab");
      GameObject canvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPref);

      foreach (Transform child in canvas.transform)
      {
        switch (child.name)
        {
          case "Status Panel":
            foreach (Transform secondChild in child)
            {
              if (secondChild.name == "Status Log") controller.StatusLog = secondChild.GetComponent<Text>();
              else if (secondChild.name == "Localization Status") controller.LocalizationStatus = secondChild.GetComponent<Text>();
            }
            break;
          case "Pause Session Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.PauseARSession);
            break;
          case "Resume Session Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.ResumeARSession);
            break;
          case "Clear Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.ClearAnchorGameObjects);
            break;
          case "Load Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.LoadWayspotAnchors);
            break;
          case "Save Anchors Button":
            UnityEventTools.AddPersistentListener(child.GetComponent<Button>().onClick, controller.SaveWayspotAnchors);
            break;
          default:
            break;
        }
      }
      //TODO: Check session managment in wayspotAnchorController. Make it compatible with objectholdercontroller.
      _target = GameObject.Find("[REPLACE ME]");
      LightshipCommon.ShowTarget();

      return controllerGO;
    }
  }
}
#endif