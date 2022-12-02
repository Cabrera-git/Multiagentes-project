from mesa import Agent

class Traffic_Light(Agent):
    """
    Obstacle agent. Just to add obstacles to the grid.
    """
    def __init__(self, unique_id, model, letter, direction):
        super().__init__(unique_id, model)
        # Possible states: Red, Yellow, Green
        # Red if uppercase, green if lowercase
        if letter.isupper():
            self.state = "Red"
        else:
            self.state = "Green"
        self.directions = direction
    
    def step(self):
        pass

class Car(Agent):
    """
    Agent that moves according to destination as calculated by A*.
    """
    def __init__(self, unique_id, model, destination, route):
        """
        Creates a new random agent.
        Args:
            unique_id: The agent's ID
            model: Model reference for the agent
            destination: Randomly chosen destination from map
            route: Path to take from original position to destination
        """
        super().__init__(unique_id, model)
        self.is_parked = False
        self.arrived = False
        self.destination = destination
        self.directionLight = (0, 0)
        self.curr_index = 1
        self.route = route
        self.re_route = False
        try: 
            self.intention = self.route[1]
        except:
            print(self.route)

    def assignDirection(self):
        """
        Initialise agent's old direction from model
        """
        self.oldDirection = self.model.grid[self.pos[0]][self.pos[1]][0].directions[0]
        self.newDirection = self.oldDirection

    def move(self):
        """ 
        Determines if the agent can move in the direction indicated by its route
        """
        # Check if car is at the destination
        if self.pos == self.destination:
            self.arrived = True
            return
        
        # Check if car is around the destination and move to it
        if self.destination in self.model.grid.get_neighborhood(self.pos, moore=False, include_center=False):
            self.model.grid.move_agent(self, self.destination)
            self.pos = self.destination
            self.is_parked = True
            return

        try:
            next_cell = self.route[self.curr_index]
        except:
            print(self.route)
            print(self.curr_index, len(self.route))
            print(self.pos)

        # Recalculate the route if there are more than 3 cars in the current one and it wasnt recalculated last step
        if len(self.route[self.curr_index:]) > 3 and not self.re_route:
            if self.isObstacle(next_cell) and self.isObstacle(self.route[self.curr_index + 1]) and self.isObstacle(self.route[self.curr_index + 2]):
                self.route = self.model.a_star.search(1, self.pos, self.destination)
                self.curr_index = 1
                self.re_route = True
                return

        if not self.isObstacle(next_cell):
            self.intention = next_cell
            self.newDirection = self.calcDirection()
            self.curr_index += 1
            # Do not take away re_route flag until the agent moves
            self.re_route = False
        
        print('Estoy en '+str(self.pos)+' y voy a ' + str(self.destination)+' quiero ir a '+str(self.intention)+' vieja '+self.oldDirection+' nueva '+self.newDirection)
        # print(self.oldDirection, self.newDirection)

    def calcDirection(self):
        """
        Returns direction string based on current and last positions
        """
        old = self.route[self.curr_index-1]
        new = self.route[self.curr_index]

        # Calculate difference between both positions
        diff = new[0] - old[0], new[1] - old[1]
        directions = {
            (-1,  0) : "Left",
            ( 1,  0) : "Right",
            ( 0, -1) : "Down",
            ( 0,  1) : "Up"
        }
        return directions[diff]
    
    def isObstacle(self, cell):
        """
        Returns true if given cell is an obstacle (unparked car, red Traffic Light)
        """
        agents = self.model.grid[cell[0]][cell[1]]
        for agent in agents:
            if isinstance(agent, Car):
                if agent.is_parked:
                    return False
                else:
                    return True
            if isinstance(agent, Traffic_Light):
                if agent.state == "Green":
                    return False
                else:
                    return True
        return False
    
    def turnOnBlinkers(self):
        """
        Compares old direction and new direction to turn on/off blinkers
        """
        if self.oldDirection == self.newDirection:
            self.directionLight = (0,0)
            return

        turns = {
            "Up" : {
                "Left" : (1, 0),
                "Right" : (0, 1)
            },
            "Down" : {
                "Right" : (1, 0),
                "Left" : (0, 1)
            },
            "Left" : {
                "Up" : (0, 1), 
                "Down": (1, 0)
            }, 
            "Right": {
                "Up" : (1, 0),
                "Down" : (0, 1)
            }
        }
        self.directionLight = turns[self.oldDirection][self.newDirection]

    def step(self):
        """ 
        Determines the new direction it will take
        """
        self.move()
    
    def advance(self):
        """
        Moves based on new direction (executes after step)
        """

        # Only moves if it isn't parked
        if self.is_parked:
            return
            
        # Checks neighbour's intentions
        neighbors = self.model.grid.get_neighbors(self.pos, include_center=False, moore=True)
        diagonalNeighbors = filter(lambda agent: agent.pos[0] != self.pos[0] and agent.pos[1] != self.pos[1], neighbors)
        shouldMove = True
        for agent in diagonalNeighbors:
            if isinstance(agent, Car) and agent.intention == self.intention and not agent.is_parked:
                # Prioritise the one going straight
                if agent.oldDirection == agent.newDirection:
                    #  Self should NOT move 
                    shouldMove = True
                    break
        
        if shouldMove:
            # Move to next cell and update direction
            self.model.grid.move_agent(self, self.intention)
            self.oldDirection = self.newDirection
        else:
            # Turn on blinkers
            self.turnOnBlinkers()
            self.curr_index-=1

class Destination(Agent):
    """
    Obstacle agent. Just to add obstacles to the grid.
    """
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)

    def step(self):
        pass

class Obstacle(Agent):
    """
    Obstacle agent. Just to add obstacles to the grid.
    """
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)

    def step(self):
        pass

class Road(Agent):
    """
    Obstacle agent. Just to add obstacles to the grid.
    """
    def __init__(self, unique_id, model, directions=["Left"]):
        super().__init__(unique_id, model)
        # Create a set of directions to allow for multidirectional road cells
        self.directions = directions

    def step(self):
        pass