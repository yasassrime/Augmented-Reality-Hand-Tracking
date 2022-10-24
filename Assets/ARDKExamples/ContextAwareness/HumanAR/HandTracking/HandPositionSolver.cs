using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARDK.Extensions;
using Niantic.ARDK.AR.Awareness;

public class HandPositionSolver : MonoBehaviour
{
    [SerializeField] private ARHandTrackingManager handTrackingManager;
    [SerializeField] private Camera ARCamera;
    [SerializeField] private float accuracyLevel = 0.85f;

    private Vector3 handPosition;
    public Vector3 HandPosition { get => handPosition; }

    void Start()
    {
        handTrackingManager.HandTrackingUpdated += UpdatedTrackingData;
    }

    private void OnDestroy()
    {
        handTrackingManager.HandTrackingUpdated -= UpdatedTrackingData;
    }

    private void UpdatedTrackingData(HumanTrackingArgs updatedData)
    {
        var trackingInfo = updatedData.TrackingData?.AlignedDetections;
        if(trackingInfo == null)
        {
            return;
        }
        foreach(var latestDataSet in trackingInfo)
        {
            if(latestDataSet.Confidence < accuracyLevel)
            {
                return;
            }

            Vector3 trackingFrameSize = new Vector3(latestDataSet.Rect.width, latestDataSet.Rect.height, 0);
            float depthEstimation = 0.2f + Mathf.Abs(1 - trackingFrameSize.magnitude);

            handPosition = ARCamera.ViewportToWorldPoint(new Vector3(latestDataSet.Rect.center.x, 1 - latestDataSet.Rect.center.y, depthEstimation));

        }
    }
}
