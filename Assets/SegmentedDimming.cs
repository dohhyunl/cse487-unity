using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class SegmentedDimming : MonoBehaviour
{
    void Start()
    {
        MLSegmentedDimmer.Activate();
    }
}