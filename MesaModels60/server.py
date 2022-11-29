from agent import *
from model import City
from mesa.visualization.modules import CanvasGrid
from mesa.visualization.ModularVisualization import ModularServer
from pathlib import Path

def agent_portrayal(agent):
    if agent is None: return
    
    portrayal = {"Shape": "rect",
                 "Filled": "true",
                 "Layer": 1,
                 "w": 1,
                 "h": 1
                 }

    if (isinstance(agent, Road)):
        portrayal["Color"] = "grey"
        portrayal["Layer"] = 0
    
    if (isinstance(agent, Destination)):
        portrayal["Color"] = "lightgreen"
        portrayal["Layer"] = 0

    if (isinstance(agent, Traffic_Light)):
        portrayal["Color"] = agent.state
        portrayal["Layer"] = 0
        portrayal["w"] = 0.8
        portrayal["h"] = 0.8

    if (isinstance(agent, Obstacle)):
        portrayal["Color"] = "cadetblue"
        portrayal["Layer"] = 0
        portrayal["w"] = 0.8
        portrayal["h"] = 0.8

    if (isinstance(agent, Car)):
        portrayal["Color"] = "darkorange"
        portrayal["Shape"] = "circle"
        portrayal["Layer"] = 1
        portrayal["r"] = 0.5
    return portrayal

width = 0
height = 0

basePath = Path("layouts/base2.txt")

with open(basePath) as baseFile: 
    lines = baseFile.readlines()
    width = len(lines[0])-1
    height = len(lines)

model_params = {"N":1}

grid = CanvasGrid(agent_portrayal, width, height, 500, 500)

server = ModularServer(City, [grid], "Traffic Base", model_params)
                       
server.port = 8521 # The default
server.launch()
