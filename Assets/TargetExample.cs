using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetExample : MonoBehaviour
{
    // wall placement objects
    public GameObject targetIndicator;
    public GameObject targetObject;

    // inputs
    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    // Start is called before the first frame update
    void Start()
    {
        // set wall objecets as inactive    
        targetIndicator.SetActive(false);
        targetObject.SetActive(true);

        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
    }

    // Update is called once per frame
    void Update()
    {
        // get controller position and rotation
        Vector3 controllerPosition = controllerActions.PointerPosition.ReadValue<Vector3>();
        Quaternion controllerRotation = controllerActions.PointerRotation.ReadValue<Quaternion>();

        // position targetObject at the controller position
        targetObject.transform.position = controllerPosition;
        targetObject.transform.rotation = controllerRotation;

        // raycast from the controller outward
        Ray raycastRay = new Ray(controllerPosition, controllerRotation * Vector3.forward);

        // if ray hits an object on the Planes layer, position the indicator at the hit point and set it as active
        if (Physics.Raycast(raycastRay, out RaycastHit hitInfo, 100, LayerMask.GetMask("Planes")))
        {
            Debug.Log(hitInfo.transform);
            // get targetIndicator width
            float width = targetIndicator.GetComponent<MeshRenderer>().bounds.size.x;

            targetIndicator.transform.position = hitInfo.point + hitInfo.normal * width / 2;
            targetIndicator.transform.rotation = Quaternion.LookRotation(-hitInfo.normal);
            targetIndicator.gameObject.SetActive(true);
        }
    }
}
