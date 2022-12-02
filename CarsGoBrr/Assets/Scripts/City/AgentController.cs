using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// ----------------------------------------------------------------------------------------------------

[Serializable]
public class CarData
{
    public string id, direction;
    public float x, z;
    public bool isParked, arrived;
    public CarData(string id, float x, float z, string direction, bool isParked, bool arrived)
    {
        this.id = id;
        this.x = x;
        this.z = z;
        this.direction = direction;
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
    public string id;
    public float x, z;
    public string state;
    public int direction;
    public LightData(string id, float x, float z, string state, int direction)
    {
        this.id = id;
        this.x = x;
        this.z = z;
        this.state = state;
        this.direction = direction;
    }
}

[Serializable]
public class LightsData
{
    public List<LightData> trafficLights;
    public LightsData() => this.trafficLights = new List<LightData>();
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
    string sendConfigEndpoint = "/init";
    string sendUpdateEndpoint = "/update";
    string getLightsEndpoint = "/trafficlights";

    Dictionary<string, GameObject> agents;
    public List<GameObject> carModels;
    List<List<char>> matrix;

    CarsData carsData;
    LightsData trafficLightsStates;
    City city;
    public float timeToUpdate;
    private float timer, dt;
    private bool updated = false, updatedL = false;
    public Dictionary<string, float[]> carPositions;
    public int currentCars;
    public GameObject carPrefab, lightPrefab,
            street_straight, street_empty, 
            street_leftturn, street_rightturn,
            building_normal, building_destination,
            lightpost,
            carsObject, cityObject, lightObject;
    public int cars, time;

    void Start()
    {
        agents = new Dictionary<string, GameObject>();
        carsData = new CarsData();
        trafficLightsStates = new LightsData();
        city = new City();
        matrix = new List<List<char>>();
        timer = timeToUpdate;
        carPositions = new Dictionary<string, float[]>();
        Debug.Log("Getting City layout");
        CityGen();

        StartCoroutine(SendConfiguration());
    }

    private void Update()
    {
        if(timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }
        if(updated && updatedL)
        {            
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);
            GenerateCars();
            foreach(CarData broom in carsData.cars)
            {
                agents[broom.id].transform.position = Vector3.Lerp(agents[broom.id].transform.position, new Vector3(broom.x, 0, broom.z), 0.7f);

                if(broom.direction == "Up")
                {
                    agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0,0,0), 0.7f);
                }
                else if(broom.direction == "Down")
                {
                    agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0,180,0), 0.7f);
                }
                else if(broom.direction == "Left")
                {
                     agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0,-90,0), 0.7f);
                }
                else if(broom.direction == "Right")
                {
                     agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0,90,0), 0.7f);
                }

                
                
              
                /*
                if(broom.x == broom.dx && broom.z == broom.dz)
                {
                    if(agents[broom.id].transform.position.x == broom.x + 1 && agents[broom.id].transform.position.z == broom.z) // [1,0]
                    {
                        agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, 90, 0), 0.7f);
                        continue;
                    }
                    else if(agents[broom.id].transform.position.x == broom.x - 1 && agents[broom.id].transform.position.z == broom.z) // [-1,0]
                    {
                        agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, -90, 0), 0.7f);
                        continue;
                    }
                    else if(agents[broom.id].transform.position.x == broom.x && agents[broom.id].transform.position.z == broom.z + 1) // [0,1]
                    {
                        agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, 180, 0), 0.7f);
                        continue;
                    }
                    else if(agents[broom.id].transform.position.x == broom.x && agents[broom.id].transform.position.z == broom.z - 1) // [0,-1]
                    {
                        agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, 0, 0), 0.7f);
                        continue;
                    }
                }

                place = matrix[(int)broom.x][(int)broom.z];

                if(place == 'v' || place == 'Ǔ')
                {
                    agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, 0, 0), 0.7f);
                }
                else if(place == '^' || place == 'Û')
                {
                    agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, 180, 0), 0.7f);
                }
                else if(place == '<' || place == 'ù')
                {
                    agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, 90, 0), 0.7f);
                }
                else if(place == '>' || place == 'ú')
                {
                    agents[broom.id].transform.rotation = Quaternion.Lerp(agents[broom.id].transform.rotation, Quaternion.Euler(0, -90, 0), 0.7f);
                }
                else
                {
                    agents[broom.id].transform.rotation = agents[broom.id].transform.rotation;
                }
                */
            }
            
            foreach(LightData light in trafficLightsStates.trafficLights)
            {
                if(light.state == "Green")
                {
                    agents[light.id].transform.GetChild(1).GetComponent<Light>().color = Color.green;
                }
                else if(light.state == "Red")
                {
                    agents[light.id].transform.GetChild(1).GetComponent<Light>().color = Color.red;
                }
                else if(light.state == "Yellow")
                {
                    agents[light.id].transform.GetChild(1).GetComponent<Light>().color = Color.yellow;
                }
            }

        }
    }

    private void CityGen()
    {
        foreach (string line in System.IO.File.ReadLines(@"./../MesaModels/layouts/base2.txt"))
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
                        street = Instantiate(street_straight, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.identity);
                    else
                        street = Instantiate(street_straight, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.Euler(new Vector3(0, -90, 0)));
                    street.transform.parent = cityObject.transform;
                }
                else if (matrix[i][j] == 'Û' || matrix[i][j] == 'Ǔ' || matrix[i][j] == 'ù' || matrix[i][j] == 'ú')
                {
                    GameObject street = Instantiate(street_empty, new Vector3(j, -0.0231f, matrix.Count - 1 - i), Quaternion.identity);
                    street.transform.parent = cityObject.transform;
                }
                else if (matrix[i][j] == '⋝' || matrix[i][j] == '≤' || matrix[i][j] == '≥' || matrix[i][j] == '⋜')
                {
                    GameObject street;

                    if(i == 0 || i == 1 || i == 24 || i == 25)
                    {
                        if(j != 1 || j != 24)
                        {
                            street = Instantiate(street_straight, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.identity);
                            street.transform.parent = cityObject.transform;
                        }
                        else
                        {
                            street = Instantiate(street_straight, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.Euler(new Vector3(0, -90, 0)));
                            street.transform.parent = cityObject.transform;
                        }
                    }
                    else if(j == 0 || j == 1 || j == 24 || j == 25)
                    {
                        if(i != 1 || i != 24)
                        {
                            street = Instantiate(street_straight, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.Euler(new Vector3(0, -90, 0)));
                            street.transform.parent = cityObject.transform;
                        }
                        else
                        {
                            street = Instantiate(street_straight, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.identity);
                            street.transform.parent = cityObject.transform;
                        }
                    }
                    else
                    {
                        street = Instantiate(street_empty, new Vector3(j, -0.0231f, matrix.Count - 1 - i), Quaternion.identity);
                        street.transform.parent = cityObject.transform;
                    }
                }
                else if (matrix[i][j] == 'D')
                {                    
                    float a = UnityEngine.Random.Range(1.0f,2.0f);
                    GameObject building = Instantiate(building_destination, new Vector3(j, 0, matrix.Count - 1 - i), Quaternion.identity);
                    Vector3 scaling = new Vector3(1,a,1);
                    building.transform.localScale = Vector3.Scale(building.transform.localScale, scaling);
                    building.transform.parent = cityObject.transform;

                    char[] side = {matrix[i+1][j],matrix[i-1][j],matrix[i][j+1],matrix[i][j-1]};

                    for(int h = 0; h < 4; h++)
                    {
                        if( side[h] == '⋝' || side[h] == '≤' || side[h] == '≥' || side[h] == '⋜' || 
                            side[h] == 'Û' || side[h] == 'Ǔ' || side[h] == 'ù' || side[h] == 'ú' || 
                            side[h] == '<' || side[h] == '>' || side[h] == 'v' || side[h] == '^')
                        {
                            if(h == 0)
                            {
                                building.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                                building.transform.position = new Vector3(building.transform.position.x - 0.1f, 0, building.transform.position.z - 0.47f);
                                break;
                            }
                            else if(h == 1)
                            {
                                building.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                                building.transform.position = new Vector3(building.transform.position.x + 0.09f, 0, building.transform.position.z + 0.47f);
                                break;
                            }
                            else if(h == 2)
                            {
                                building.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                                building.transform.position = new Vector3(building.transform.position.x + 0.47f, 0, building.transform.position.z - 0.1f);
                                break;
                            }
                            else if(h == 3)
                            {
                                building.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                                building.transform.position = new Vector3(building.transform.position.x - 0.47f, 0, building.transform.position.z + 0.1f);
                                break;
                            }
                        }
                    }
                }
                else if (matrix[i][j] == '#')
                {                    
                    float a = UnityEngine.Random.Range(1.0f,2.0f);
                    GameObject building = Instantiate(building_normal, new Vector3(j-0.48f, 0, matrix.Count - 1 - i + 0.22f), Quaternion.identity);
                    Vector3 scaling = new Vector3(1,a,1);
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
            Debug.Log("Getting Lights trafficLights");
            StartCoroutine(GetLightsData());
        }
    }

    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + sendUpdateEndpoint);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            StartCoroutine(GetAgentsData());
            StartCoroutine(GetLightsData());
        }
    }
    
    IEnumerator GetLightsData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getLightsEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            trafficLightsStates = JsonUtility.FromJson<LightsData>(www.downloadHandler.text);
            addLightAgents();
            updatedL = true;
        }

    }

    void addLightAgents()
    {
        for (int i = 0; i < trafficLightsStates.trafficLights.Count(); i++)
        {          
            if (!agents.ContainsKey(trafficLightsStates.trafficLights[i].id))
            {
                GameObject light = Instantiate(lightpost, new Vector3(trafficLightsStates.trafficLights[i].x, 1.5f, trafficLightsStates.trafficLights[i].z), Quaternion.Euler(new Vector3(180, 0, 0)));
                agents.Add(trafficLightsStates.trafficLights[i].id, light);
                light.transform.parent = lightObject.transform;
                light.tag = "LightP";
/*
                char place;
                place = matrix[(int)trafficLightsStates.trafficLights[i].x][(int)trafficLightsStates.trafficLights[i].z];

                if(place == 'Ǔ')
                {
                    light.transform.rotation = Quaternion.Euler(180, 90, 0);                        
                }
                else if(place == 'Û')
                {
                    light.transform.rotation = Quaternion.Euler(180, -90, 0);
                }
                else if(place == 'ù')
                {
                    light.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                else if(place == 'ú')
                {
                    light.transform.rotation = Quaternion.Euler(0, 180, 180);
                }
                */
            }
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
            updated = true;
        }
    }
    
    void GenerateCars()
    {
        for (int i = 0; i < carsData.cars.Count(); i++)
        {          
            if (!agents.ContainsKey(carsData.cars[i].id))
            {
                currentCars = carsData.cars.Count();
                GameObject car = Instantiate(carModels[UnityEngine.Random.Range(0, 4)], new Vector3(carsData.cars[i].x, 0, carsData.cars[i].z), Quaternion.identity);
                agents.Add(carsData.cars[i].id, car);
                car.transform.parent = carsObject.transform;
                car.tag = "Car";
            }
        }
    }

    /* 
    public GameObject FindClosestTrafficLight(float x,float y,float z)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("LightP");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = new Vector3(x,y,z);
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    } 
    */
}

// ----------------------------------------------------------------------------------------------------