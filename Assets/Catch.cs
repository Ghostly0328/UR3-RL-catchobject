using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Catch : Agent
{
    [Header("Training Setting")]
    [SerializeField] private bool isRotate = false;

    [Header("Inference")]
    [SerializeField] private bool isSlowDown = false;
    [SerializeField] private GameObject target;
    [SerializeField] private Transform grab;
    [SerializeField] private Transform target_basic;
    private GameObject getGameObject;

    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    [SerializeField] List<Transform> transformList = new List<Transform>();

    private int maxStep;
    private int currentStep;

    private void Start()
    {
        if (isSlowDown)
        {
            // inference slow down
            Time.timeScale = 0.1f;
        }
    }

    private void FixedUpdate()
    {
        if (getGameObject != null)
        {
            float distance = Vector3.Distance(grab.position, getGameObject.transform.position);
            if (distance < 0.02f)
            {
                Debug.Log("Finish!");
                Terminal();
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        // Get Information
        maxStep = MaxStep;  // Get MaxStep
        currentStep = 0;    // Reset CurrentStep

        if (getGameObject != null)
        {
            Destroy(getGameObject);
        }
        float spawnAreaSize = 0.2f;
        // Spawn objects
        float x = Random.Range(-spawnAreaSize, spawnAreaSize);
        float y = Random.Range(-spawnAreaSize, spawnAreaSize);
        float z = Random.Range(-spawnAreaSize, spawnAreaSize);
        Vector3 spawnPosition = new Vector3(target_basic.position.x + x, target_basic.position.y + y, target_basic.position.z + z);

        // Instantiate object at random position
        getGameObject = Instantiate(target, spawnPosition, Quaternion.identity, transform.parent);

        if (isRotate)
        {
            // Randomly rotate the object
            float Xangle = Random.Range(-90f, 90f);
            float Yangle = Random.Range(-90f, 90f);
            getGameObject.transform.Rotate(Vector3.up, Yangle);
            getGameObject.transform.Rotate(Vector3.right, Xangle);
        }

        for (int i = 0; i < transformList.Count; i++)
        {
            transformList[i].localRotation = Quaternion.Euler(Vector3.zero);
        }
        //transformList[0].Rotate(new Vector3(-90, 0, 0));
        //transformList[1].Rotate(new Vector3(90, 0, 45));
        //transformList[2].Rotate(new Vector3(0, 0, -45));
        //transformList[3].Rotate(new Vector3(0, -90, 0));
        transformList[0].Rotate(new Vector3(-90, 0, 0), Space.Self);
        transformList[1].Rotate(new Vector3(90, 0, 45), Space.Self);
        transformList[2].Rotate(new Vector3(0, 0, 90), Space.Self);
        transformList[3].Rotate(new Vector3(0, 0, -135), Space.Self);
        transformList[4].Rotate(new Vector3(0, -90, -90), Space.Self);
    } 
    public override void CollectObservations(VectorSensor sensor)
    {
        for (int i = 0; i < transformList.Count; i++)
        {
            sensor.AddObservation(transformList[i].localRotation.eulerAngles);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        currentStep++;

        for (int i = 0; i < transformList.Count; i++)
        {
            float newZ = transformList[i].rotation.eulerAngles.z + actions.ContinuousActions[i];
            transformList[i].rotation = Quaternion.Euler(transformList[i].rotation.eulerAngles.x, transformList[i].rotation.eulerAngles.y, newZ);
        }

        float distance = Vector3.Distance(grab.position, getGameObject.transform.position);
        float score = CalculateScore(distance);
        AddReward(score);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contionousActions = actionsOut.ContinuousActions;
        contionousActions[2] = Input.GetAxisRaw("Horizontal");
        contionousActions[3] = Input.GetAxisRaw("Vertical");
    }

    public void Terminal()
    {
        SetReward(10f);
        floorMeshRenderer.material = winMaterial;
        EndEpisode();
    }

    public void HitWall()
    {
        SetReward(-10f);
        floorMeshRenderer.material = loseMaterial;
        EndEpisode();
    }

    public void HitGrap()
    {
        SetReward(1f);
        floorMeshRenderer.material = loseMaterial;
        EndEpisode();
    }

    private float CalculateScore(float distance)
    {
        float maxDistance = 2f;
        float maxScore = 0f;
        float minScore = -0.1f;
        float score = 0f;

        //float maxScore = 0.1f;
        //float minScore = 0f;

        if (distance < maxDistance)
        {
            score = maxScore - ((distance / maxDistance) * (maxScore - minScore));
        }
        return score;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(target_basic.position, Vector3.one * 0.4f);
    }
}
