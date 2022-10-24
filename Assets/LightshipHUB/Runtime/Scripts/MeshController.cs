using System.Collections;
using UnityEngine;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Mesh;
using Niantic.ARDK.Extensions.Meshing;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Utilities.Logging;

namespace Niantic.ARDK.Templates
{
  public class MeshController : MonoBehaviour
  {
    [HideInInspector]
    public ARSessionManager ARSessionManager;
    [HideInInspector]
    public ARMeshManager ARMeshManager;
    [HideInInspector]
    public Material InvisibleMaterial;
    public bool ShowWorldMesh = false;
    public Material WorldMeshMaterial;
    private bool _contextAwarenessLoadComplete = false;
    private Material _originalMaterial;

    IEnumerator ChangeMeshMaterial()
    {
      yield return new WaitForSeconds(0.2f);

      MeshMaterial meshMat = (MeshMaterial)GameObject.FindObjectOfType(typeof(MeshMaterial));
      if (meshMat != null)
      {
        if (!ShowWorldMesh)
        {
          meshMat.SetMaterialToMesh(InvisibleMaterial);
        }
        else
        {
          meshMat.SetMaterialToMesh(WorldMeshMaterial);
        }
      }
    }

    void SetMaterialToGameObject(GameObject obj, Material material)
    {
      if (obj == null)
      {
        Debug.Log("obj is null");
        return;
      }

      MeshRenderer mesh = obj.GetComponent<MeshRenderer>();
      if (mesh != null) mesh.sharedMaterial = material;

      foreach (Transform child in obj.transform)
      {
        if (null == child) continue;
        SetMaterialToGameObject(child.gameObject, material);
      }
    }

    void Start()
    {
      //Following lines were on Awake function. Moved to start so as to not generate conflict with preloading. Now the coroutine starts when preloading has finished.
      var logFeatures = new string[] { "Niantic.ARDK.Extensions.Meshing", "UnityEngine.Events.UnityAction" };
      ARLog.EnableLogFeatures(logFeatures);

      StartCoroutine(ChangeMeshMaterial());

      _originalMaterial = ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial;
      if (WorldMeshMaterial != null) ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial = WorldMeshMaterial;
      ARMeshManager.SetUseInvisibleMaterial(!ShowWorldMesh);

      ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    private void OnDestroy()
    {
      // Set original prefab material again. If we don't do that the prefab stays with new material.
      if (ARMeshManager != null && WorldMeshMaterial != null) ARMeshManager.MeshPrefab.GetComponent<MeshRenderer>().sharedMaterial = _originalMaterial;
      ARSessionFactory.SessionInitialized -= OnSessionInitialized;

      if (ARSessionManager.ARSession != null) ARSessionManager.ARSession.Mesh.MeshBlocksUpdated -= OnMeshUpdated;
    }

    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
      args.Session.Mesh.MeshBlocksUpdated += OnMeshUpdated;

      _contextAwarenessLoadComplete = false;
    }

    private void OnMeshUpdated(MeshBlocksUpdatedArgs args)
    {
      if (!_contextAwarenessLoadComplete)
      {
        _contextAwarenessLoadComplete = true;
      }
    }

    private void Update()
    {
      if (ARSessionManager.ARSession != null && !_contextAwarenessLoadComplete)
      {
        var status = ARSessionManager.ARSession.GetAwarenessInitializationStatus(
            out AwarenessInitializationError error,
            out string errorMessage
        );

        if (status == AwarenessInitializationStatus.Ready)
        {
          _contextAwarenessLoadComplete = true;
        }
        else if (status == AwarenessInitializationStatus.Failed)
        {
          _contextAwarenessLoadComplete = true;
          Debug.LogErrorFormat(
              "Failed to initialize Context Awareness processes. Error: {0} ({1})",
              error,
              errorMessage
          );
        }
      }
    }
  }
}
