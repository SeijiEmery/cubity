using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class WorldTransformManip : MonoBehaviour
{
    // public SteamVR_TrackedObject leftController;
    // public SteamVR_TrackedObject rightController;
    public Transform leftController;
    public Transform rightController;

    public SteamVR_Action_Boolean pressAction;
    public SteamVR_Action_Vector2 thing;
    public SteamVR_Action_Vector3 fubar;

    public float translationScale = 10.0f;

    private Vector3 initialControllerDelta;
    private Vector3 initialWorldTranslation;
    private Vector3 initialWorldScale;
    private Quaternion initialWorldRotation;
    private Vector3 initialLeftControllerPos;
    private Vector3 initialRightControllerPos;

    enum CameraActionState {
        None, TranslateLeft, TranslateRight, FullTransform
    }
    public enum CameraRotationMode {
        None, RotateY, FullRotation
    }
    public CameraRotationMode rotationMode = CameraRotationMode.None;

    CameraActionState cameraActionState = CameraActionState.None;

    // Update is called once per frame
    void Update()
    {
        bool leftPressed = pressAction.GetState(SteamVR_Input_Sources.LeftHand);
        bool rightPressed = pressAction.GetState(SteamVR_Input_Sources.RightHand);

        var prevCameraActionState = cameraActionState;
        cameraActionState = leftPressed && rightPressed ? CameraActionState.FullTransform :
            leftPressed ? CameraActionState.TranslateLeft :
            rightPressed ? CameraActionState.TranslateRight :
            CameraActionState.None;

        if (cameraActionState != prevCameraActionState) {
            initialWorldTranslation = transform.position;
            initialWorldRotation = transform.rotation;
            initialWorldScale = transform.localScale;
            if (leftPressed) {
                initialLeftControllerPos = leftController.gameObject.transform.position;
            }
            if (rightPressed) {
                initialRightControllerPos = rightController.gameObject.transform.position;
            }
            if (leftPressed && rightPressed) {
                initialControllerDelta = initialLeftControllerPos - initialRightControllerPos;
            }
        }
        var leftDelta = leftController.gameObject.transform.position - initialLeftControllerPos;
        var rightDelta = rightController.gameObject.transform.position - initialRightControllerPos;
        var leftRightDelta = leftController.gameObject.transform.position - rightController.gameObject.transform.position;

        switch (cameraActionState) {
            case CameraActionState.TranslateLeft: {
                transform.position = initialWorldTranslation + leftDelta * translationScale * transform.localScale.magnitude;
            } break;
            case CameraActionState.TranslateRight: {
                transform.position = initialWorldTranslation + rightDelta * translationScale * transform.localScale.magnitude;
            } break;
            case CameraActionState.FullTransform: {
                transform.localScale = initialWorldScale * leftRightDelta.magnitude / initialControllerDelta.magnitude;
                transform.position = initialWorldTranslation
                    - (leftController.gameObject.transform.position + rightController.gameObject.transform.position) / 2.0f
                    * (transform.localScale.magnitude / initialWorldScale.magnitude);//` `11 * translationScale;
                switch (rotationMode) {
                    case CameraRotationMode.None: break;
                    case CameraRotationMode.RotateY: {
                        transform.rotation = initialWorldRotation * Quaternion.FromToRotation(
                            new Vector3(initialControllerDelta.x, 0.0f, initialControllerDelta.z).normalized,
                            new Vector3(leftRightDelta.x, 0.0f, leftRightDelta.z).normalized
                        );
                    } break;
                    case CameraRotationMode.FullRotation: {
                        transform.rotation = initialWorldRotation * Quaternion.FromToRotation(initialControllerDelta.normalized, leftRightDelta.normalized);
                    } break;
                }
            } break;
        }
    }
}
