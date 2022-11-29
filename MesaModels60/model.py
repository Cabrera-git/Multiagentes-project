from mesa import Model
from mesa.time import SimultaneousActivation
from mesa.space import MultiGrid
from agent import *
from a_star import AStar
from pathlib import Path
import json


class City(Model):
    """ 
    Creates a new model with random agents.
    Args:
        N: Number of agents in the simulation
        height, width: The size of the grid to model
    """
    def __init__(self, N):

        dataDictionary = json.load(open(Path("layouts/mapDictionary.json"))) # Change Path

        destinations = []
        maze = []

        basePath = Path("layouts/base2.txt")

        with open(basePath) as baseFile: # Change Path
            lines = baseFile.readlines()
            self.width = len(lines[0])-1
            self.height = len(lines)

            self.grid = MultiGrid(self.width, self.height,torus = False) 
            self.schedule = SimultaneousActivation(self)

            for r, row in enumerate(lines):
                maze.append(list(row)[:-1])
                for c, col in enumerate(row):
                    if col in ["v", "^", ">", "<", "≤", "⋜", "≥", "⋝"]:
                        agent = Road(f"r{r*self.width+c}", self, dataDictionary[col])
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                        self.schedule.add(agent)
                    elif col in ["ú", "ù", "Û", "Ǔ"]:
                        agent = Traffic_Light(f"tl{r*self.width+c}", self, col, dataDictionary[col])
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                        self.schedule.add(agent)
                    elif col == "#":
                        agent = Obstacle(f"ob{r*self.width+c}", self)
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                        self.schedule.add(agent)
                    elif col == "D":
                        agent = Destination(f"d{r*self.width+c}", self)
                        self.grid.place_agent(agent, (c, self.height - r - 1))
                        destinations.append((c, self.height - r - 1))
                        self.schedule.add(agent)

        a_star = AStar(maze)
        # Add cars with their destinations
        for i in range(N):
            cell_type = None
            x = 0
            y = 0
            # Place car in road and where there aren't other cars
            while not isinstance(cell_type, Road) or len(self.grid[x][y]) > 1:
                x = self.random.randint(0, self.width-1)
                y = self.random.randint(0, self.height-1)
                cell_type = self.grid[x][y][0]
            
            fate = self.random.choice(destinations)
            route = a_star.search(1, (x,y), fate)
            if route == None:
                print("No route found")
                continue

            agent = Car(f"c{i}", self, fate, route)
            self.grid.place_agent(agent, (x,y))
            self.schedule.add(agent)
            agent.assignDirection()
            print(f"Car {i} assigned to {fate}")
           

        self.running = True 

    def step(self):
        '''Advance the model by one step.'''
        self.schedule.step()

        # Change stoplights every ten steps
        if self.schedule.steps % 10 == 0:
            for agent in self.schedule.agents:
                if isinstance(agent, Traffic_Light):
                    # Change reds to green
                    if agent.state == "Red":
                        agent.state = "Green"
                    # Change yellows to red
                    else:
                        agent.state = "Red"

        # Change green lights to yellow
        elif self.schedule.steps % 10 == 8:
            for agent in self.schedule.agents:
                if isinstance(agent, Traffic_Light):
                    if agent.state == "Green":
                        agent.state = "Yellow"
        
        # Stop model when all cars are parked
        count = 0
        for agent in self.schedule.agents:
            if isinstance(agent, Car) and not agent.is_parked:
                count += 1
        
        if count == 0:
            self.running = False