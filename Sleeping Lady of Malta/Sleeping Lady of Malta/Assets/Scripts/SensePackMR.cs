using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class SensePackMR : MonoBehaviour
{
    // Initial awakening of See-through
    private void Awake()
    {
        PXR_MixedReality.EnableVideoSeeThrough(true);
    }

    // Re-enable See-through after the app resumes
    private void OnApplicationPause(bool pause)
    {
        if(!pause)
            PXR_MixedReality.EnableVideoSeeThrough(true);
    }

        // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
