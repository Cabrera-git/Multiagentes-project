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
    public int o;
    public CarData(string id, float x, float z, int o)
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
    public LightData(string id, float x, float z, int state)
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
    string serverUrl = "http://localhost:8521";
    string getAgentsEndpoint = "/games";
    string getCityEndpoint = "/city";
    string sendConfigEndpoint = "/init";

    Dictionary<string, GameObject> agents;
    List<List<char>> matrix;

    CarsData carsData;
    LightsData lightsData;
    City city;

    public GameObject carPrefab, lightPrefab, cityObject,
            street_straight, street_empty, 
            street_leftturn, street_rightturn,
            building_normal, building_destination,
            lightpost_left, lightpost_right;
    public int cars, time;

    void Start()
    {
        agents = new Dictionary<string, GameObject>();
        carsData = new CarsData();
        lightsData = new LightsData();
        city = new City();
        matrix = new List<List<char>>();

        CityGen();

        StartCoroutine(SendConfiguration());
    }

    private void Update()
    {

    }

    private void CityGen()
    {
        foreach (string line in System.IO.File.ReadLines(@"./../MesaModels60/base.txt"))
        {  
            Debug.Log(line);
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

                    GameObject light = Instantiate(lightpost_left, new Vector3(i-0.5f, 0, j-0.5f), Quaternion.identity);
                }
                else if (matrix[i][j] == '⋝' || matrix[i][j] == '≤' || matrix[i][j] == '≥' || matrix[i][j] == '⋜')
                {
                    if(x == matrix.Count - 1)
                    {
                        
                    }

                    /*GameObject street = Instantiate(street_empty, new Vector3(i, 0, j), Quaternion.identity);
                    street.transform.parent = cityObject.transform;*/
                }
                else if (matrix[i][j] == 'D')
                {
                    float h = UnityEngine.Random.Range(1.0f,2.0f);
                    GameObject building = Instantiate(building_destination, new Vector3(i, 0, j), Quaternion.identity);
                    Vector3 scaling = new Vector3(1,h,1);
                    building.transform.localScale = Vector3.Scale(building.transform.localScale, scaling);
                    building.transform.parent = cityObject.transform;
                }
                else if (matrix[i][j] == '#')
                {
                    float h = UnityEngine.Random.Range(1.0f,2.0f);
                    GameObject building = Instantiate(building_normal, new Vector3(i, 0, j), Quaternion.identity);
                    Vector3 scaling = new Vector3(1,h,1);
                    building.transform.localScale = Vector3.Scale(building.transform.localScale, scaling);
                    building.transform.parent = cityObject.transform;
                }
                else
                {
                    Debug.Log("Welp");
                }
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
        WWWForm form = new WWWForm();

        form.AddField("cars", cars.ToString());
        form.AddField("timeLimit", time.ToString());

        UnityWebRequest www = UnityWebRequest.Get(serverUrl + sendConfigEndpoint);
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
            Debug.Log("Getting City layout");
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
        }
    }
}

// ----------------------------------------------------------------------------------------------------