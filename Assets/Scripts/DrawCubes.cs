using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System;

public class DrawCubes : MonoBehaviour
{
    private Transform leftController;
    private Transform rightController;
    public SteamVR_Action_Boolean drawButton;
    public GameObject primitive;
    public GameObject drawnObject = null;

    public String filename = "default";
    public bool saveFile = false;
    public bool loadFile = false;


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
    private void Awake()
    {
        Load(filename);
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

        if (saveFile)
        {
            saveFile = false;
            Save(filename);
        }
        if (loadFile)
        {
            loadFile = false;
            Load(filename);
        }

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

    public void Save (string filename)
    {
        var transforms = gameObject.GetComponentsInChildren<Transform>();
        var entities = new GameEntity[transforms.Length];
        int i = 0;
        foreach (Transform transform in transforms)
        {
            entities[i++] = new GameEntity(transform);
        }
        using (FileStream file = File.Create(Application.persistentDataPath + filename + ".cubes"))
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(entities.GetType());
            serializer.WriteObject(file, entities);
        }
        Debug.Log("saved " + entities.Length + " objects to " + filename + ".cubes");
    }
    public void Load(string filename)
    {
        GameEntity[] entities = new GameEntity[1];

        try
        {
            using (FileStream file = File.Open(Application.persistentDataPath + filename + ".cubes", FileMode.Open))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(entities.GetType());
                entities = (GameEntity[])serializer.ReadObject(file);
            }
        } catch (FileNotFoundException exc)
        {
            Debug.Log("could not load " + filename + ".cubes");
            return;
        }
        var oldTransforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform transform in oldTransforms)
        {
            GameObject.Destroy(transform.gameObject);
        }
        Debug.Log("deleted " + oldTransforms.Length + " objects");

        foreach (GameEntity entity in entities)
        {
            entity.Instantiate(primitive, transform);
        }
        Debug.Log("loaded " + entities.Length + " objects from " + filename +".cubes");
    }

    struct GameEntity
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        public GameEntity (Transform transform)
        {
            position = transform.position;
            scale = transform.localScale;
            rotation = transform.rotation;
        }

        public GameObject Instantiate (GameObject primitive, Transform parent)
        {
            GameObject g = GameObject.Instantiate(primitive, position, rotation, parent);
            g.transform.localScale = scale;
            return g;
        }
    }


}
