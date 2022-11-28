from cloudant import Cloudant
from flask import Flask, render_template, request, jsonify
import atexit
import os
import json
from flask.json import jsonify
import uuid
from agents.car import Car
from agents.city import City
from agents.trafficLight import TrafficLight

cityModel = None

app = Flask(__name__, static_url_path='')
port=int(os.environ.get('PORT', 8000))

@app.route("/")
def root():
    return "ok"

@app.route("/init", methods=["POST"])
def create():
    global model
    model = City()
    return "ok", 201, {'Location': f"/model"}

@app.route("/city", methods=["GET"])
def queryCity():
    global model
    model = cityModel
    model.step()

    return jsonify({"city": model.matrix})

@app.route("/cars", methods=["GET"])
def queryStateCars():
    global model
    model = cityModel
    model.step()
    agents = model.schedule.agents
    
    listCars = []
    listLights = []

    for agent in agents:
        if(isinstance(agent, Car)):
            listCars.append({"id": agent.unique_id, "x": agent.pos[0], "z": agent.pos[1], "o": agent.orientacion})
        elif(isinstance(agent, TrafficLight)):
            listLights.append({"id": agent.unique_id, "x": agent.pos[0], "z": agent.pos[1], "state": agent.estado})

    return jsonify({"cars": listCars, "status": listLights})



if __name__ == '__main__':
    app.run(host='0.0.0.0', port=port)