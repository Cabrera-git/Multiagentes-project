from mesa import Agent
import random 
from trafficLight import TrafficLight
from vars_ import *

class Car(Agent):
    def __init__(self, model, pos):
        super().__init__(model.next_id(), model)
        self.pos = pos
        self.model = model
        self.cambioDireccion = 0
        self.orientacion = None  # 1 arriba, 2 abajo, 3 derecha, 4 izquierda
         
    def step(self):
        tipoCasilla = self.model.matrix[INDEXMAT[self.pos[1]]][self.pos[0]]
        next_move = self.getNextStep(tipoCasilla)
        next_move = self.orientationChange(next_move)
        print("Actual:")
        print(self.pos)
        print("Siguiente:")
        print(next_move)
        contenidos_siguiente_casilla = self.model.grid.get_cell_list_contents(next_move)
        if len(contenidos_siguiente_casilla) == 0:
            if(self.pos in CROSSPATH):
                if self.checarPaso() == True:
                    self.model.grid.move_agent(self, next_move)
            else:
                self.model.grid.move_agent(self, next_move)
        elif len(contenidos_siguiente_casilla) == 1 and type(contenidos_siguiente_casilla[0]) is TrafficLight:
            if(self.pos in self.model.semaforos):
                self.model.grid.move_agent(self, next_move)
            elif contenidos_siguiente_casilla[0].estado == 2:
                self.model.grid.move_agent(self, next_move)
    
    def checarPaso(self):
        tipoCasilla = self.model.matrix[INDEXMAT[self.pos[1]]][self.pos[0]]
        if tipoCasilla == 1:
            for x in range(self.pos[0]-2, self.pos[0] + 3 ):
                for y in range(self.pos[1]+1, self.pos[1] + 3 ):
                    contenidos_siguiente_casilla = self.model.grid.get_cell_list_contents((x, y))
                    if len(contenidos_siguiente_casilla) > 0:
                        return False
            return True
        
        elif tipoCasilla == 2:
            for x in range(self.pos[0]-2, self.pos[0] + 3 ):
                for y in range(self.pos[1]- 2, self.pos[1]):
                    contenidos_siguiente_casilla = self.model.grid.get_cell_list_contents((x, y))
                    if len(contenidos_siguiente_casilla) > 0:
                        return False
            return True
            
        elif tipoCasilla == 3:
            for x in range(self.pos[0] + 1, self.pos[0] + 3 ):
                for y in range(self.pos[1]- 2, self.pos[1] + 3):
                    contenidos_siguiente_casilla = self.model.grid.get_cell_list_contents((x, y))
                    if len(contenidos_siguiente_casilla) > 0:
                        return False
            return True
        
        elif tipoCasilla == 4:
            for x in range(self.pos[0] - 2, self.pos[0] ):
                for y in range(self.pos[1]- 2, self.pos[1] + 3):
                    contenidos_siguiente_casilla = self.model.grid.get_cell_list_contents((x, y))
                    if len(contenidos_siguiente_casilla) > 0:
                        return False
            return True
        
    
    def orientationChange(self, next_move):
        if self.pos[0] != next_move[0]:
            if self.pos[0] > next_move[0]:
                siguienteOrientacion = 4
            else:
                siguienteOrientacion = 3

        elif self.pos[1] != next_move[1]:
            if self.pos[1] > next_move[1]:
                siguienteOrientacion = 2
            else:
                siguienteOrientacion = 1

        if self.orientacion == None:
            self.orientacion = siguienteOrientacion 
            return next_move
            
        #Se corrije next step y se continúa con la misma direccion
        if self.cambioDireccion == 1 and siguienteOrientacion != self.orientacion:
            self.cambioDireccion = 0
            casillaActual = self.model.matrix[INDEXMAT[self.pos[1]]][self.pos[0]]
            if casillaActual == 1 or casillaActual == 2 or casillaActual == 3 or casillaActual == 4:
                return self.getNextStep(casillaActual)
            else:
                return self.getNextStep(self.orientacion)
        
        if not self.cambioDireccion == 1 and siguienteOrientacion != self.orientacion:
            self.cambioDireccion += 1
            self.orientacion = siguienteOrientacion
            return next_move

        if siguienteOrientacion == self.orientacion:
            self.cambioDireccion = 0
            return next_move
            
        
    def getNextStep(self, tipoCasilla):
        if tipoCasilla == 1: #arriba
            return (self.pos[0], self.pos[1] + 1)
        elif tipoCasilla == 2: #abajo
            return (self.pos[0], self.pos[1] - 1)
        elif tipoCasilla == 3: #derehca
            return (self.pos[0] + 1, self.pos[1])
        elif tipoCasilla == 4: #izquierda
            return (self.pos[0] - 1, self.pos[1])
        elif tipoCasilla == 5: #arriba o derehca
            posibles = [(self.pos[0], self.pos[1] + 1),(self.pos[0] + 1, self.pos[1])]
            return posibles[random.randint(0,1)]
        elif tipoCasilla == 6: #arriba o izquierda
            posibles = [(self.pos[0], self.pos[1] + 1),(self.pos[0] - 1, self.pos[1])]
            return posibles[random.randint(0,1)]
        elif tipoCasilla == 7: #abajo o derecha
            posibles = [(self.pos[0], self.pos[1] - 1),(self.pos[0] + 1, self.pos[1])]
            return posibles[random.randint(0,1)]
        elif tipoCasilla == 8: #abajo o izquierda
            posibles = [(self.pos[0], self.pos[1]-1),(self.pos[0] - 1, self.pos[1])]
            return posibles[random.randint(0,1)]