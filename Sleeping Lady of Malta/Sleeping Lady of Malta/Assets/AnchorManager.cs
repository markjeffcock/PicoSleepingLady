using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class AnchorManager : MonoBehaviour
{
    [SerializeField]
    private InputActionReference rightTrigger;
    [SerializeField]
    private GameObject anchorPreview;
    [SerializeField]
    private GameObject anchorPrefab;
    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

    private Dictionary<ulong, GameObject> anchorMap = new Dictionary<ulong, GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        rightTrigger.action.started += onRightTriggerPressed;
        rightTrigger.action.canceled += onRightTriggerReleased;
        PXR_Manager.AnchorEntityCreated += AnchorEntityCreated;
    }

    private void OnDisable()
    {
        rightTrigger.action.started -= onRightTriggerPressed;
        rightTrigger.action.canceled -= onRightTriggerReleased;
        PXR_Manager.AnchorEntityCreated -= AnchorEntityCreated;
    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }

    private void onRightTriggerPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview();
    }

    private void onRightTriggerReleased(InputAction.CallbackContext callback)
    {
        CreateAnchor();
    }

    // Instantiate a Preview
    private void ShowAnchorPreview()
    {
        anchorPreview.SetActive(true);
    }

    // Create an the Anchor
    private void CreateAnchor()
    {
        //hide anchor
        anchorPreview.SetActive(false);
        // trigger Event
        PXR_MixedReality.CreateAnchorEntity(anchorPreview.transform.position, anchorPreview.transform.rotation, out ulong taskID);
    }

    // Instatiate the Anchor Prefab
    private void AnchorEntityCreated(PxrEventAnchorEntityCreated result)
    {
        if(result.result == PxrResult.SUCCESS)
        {
            GameObject anchorObject = Instantiate(anchorPrefab);
            PXR_MixedReality.GetAnchorPose(result.anchorHandle, out var rotation, out var position);
            anchorObject.transform.position = position;
            anchorObject.transform.rotation = rotation;

            // spatial drift handling
            anchorMap.Add(result.anchorHandle, anchorObject);
        }
    }

    private void HandleSpatialDrift()
    {
        // no handling required if no anchors
        if (anchorMap.Count == 0)
            return;

        currDriftDelay += Time.deltaTime;
        if(currDriftDelay >= maxDriftDelay)
        {
            currDriftDelay = 0;
            foreach (var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorObj = handlePair.Value;

                //if (handle == UInt64.MinValue)
                //{
                //   Debug.LogError("Handle is invalid");
                //    continue;
                //}

                PXR_MixedReality.GetAnchorPose(handle, out var rotation, out var position);
                anchorObj.transform.position = position;
                anchorObj.transform.rotation = rotation;
            }
        }        
    }
}
