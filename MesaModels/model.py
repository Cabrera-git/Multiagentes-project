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

        dataDictionary = json.load(open(Path("layouts/mapDictionary.json"),encoding='utf-8')) # Change Path

        self.destinations = []
        maze = []

        basePath = Path("layouts/base2.txt")

        with open(basePath,encoding='utf-8') as baseFile: # Change Path
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
                        self.destinations.append((c, self.height - r - 1))
                        self.schedule.add(agent)


        # Set spawn points at the corners of the grid
        self.spawns = [(0, self.height-2), (self.width - 1, 0)]
        self.a_star = AStar(maze)   
        self.running = True 
        self.num_agents = N

    def step(self):
        '''Advance the model by one step.'''
        self.schedule.step()

        cars_in_model = sum([1 if isinstance(test_agent, Car) else 0 for test_agent in self.schedule.agents])
        cars = 0
        # Spawn a car, unless there are more than num_agents
        if cars_in_model < self.num_agents and self.schedule.steps % 4 == 0:
            spawn_point = self.spawns[cars_in_model % 2]
            # Check if there is a car at the spawn point
            cars_in_spawn = sum([1 if isinstance(test_agent, Car) else 0 for test_agent in self.grid.get_cell_list_contents(spawn_point)])
            if cars_in_spawn == 0:
                fate = self.random.choice(self.destinations)
                route = self.a_star.search(1, spawn_point, fate)
                if route == None:
                    print(f"No route found when going to {fate}")
                else:
                    agent = Car(f"c{self.schedule.steps}", self, fate, route)
                    self.grid.place_agent(agent, spawn_point)
                    self.schedule.add(agent)
                    agent.assignDirection()
                    print(f"Car {self.schedule.steps} assigned to {fate}")
                    cars += 1
        
        # Change stoplights every ten steps
        if self.schedule.steps % 10 == 8:
            for agent in self.schedule.agents:
                if isinstance(agent, Traffic_Light):
                    # Change reds to green
                    if agent.state == "Red":
                        agent.state = "Green"
                    # Change yellows to red
                    else:
                        agent.state = "Red"

        # Change green lights to yellow
        elif self.schedule.steps % 8 == 0:
            for agent in self.schedule.agents:
                if isinstance(agent, Traffic_Light):
                    if agent.state == "Green":
                        agent.state = "Yellow"
        
        # Stop model when 500 steps are reached
        if self.schedule.steps >= 500:
            self.running = False
