using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;

public class AnchorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AnchorTest.Start");
        // Get the ARAnchorManager
        ARAnchorManager anchorManager = FindObjectOfType<ARAnchorManager>();
        if (anchorManager == null)
        {
            Debug.LogError("ARAnchorManager not found");
            return;
        }

        // Create a new ARAnchor
        ARAnchor anchor = anchorManager.AddAnchor(new Pose(new Vector3(0, 0, 0), Quaternion.identity));

        // Log the anchor's position
        Debug.Log("Anchor position: " + anchor.transform.position);

        // Host the anchor to the cloud
        CloudAnchorMode cloudAnchorMode = CloudAnchorMode.Enabled;
        HostCloudAnchorPromise result = anchorManager.HostCloudAnchorAsync(anchor, 1);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
