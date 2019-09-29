using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class DrawCubes : MonoBehaviour
{
    private Transform leftController;
    private Transform rightController;
    public SteamVR_Action_Boolean drawButton;
    public GameObject primitive;
    public GameObject drawnObject = null;

    public enum DrawState
    {
        None, DrawWithLeft, DrawWithRight
    }
    public DrawState drawState = DrawState.None;
    public Vector3 drawStartPos;
    private WorldManipulator worldManip = null;

    private void Start ()
    {
        worldManip = GetComponent<WorldManipulator>();
        leftController = worldManip.leftController;
        rightController = worldManip.rightController;
        drawnObject = null;
    }

    private void Update()
    {
        bool leftPressed = drawButton.GetState(SteamVR_Input_Sources.LeftHand);
        bool rightPressed = drawButton.GetState(SteamVR_Input_Sources.RightHand);
        bool anyPressed = leftPressed || rightPressed;

        if (drawState == DrawState.None && anyPressed)
        {
            if (rightPressed) { drawState = DrawState.DrawWithRight; beginDraw(rightController.position); }
            else if (leftPressed) { drawState = DrawState.DrawWithLeft; beginDraw(leftController.position); }
        }
        else if (drawState != DrawState.None && !anyPressed)
        {
            endDraw();
            drawState = DrawState.None;
        }
        if (drawState != DrawState.None)
            updateDraw(drawState == DrawState.DrawWithRight ? rightController.position : leftController.position);
    }

    void beginDraw (Vector3 pos)
    {
        drawStartPos = pos;
        if (drawnObject == null)
        {
            drawnObject = GameObject.Instantiate(primitive, pos, Quaternion.identity, transform);
            drawnObject.transform.localScale = Vector3.one * 1.0f;
        }

    }
    void endDraw ()
    {
        drawnObject = null;
    }
    void updateDraw (Vector3 pos)
    {
        drawnObject.transform.localScale = Vector3.one * Vector3.Distance(pos, drawStartPos) * worldManip.worldScale;
    }
}
