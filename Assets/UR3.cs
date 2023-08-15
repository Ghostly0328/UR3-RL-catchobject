using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class UR3 : Agent
{
    [Header("Demo Setting")]
    [SerializeField] private bool isTest = false;
    [SerializeField] private int heuristicAction = 0;

    [Header("AI Setting")]
    [SerializeField] private bool isEyeCamera = true;
    private Transform termainal;
    private GameObject UR3GameObject;
    [SerializeField] private GameObject UR3Prefab;

    [SerializeField] private GameObject target;
    [SerializeField] private Transform target_basic;
    private GameObject getGameObject;

    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    [SerializeField] List<Transform> transformList = new List<Transform>();

    public float spawnAreaSize = 0.2f;

    private int maxStep;
    private int currentStep;

    private void Start()
    {
        if (isTest)
        {
            // inference slow down
            Time.timeScale = 0.1f;
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Start !");
        // Get MaxStep
        maxStep = MaxStep;
        // Reset CurrentStep
        currentStep = 0;

        if (getGameObject != null)
        {
            getGameObject.SetActive(false);
            UR3GameObject.SetActive(false);

            Destroy(getGameObject);
            Destroy(UR3GameObject);
        }
        // Spawn objects
        float x = Random.Range(-spawnAreaSize, spawnAreaSize);
        float y = Random.Range(-spawnAreaSize, spawnAreaSize);
        float z = Random.Range(-spawnAreaSize, spawnAreaSize);
        Vector3 spawnPosition = new Vector3(target_basic.position.x + x, target_basic.position.y + y, target_basic.position.z + z);

        // Instantiate object at random position
        getGameObject = Instantiate(target, spawnPosition, Quaternion.identity, transform.parent);
        getGameObject.GetComponent<TargetCollider>().brain = GetComponent<UR3>();
        UR3GameObject = Instantiate(UR3Prefab, transform.position, Quaternion.identity, transform.parent);
        //UR3GameObject = Instantiate(UR3Prefab, transform.position + new Vector3(-0.2f, 0.3f, 0), Quaternion.Euler(0, 0, 90), transform.parent);

        transformList[0] = UR3GameObject.transform.GetChild(0);
        transformList[1] = transformList[0].GetChild(0);
        transformList[2] = transformList[1].GetChild(0);
        transformList[3] = transformList[2].GetChild(0);
        transformList[4] = transformList[3].GetChild(0);
        transformList[5] = transformList[4].GetChild(0);

        foreach (Transform jointTransform in transformList)
        {
            ArticulationBody baseJoint = jointTransform.GetComponent<ArticulationBody>();
            ArticulationDrive drive = baseJoint.xDrive;

            float targetAngle = Random.Range(-90f, 90f);
            drive.target = targetAngle;
            baseJoint.xDrive = drive;
        }

        termainal = transformList[5].GetChild(0).Find("TerminalPoint");

        if (isEyeCamera)
        {
            CameraSensorComponent cameraSensor = gameObject.GetComponent<CameraSensorComponent>();
            cameraSensor.Camera = transformList[5].GetChild(0).Find("ArmCamera").GetComponent<Camera>();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int i = 0; i < transformList.Count; i++)
        {
            //Debug.Log(transformList[i].localRotation.eulerAngles);
            sensor.AddObservation(transformList[i].localRotation.eulerAngles);
        }
        sensor.AddObservation(getGameObject.transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        currentStep++;

        MoveUR3AllControl(actions);

        float distance = Vector3.Distance(termainal.position, getGameObject.transform.position);
        float score = CalculateScore(distance);
        AddReward(score);

        // if (distance < 0.02f)
        // {
        //     Debug.Log("Finish !");
        //     Terminal();
        // }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = heuristicAction;
    }

    public void Terminal()
    {
        RobotController robotController = UR3GameObject.GetComponent<RobotController>();
        robotController.StopAllJointRotations();
        SetReward(10f);
        floorMeshRenderer.material = winMaterial;
        EndEpisode();
    }

    private float CalculateScore(float distance)
    {
        float maxDistance = 1f;
        float maxScore = 0f;
        float minScore = -0.1f;
        float score = 0f;

        if (distance < maxDistance)
        {
            score = maxScore - ((distance / maxDistance) * (maxScore - minScore));
        }
        return score;
    }

    private void MoveUR3(ActionBuffers actions)
    {
        RobotController robotController = UR3GameObject.GetComponent<RobotController>();
        float actionValue = actions.DiscreteActions[0] - 7;
        if (actionValue == 0)
        {
            robotController.StopAllJointRotations();
            return;
        }
        int pickJoint = (int)Mathf.Abs(actionValue) - 1;
        RotationDirection direction = GetRotationDirection(actionValue);
        robotController.RotateJoint(pickJoint, direction);
    }

    static RotationDirection GetRotationDirection(float inputVal)
    {
        if (inputVal > 0)
        {
            return RotationDirection.Positive;
        }
        else if (inputVal < 0)
        {
            return RotationDirection.Negative;
        }
        else
        {
            return RotationDirection.None;
        }
    }

    private void OnDrawGizmos()
    {
        float size = spawnAreaSize;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(target_basic.position, Vector3.one * size * 2);
        //Gizmos.DrawWireCube(target_basic.position + new Vector3(0, -size, 0), new Vector3(size * 2, 0, size * 2));
    }

    private void MoveUR3AllControl(ActionBuffers actions)
    {
        RobotController robotController = UR3GameObject.GetComponent<RobotController>();
        for (int i = 0; i < 5; i++)
        {
            if (actions.DiscreteActions[i] == 0)
            {
                robotController.RotateJoint(i, RotationDirection.None);
            }
            else if(actions.DiscreteActions[i] == 1)
            {
                robotController.RotateJoint(i, RotationDirection.Positive);
            }
            else if (actions.DiscreteActions[i] == 2)
            {
                robotController.RotateJoint(i, RotationDirection.Negative);
            }
        }

        //夾取動作
        if (actions.DiscreteActions[6] == 0)
        {
            float distance = Vector3.Distance(termainal.position, getGameObject.transform.position);
            if (distance < 0.02f)
            {
                Debug.Log("Finish !");
                Terminal();
            }
            else
            {
                float score = CalculateScore(distance);
                AddReward(10 * score);
            }
        }
    }
}
