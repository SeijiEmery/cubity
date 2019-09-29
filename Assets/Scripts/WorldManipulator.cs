using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class WorldManipulator : MonoBehaviour
{
    // public SteamVR_TrackedObject leftController;
    // public SteamVR_TrackedObject rightController;
    public Transform leftController;
    public Transform rightController;

    public SteamVR_Action_Boolean cameraManipButton;

    public float   worldScale = 1.0f;
    public Vector3 worldOrigin = Vector3.zero;
    public Quaternion worldRotation = Quaternion.identity;

    public float maxScale = 100.0f;
    public float minScale = 0.1f;

    public Vector3 worldBounds = Vector3.one * 1000f;

    public enum CameraRotationConstraints
    {
        None, RotateY, FullRotation
    }
    public CameraRotationConstraints cameraRotationMode = CameraRotationConstraints.RotateY;

    public enum CameraManipState
    {
        None, TranslateFromLeft, TranslateFromRight, TransformWithBothControllers
    }
    public CameraManipState cameraManipState = CameraManipState.None;

    private CameraManipState getCameraManipState ()
    {
        bool leftPressed = cameraManipButton.GetState(SteamVR_Input_Sources.LeftHand);
        bool rightPressed = cameraManipButton.GetState(SteamVR_Input_Sources.RightHand);

        return leftPressed && rightPressed ? CameraManipState.TransformWithBothControllers
            : leftPressed ? CameraManipState.TranslateFromLeft
            : rightPressed ? CameraManipState.TranslateFromRight
            : CameraManipState.None
        ;
    }
    public Vector3 initialLeftPos = Vector3.zero;
    public Vector3 initialRightPos = Vector3.zero;
    public Vector3 initialOrigin = Vector3.zero;
    public float initialScale = 1.0f;

    public Vector3 debugManipMovement = Vector3.zero;
    public float debugRelDelta = 1.0f;
    public float debugScaleFactorChange = 0.0f;
    public Vector3 debugLeft = Vector3.zero, debugRight = Vector3.zero;

    private void updateCameraManip () {
        var prevState = cameraManipState;
        cameraManipState = getCameraManipState();

        var leftPos = leftController.transform.position;
        var rightPos = rightController.transform.position;

        if (cameraManipState != prevState) {
            Debug.Log("switching from " + prevState + " to " + cameraManipState);
            initialLeftPos = leftPos;
            initialRightPos = rightPos;
            initialScale  = worldScale;
            initialOrigin = transform.position / initialScale;
        } else if (cameraManipState != CameraManipState.None) {
            Debug.Log("updating with " + cameraManipState);
            var leftDelta = leftPos - initialLeftPos;
            var rightDelta = rightPos - initialRightPos;
            var relDelta = (leftPos - rightPos).magnitude / (initialLeftPos - initialRightPos).magnitude;

            debugLeft = leftPos;
            debugRight = rightPos;

            debugManipMovement = getManipMovement(leftDelta, rightDelta);
            var newOrigin = initialOrigin + getManipMovement(leftDelta, rightDelta);
            worldOrigin.x = Mathf.Clamp(newOrigin.x, -Mathf.Abs(worldBounds.x), Mathf.Abs(worldBounds.x));
            worldOrigin.y = Mathf.Clamp(newOrigin.y, -Mathf.Abs(worldBounds.y), Mathf.Abs(worldBounds.y));
            worldOrigin.x = Mathf.Clamp(newOrigin.z, -Mathf.Abs(worldBounds.z), Mathf.Abs(worldBounds.z));

            debugRelDelta = relDelta;
            debugScaleFactorChange = getScaleFactorChange(leftDelta, rightDelta, relDelta);
            var newScale = initialScale * getScaleFactorChange(leftDelta, rightDelta, relDelta);
            worldScale = Mathf.Clamp(newScale, minScale, maxScale);

            var currentScale = transform.localScale.magnitude;
            if (currentScale != newScale)
            {
                transform.localScale = Vector3.one * newScale;
            }
            var currentPos = transform.position;
            var newPos = newOrigin;
            if (currentPos != newPos)
            {
                transform.position = newPos;
            }
        }
    }
    public float getMovementSpeed () { return 1.0f; }

    public Vector3 getManipMovement (Vector3 leftDelta, Vector3 rightDelta) {
        switch (cameraManipState)
        {
            case CameraManipState.None: return Vector3.zero;
            case CameraManipState.TranslateFromLeft: return leftDelta * worldScale * getMovementSpeed();
            case CameraManipState.TranslateFromRight: return rightDelta * worldScale * getMovementSpeed();
            case CameraManipState.TransformWithBothControllers: return (leftDelta + rightDelta) * worldScale * getMovementSpeed();
        }
        throw new System.Exception("unhandled state " + cameraManipState);
    }

    public float getScaleFactorChange (Vector3 leftDelta, Vector3 rightDelta, float relDelta)
    {
        switch (cameraManipState)
        {
            case CameraManipState.TransformWithBothControllers: return relDelta;
            default: return 1.0f;
        }
    }

    public void Update()
    {
        updateCameraManip();
    }
}
