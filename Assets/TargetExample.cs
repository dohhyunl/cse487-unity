using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetExample : MonoBehaviour
{
    /*
    [SerializeField]
    public InputAction positionInputAction =
    new InputAction(binding: "<MagicLeapController>/pointerPosition", expectedControlType: "Vector3");

    //The input action that will log every frame when pressed.
    [SerializeField]
    public InputAction triggerInputAction =
        new InputAction(binding: "<XRController>/triggerPressed", expectedControlType: "Button");
    */

    // wall placement objects
    public GameObject targetIndicator;
    public GameObject targetObject;
    public Material mateiralblue;

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

        controllerActions.Trigger.performed += Trigger_performed;
        controllerActions.Bumper.performed += Bumper_performed;
    }

    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Trigger pressed");

        targetObject.transform.position = targetIndicator.transform.position;
        targetObject.transform.rotation = targetIndicator.transform.rotation;

        GameObject newTarget = Instantiate(targetObject, targetIndicator.transform.position, targetIndicator.transform.rotation);
        newTarget.GetComponent<MeshRenderer>().sharedMaterial = mateiralblue;

    }

    private void Bumper_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Bumper pressed");

        targetObject.transform.position = targetIndicator.transform.position;
        targetObject.transform.rotation = targetIndicator.transform.rotation;

        GameObject newTarget = Instantiate(targetObject, targetIndicator.transform.position, targetIndicator.transform.rotation);
        newTarget.GetComponent<MeshRenderer>().sharedMaterial = mateiralblue;

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
            Debug.Log("Ray Hit");
            Debug.Log(hitInfo.transform);
            // get targetIndicator width
            float width = targetIndicator.GetComponent<MeshRenderer>().bounds.size.x;

            targetIndicator.transform.position = hitInfo.point + hitInfo.normal * width / 2;
            targetIndicator.transform.rotation = Quaternion.LookRotation(-hitInfo.normal);
            targetIndicator.gameObject.SetActive(true);
        }
    }
}
