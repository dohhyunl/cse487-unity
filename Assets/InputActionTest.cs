using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This script shows how to create an Input Action at runtime.
/// </summary>
public class InputActionTest : MonoBehaviour
{
    //The input action that will log it's Vector3 value every frame.
    [SerializeField]
    public InputAction positionInputAction =
        new InputAction(binding: "<MagicLeapController>/pointerPosition", expectedControlType: "Vector3");

    //The input action that will log every frame when pressed.
    [SerializeField]
    public InputAction triggerInputAction =
        new InputAction(binding: "<XRController>/trigger", expectedControlType: "Button");

    private void Start()
    {
        positionInputAction.Enable();

        triggerInputAction.Enable();
        triggerInputAction.performed += ActionOnPerformed;
    }

    private void Update()
    {
        // Print pointer position to log.
        Debug.Log($"Pointer Position: {positionInputAction.ReadValue<Vector3>()}");
    }

    private void ActionOnPerformed(InputAction.CallbackContext obj)
    {
        // Print trigger message to log.
        Debug.Log("The trigger action was performed.");
    }

    private void OnDestroy()
    {
        triggerInputAction.Dispose();
        positionInputAction.Dispose();
    }
}