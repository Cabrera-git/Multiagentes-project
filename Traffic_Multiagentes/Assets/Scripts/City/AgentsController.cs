using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


[Serializable]
public class AgentData
{
    public string id;
    public float x, y, z;

    public AgentData(string id, float x, float y, float z)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
    }

}


[Serializable]
public class AgentsData
{
    public List<AgentData> positions;

    public AgentsData() => this.positions = new List<AgentData>();
}

public class AgentController : MonoBehaviour
{
    // private string url = "https://agents.us-south.cf.appdomain.cloud/";
    string serverUrl = "http://localhost:8522";
    string getAgentsEndpoint = "/games";
    string getObstaclesEndpoint = "/";
    string getBoxesEndpoint = "/getBoxes";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    string dropEndpoint = "/drop";
    AgentsData agentsData, obstacleData, boxData, dropData;

    Dictionary<string, GameObject> agents;
    Dictionary<string, Vector3> prevPositions, currPositions;
    Dictionary<string, bool> dropped;
    List<List<float>> dropoff;

    bool updated = false, started = false, initialized = false;

    public GameObject agentPrefab, obstaclePrefab, boxPrefab, floor, daddy;
    public int NAgents, width, height, boxes, executionSteps;
    public float timeToUpdate = 5.0f;
    private float timer, dt;
    public GameObject[] all_boxes;
    public Transform[] boxes_ids;

    void Start()
    {
        agentsData = new AgentsData();
        obstacleData = new AgentsData();
        dropData = new AgentsData();
        boxData = new AgentsData();
        randomsData = new RandomsData();
        dropoff = new List<List<float>>();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();
        dropped = new Dictionary<string, bool>();

        agents = new Dictionary<string, GameObject>();

        floor.transform.localScale = new Vector3((float)width / 10, 1, (float)height / 10);
        floor.transform.localPosition = new Vector3((float)width / 2 - 0.5f, 0, (float)height / 2 - 0.5f);

        timer = timeToUpdate;

        daddy = GameObject.Find("BokkSUS");

        StartCoroutine(SendConfiguration());
    }

    private void Update()
    {
        boxes_ids = daddy.transform.GetComponentsInChildren<Transform>(true);
        all_boxes = GameObject.FindGameObjectsWithTag("Box");
        if (timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }

        if (updated)
        {
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);

            foreach (var agent in currPositions)
            {
                Vector3 currentPosition = agent.Value;
                Vector3 previousPosition = prevPositions[agent.Key];

                Vector3 interpolated = Vector3.Lerp(previousPosition, currentPosition, dt);
                Vector3 direction = currentPosition - interpolated;

                agents[agent.Key].transform.localPosition = interpolated;
                if (direction != Vector3.zero) agents[agent.Key].transform.rotation = Quaternion.LookRotation(new Vector3(-direction.x, direction.y, -direction.z));
            }

            // float t = (timer / timeToUpdate);
            // dt = t * t * ( 3f - 2f*t);
        }
    }

    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            StartCoroutine(GetAgentsData());
            StartCoroutine(GetBoxData());
        }
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("NAgents", NAgents.ToString());
        form.AddField("width", width.ToString());
        form.AddField("height", height.ToString());
        form.AddField("boxes", boxes.ToString());
        form.AddField("execution", executionSteps.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Configuration upload complete!");
            Debug.Log("Getting Agents positions");
            StartCoroutine(GetAgentsData());
            StartCoroutine(GetObstacleData());
            StartCoroutine(GetBoxData());
            StartCoroutine(GetDropoff());
        }
    }

    IEnumerator GetAgentsData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getAgentsEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            randomsData = JsonUtility.FromJson<RandomsData>(www.downloadHandler.text);

            foreach (RandomData agent in randomsData.positions)
            {
                Vector3 newAgentPosition = new Vector3(agent.x, agent.y, agent.z);

                if (!started)
                {
                    prevPositions[agent.id] = newAgentPosition;
                    agents[agent.id] = Instantiate(agentPrefab, newAgentPosition, Quaternion.identity);
                }
                else
                {
                    Vector3 currentPosition = new Vector3();
                    if (currPositions.TryGetValue(agent.id, out currentPosition))
                        prevPositions[agent.id] = currentPosition;
                    currPositions[agent.id] = newAgentPosition;
                }

                if (agent.carrying)
                {
                    agents[agent.id].transform.GetChild(0).gameObject.SetActive(true);

                    foreach (Transform go in boxes_ids)
                    {
                        if (go.name == agent.box)
                        {
                            go.gameObject.SetActive(false);
                        }
                    }
                }
                else if (agent.prior != null)
                {
                    agents[agent.id].transform.GetChild(0).gameObject.SetActive(false);

                    foreach (Transform go in boxes_ids)
                    {
                        List<float> min_v = new List<float>() { 0, 0 };

                        if (go.name == agent.prior && !dropped[agent.prior])
                        {
                            float min_dist = 999999.0f;
                            foreach (var t in dropoff)
                            {
                                float dist = Vector3.Distance(new Vector3(t[0], 0, t[1]), new Vector3(agent.x, 0, agent.z));
                                if (dist < min_dist)
                                {
                                    min_dist = dist;
                                    min_v = t;
                                }
                            }

                            go.position = new Vector3(min_v[0], agent.y, min_v[1]);
                            go.gameObject.SetActive(true);
                            dropped[agent.prior] = true;
                        }
                    }
                }
            }

            updated = true;
            if (!started) started = true;
        }
    }

    IEnumerator GetObstacleData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getObstaclesEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            obstacleData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            Debug.Log(obstacleData.positions);

            foreach (AgentData obstacle in obstacleData.positions)
            {
                Instantiate(obstaclePrefab, new Vector3(obstacle.x, obstacle.y, obstacle.z), Quaternion.identity);
            }
        }
    }

    IEnumerator GetDropoff()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + dropEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            dropData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach (AgentData drop in dropData.positions)
            {
                List<float> john = new List<float>() { drop.x, drop.z }; // if you can't tell, i've given up on life and am just naming variables john <-- this post was sponsored by GitHub Copilot
                dropoff.Add(john);
            }
        }
    }

    IEnumerator GetBoxData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getBoxesEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            boxData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            if (!initialized)
            {
                foreach (AgentData box in boxData.positions)
                {
                    var temp = Instantiate(boxPrefab, new Vector3(box.x, box.y, box.z), Quaternion.identity);
                    temp.transform.parent = daddy.transform;
                    temp.name = box.id;
                    dropped[box.id] = false;
                }

                initialized = true;
            }
        }
    }
}
