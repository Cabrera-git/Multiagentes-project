using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// ----------------------------------------------------------------------------------------------------

[Serializable]
public class CarData
{
    public string id;
    public float x, z;
    public int o;
    public AgentData(string id, float x, float z, int o)
    {
        this.id = id;
        this.x = x;
        this.z = z;
        this.o = o;
    }
}

[Serializable]
public class CarsData
{
    public List<CarData> cars;
    public CarsData() => this.cars = new List<CarData>();
}

// ----------------------------------------------------------------------------------------------------

[Serializable]
public class LightData
{
    public string id;
    public float x, z;
    public int state;
    public AgentData(string id, float x, float z, int state)
    {
        this.id = id;
        this.x = x;
        this.z = z;
        this.state = state;
    }
}

[Serializable]
public class LightsData
{
    public List<LightData> status;
    public LightsData() => this.status = new List<LightData>();
}

// ----------------------------------------------------------------------------------------------------

[Serializable]
public class City
{
    public List<List<int>> city;
    public City() => this.city = new List<List<int>>();
}

// ----------------------------------------------------------------------------------------------------

public class AgentController : MonoBehaviour
{
    string serverUrl = "http://localhost:8522";
    string getAgentsEndpoint = "/games";
    string getCityEndpoint = "/city";
    string sendConfigEndpoint = "/init";

    Dictionary<string, GameObject> agents;

    CarsData carsData;
    LightsData lightsData;
    City city;

    public GameObject carPrefab, lightPrefab, cityObject,
            street_straight, street_innercorner, 
            street_outercorner, street_cross,
            building;

    void Start()
    {
        agents = new Dictionary<string, GameObject>();
        carsData = new CarsData();
        lightsData = new LightsData();
        city = new City();

        timer = timeToUpdate;

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
        }
    }

    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getAgentsEndpoint);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            StartCoroutine(GetAgentsData());
        }
    }

    IEnumerator SendConfiguration()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + sendConfigEndpoint);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Satrted city model!");
            Debug.Log("Getting Agents positions");
            StartCoroutine(GetAgentsData());
            Debug.Log("Getting City layout");
            StartCoroutine(GetCityData());
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
            carsData = JsonUtility.FromJson<CarsData>(www.downloadHandler.text);
            lightsData = JsonUtility.FromJson<LightsData>(www.downloadHandler.text);

            foreach (var car in carsData.cars)
            {
                if (!agents.ContainsKey(car.id))
                {
                    GameObject agent = Instantiate(carPrefab, new Vector3(car.x, 0, car.z), Quaternion.identity);
                    agents.Add(car.id, agent);
                }
                else
                {
                    agents[car.id].transform.localPosition = new Vector3(car.x, 0, car.z);
                }
            }

            foreach (var light in lightsData.status)
            {
                if (!agents.ContainsKey(light.id))
                {
                    GameObject agent = Instantiate(lightPrefab, new Vector3(light.x, 2, light.z), Quaternion.identity);
                    agents.Add(light.id, agent);
                }
                else
                {
                    // add light change using 'light.status' parameter
                }
            }
        }
    }

    IEnumerator GetCityData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCityEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            city = JsonUtility.FromJson<City>(www.downloadHandler.text);
            Debug.Log("City layout received!");

            int x = 0, z;

            foreach (var row in city.city) // each x
            {
                z = 0;

                foreach (var cell in row) // each z
                {
                    if((x == 0 && z == 0) || (x == 15 && z == 0) || (x == 0 && z == 15) || (x == 15 && z == 15))
                    {
                        GameObject street = Instantiate(street_outercorner, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                        break;
                    }
                    if((x == 1 && z == 1) || (x == 14 && z == 1) || (x == 1 && z == 14) || (x == 14 && z == 14))
                    {
                        GameObject street = Instantiate(street_innercorner, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                        break;
                    }
                    if (cell == 1)
                    {
                        GameObject street = Instantiate(street_straight, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 2)
                    {
                        GameObject street = Instantiate(street_straight, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 3)
                    {
                        GameObject street = Instantiate(street_straight, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 4)
                    {
                        GameObject street = Instantiate(street_straight, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 5)
                    {
                        GameObject street = Instantiate(street_cross, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 6)
                    {
                        GameObject street = Instantiate(street_cross, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 7)
                    {
                        GameObject street = Instantiate(street_cross, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    if (cell == 8)
                    {
                        GameObject street = Instantiate(street_cross, new Vector3(x, 0, z), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                        z++;
                    }
                    else
                    {
                        GameObject streetn_t = Instantiate(building, new Vector3(x, 0, z), Quaternion.identity);
                        streetn_t.transform.parent = cityObject.transform;
                        z++;
                    }
                }

                x++;
            }
        }
    }
}

// ----------------------------------------------------------------------------------------------------