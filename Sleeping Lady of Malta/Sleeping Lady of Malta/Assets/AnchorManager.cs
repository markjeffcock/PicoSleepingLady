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
    private InputActionReference leftTrigger;
    [SerializeField]
    private GameObject rightAnchorPreview;
    [SerializeField]
    private GameObject leftAnchorPreview;
    [SerializeField]
    public GameObject anchorPrefab;
    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

  
    public GameObject changedPrefab
    {
        get => anchorPrefab;
        set => anchorPrefab = value;
    }

    private Dictionary<ulong, GameObject> anchorMap = new Dictionary<ulong, GameObject>();

    // Start is called before the first frame update,
    // set anchor preview off
    void Start()
    {
        rightAnchorPreview.SetActive(false);
        leftAnchorPreview.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        rightTrigger.action.started += onRightTriggerPressed;
        rightTrigger.action.canceled += onRightTriggerReleased;
        leftTrigger.action.started += onLeftTriggerPressed;
        leftTrigger.action.canceled += onLeftTriggerReleased;
        PXR_Manager.AnchorEntityCreated += AnchorEntityCreated;
    }

    private void OnDisable()
    {
        rightTrigger.action.started -= onRightTriggerPressed;
        rightTrigger.action.canceled -= onRightTriggerReleased;
        leftTrigger.action.started -= onLeftTriggerPressed;
        leftTrigger.action.canceled -= onLeftTriggerReleased;
        PXR_Manager.AnchorEntityCreated -= AnchorEntityCreated;
    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }

    private void onRightTriggerPressed(InputAction.CallbackContext callback)
    {
        RightShowAnchorPreview();
    }

    private void onRightTriggerReleased(InputAction.CallbackContext callback)
    {
        RightCreateAnchor();
    }

    private void onLeftTriggerPressed(InputAction.CallbackContext callback)
    {
        LeftShowAnchorPreview();
    }

    private void onLeftTriggerReleased(InputAction.CallbackContext callback)
    {
        LeftCreateAnchor();
    }

    // Instantiate a Preview - Right
    private void RightShowAnchorPreview()
    {
        rightAnchorPreview.SetActive(true);
    }

    // Instantiate a Preview - Left
    private void LeftShowAnchorPreview()
    {
        leftAnchorPreview.SetActive(true);
    }

    // Create an the Anchor - Right
    private void RightCreateAnchor()
    {
        //hide anchor
        rightAnchorPreview.SetActive(false);
        // trigger Event
        PXR_MixedReality.CreateAnchorEntity(rightAnchorPreview.transform.position, rightAnchorPreview.transform.rotation, out ulong taskID);
    }

    // Create an the Anchor - Left
    private void LeftCreateAnchor()
    {
        //hide anchor
        leftAnchorPreview.SetActive(false);
        // trigger Event
        PXR_MixedReality.CreateAnchorEntity(leftAnchorPreview.transform.position, leftAnchorPreview.transform.rotation, out ulong taskID);
    }

    // Instantiate the Anchor Prefab
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
