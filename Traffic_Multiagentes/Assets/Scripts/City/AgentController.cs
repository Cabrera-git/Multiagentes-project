using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// ----------------------------------------------------------------------------------------------------

[Serializable]
public class CarData
{
    public string id;
    public float x, z;
    public int directionLight;
    public bool isParked, arrived;
    public CarData(string id, float x, float z, int directionLight, bool isParked, bool arrived)
    {
        this.id = id;
        this.x = x;
        this.z = z;
        this.directionLight = directionLight;
        this.isParked = isParked;
        this.arrived = arrived;
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
    public float x, z;
    public string state;
    public int direction;
    public LightData(float x, float z, string state, int direction)
    {
        this.x = x;
        this.z = z;
        this.state = state;
        this.direction = direction;
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
    string serverUrl = "http://localhost:8585";
    string getAgentsEndpoint = "/cars";
    string getCityEndpoint = "/city";
    string sendConfigEndpoint = "/init";
    string sendUpdateEndpoint = "/update";

    Dictionary<string, GameObject> agents;
    
    public List<GameObject> carModels;
    List<List<char>> matrix;

    CarsData carsData;
    LightsData lightsData;
    City city;

    public GameObject carPrefab, lightPrefab,
            street_straight, street_empty, 
            street_leftturn, street_rightturn,
            building_normal, building_destination,
            lightpost_left, lightpost_right,
            carsObject, cityObject, lightObject;
    public int cars, time;

    void Start()
    {
        agents = new Dictionary<string, GameObject>();
        carsData = new CarsData();
        lightsData = new LightsData();
        city = new City();
        matrix = new List<List<char>>();

        Debug.Log("Getting City layout");
        CityGen();

        StartCoroutine(SendConfiguration());
    }

    private void Update()
    {
        StartCoroutine(UpdateSimulation());
    }

    private void CityGen()
    {
        foreach (string line in System.IO.File.ReadLines(@"./../MesaModels60/layouts/base.txt"))
        {  
            List<char> lst = new List<char>();
            lst = line.ToCharArray().ToList();                       
            matrix.Add(lst);
        }
        for (int i = 0; i < matrix.Count; i++)
        {
            for (int j = 0; j < matrix[i].Count; j++)
            {
                if (matrix[i][j] == '<' || matrix[i][j] == '>' || matrix[i][j] == 'v' || matrix[i][j] == '^')
                {
                    GameObject street;
                    if (matrix[i][j] == '<' || matrix[i][j] == '>')
                        street = Instantiate(street_straight, new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, -90, 0)));
                    else
                        street = Instantiate(street_straight, new Vector3(i, 0, j), Quaternion.identity);
                    street.transform.parent = cityObject.transform;
                }
                else if (matrix[i][j] == 'Û' || matrix[i][j] == 'Ǔ' || matrix[i][j] == 'ù' || matrix[i][j] == 'ú')
                {
                    GameObject street = Instantiate(street_empty, new Vector3(i, 0, j), Quaternion.identity);
                    street.transform.parent = cityObject.transform;

                    GameObject light = Instantiate(lightpost_left, new Vector3(i, 1.5f, j), Quaternion.Euler(new Vector3(180, 0, 0)));
                    light.transform.parent = lightObject.transform;
                }
                else if (matrix[i][j] == '⋝' || matrix[i][j] == '≤' || matrix[i][j] == '≥' || matrix[i][j] == '⋜')
                {
                    GameObject street;

                    if(i == 0 || i == 1 || i == 24 || i == 25)
                    {
                        if(j != 1 || j != 24)
                        {
                            street = Instantiate(street_straight, new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, -90, 0)));
                            street.transform.parent = cityObject.transform;
                        }
                        else
                        {
                            street = Instantiate(street_straight, new Vector3(i, 0, j), Quaternion.identity);
                            street.transform.parent = cityObject.transform;
                        }
                    }
                    else if(j == 0 || j == 1 || j == 24 || j == 25)
                    {
                        if(i != 1 || i != 24)
                        {
                            street = Instantiate(street_straight, new Vector3(i, 0, j), Quaternion.identity);
                            street.transform.parent = cityObject.transform;
                        }
                        else
                        {
                            street = Instantiate(street_straight, new Vector3(i, 0, j), Quaternion.Euler(new Vector3(0, -90, 0)));
                            street.transform.parent = cityObject.transform;
                        }
                    }
                    else
                    {
                        street = Instantiate(street_empty, new Vector3(i, 0, j), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                    }
                }
                else if (matrix[i][j] == 'D')
                {                    
                    float h = UnityEngine.Random.Range(1.0f,2.0f);
                    GameObject building = Instantiate(building_destination, new Vector3(i+.07f, 0, j+ .47f), Quaternion.identity);
                    Vector3 scaling = new Vector3(1,h,1);
                    building.transform.localScale = Vector3.Scale(building.transform.localScale, scaling);
                    building.transform.parent = cityObject.transform;
                }
                else if (matrix[i][j] == '#')
                {                    
                    float h = UnityEngine.Random.Range(1.0f,2.0f);
                    GameObject building = Instantiate(building_normal, new Vector3(i-0.48f, 0, j+0.22f), Quaternion.identity);
                    Vector3 scaling = new Vector3(1,h,1);
                    building.transform.localScale = Vector3.Scale(building.transform.localScale, scaling);
                    building.transform.parent = cityObject.transform;
                }
                else
                {
                    Debug.Log("Unknown character");
                }
            }
        }
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("cars", cars.ToString());
        form.AddField("timeLimit", time.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint,form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Started city model!");
            Debug.Log("Getting Agents positions");
            StartCoroutine(GetAgentsData());
        }
    }

    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getUpdateEndpoint);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            StartCoroutine(GetAgentsData());
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
            GenerateCars();
        }
    }
    
    void GenerateCars()
    {
        for (int i = 0; i < carsData.cars.Count(); i++)
        {          
            if (!agents.ContainsKey(carsData.cars[i].id))
            {
                GameObject car = Instantiate(carModels[UnityEngine.Random.Range(0, 4)], new Vector3(cData.cars[i].x, 0, cData.cars[i].z), Quaternion.identity);
                agents.Add(carsData.cars[i].id, car);
                car.transform.parent = carsObject.transform;
                car.tag = "Car";
            }
            if (carsData.cars[i].arrived)
            {
                Destroy(agents[carsData.cars[i].id]);
                agents.Remove(carsData.cars[i].id);
            }
        }
    }
}

// ----------------------------------------------------------------------------------------------------