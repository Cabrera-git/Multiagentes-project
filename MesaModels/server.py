from mesa.visualization.ModularVisualization import ModularServer
from mesa.visualization.modules import CanvasGrid
from agents.car import Car
from agents.trafficLight import TrafficLight
from agents.city import City
from agents.cityObjects import Grass, Street

def agent_portrayal(agent):
    if type(agent) is Car:
        portrayal = {"Shape": "circle", "Filled": "true", "Color": "Blue", "r": 0.75, "Layer": 0}
    elif type(agent) is Grass:
        portrayal = {"Shape": "rect",  "w": 1, "h": 1, "Filled": "true", "Color": "Gray", "Layer": 0}
    elif type(agent) is TrafficLight:
        if(agent.estado == 0):
            portrayal = {"Shape": "rect",  "w": 1, "h": 1, "Filled": "true", "Color": "Red", "Layer": 0}
        elif(agent.estado == 1):
            portrayal = {"Shape": "rect",  "w": 1, "h": 1, "Filled": "true", "Color": "Yellow", "Layer": 0}
        elif(agent.estado == 2):
            portrayal = {"Shape": "rect",  "w": 1, "h": 1, "Filled": "true", "Color": "Green", "Layer": 0}
    return portrayal

if __name__ == '__main__':
    grid = CanvasGrid(agent_portrayal, 16, 16, 450, 450)

    server = ModularServer(City, [grid], "Reto unu", {})
    server.port = 8522
    server.launch()