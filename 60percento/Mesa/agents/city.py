from mesa import Model
from mesa.time import RandomActivation
from mesa.space import MultiGrid as Grid
from agents.trafficLight import TrafficLight
from agents.car import Car
from agents.cityObjects import Grass

class City(Model):
    def __init__(self, nCoches=15):
        super().__init__()
        self.schedule = RandomActivation(self)
        self.grid = Grid(16, 16, torus=False)
        self.origenes = [(0, 0), (1, 0), (3, 0), (13, 1), (8, 4), (7, 11), (4, 14),
                         (8, 6), (8, 5), (7, 9), (7, 10), (6, 7), (5, 7), (9, 8), (10, 8)]
        self.semaforos = [(7, 7), (8, 7), (7, 8), (8, 8)]
        self.turnoSemaforos = 0
        self.tiempoVerde = 6
        self.tiempoAmarillo = 2

        self.numCoches = nCoches
        self.numLights = 8

        # 1 - solo arriba
        # 2 - solo abajo
        # 3 - solo derecha
        # 4 - solo izquierda
        # 5 - arriba o derecha
        # 6 - arriba o izquierda
        # 7 - abajo o derecha
        # 8 - abajo o izquierda

        self.matrix = [
            [2, 4, 4, 4, 4, 4, 4, 8, 4, 4, 4, 4, 4, 4, 4, 4],
            [2, 3, 3, 3, 3, 3, 3, 7, 5, 3, 3, 3, 3, 3, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 6, 4, 4, 4, 4, 4, 8, 6, 4, 4, 4, 4, 4, 8, 6],
            [7, 5, 3, 3, 3, 3, 3, 7, 5, 3, 3, 3, 3, 3, 7, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 2, 1],
            [2, 1, 4, 4, 4, 4, 4, 8, 6, 4, 4, 4, 4, 4, 4, 1],
            [3, 3, 3, 3, 3, 3, 3, 3, 5, 3, 3, 3, 3, 3, 3, 1],
        ]

        for i in range(nCoches):
            car = Car(self, self.origenes[i])
            self.grid.place_agent(car, car.pos)
            self.schedule.add(car)

        for i in range(len(self.semaforos)):
            semaforo = TrafficLight(self, self.semaforos[i], i)
            self.grid.place_agent(semaforo, semaforo.pos)
            self.schedule.add(semaforo)

        for _, x, y in self.grid.coord_iter():
            if self.matrix[y][x] == 0:
                block = Grass(self, (x, y))
                self.grid.place_agent(block, block.pos)

    def siguienteTurnoSemaforo(self):
        if(self.turnoSemaforos < len(self.semaforos) - 1):
            self.turnoSemaforos += 1
        else:
            self.turnoSemaforos = 0
        print("Turno Semaforo: " + str(self.turnoSemaforos))

    def step(self):
        self.schedule.step()
