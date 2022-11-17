from mesa import Agent

class TrafficLight(Agent):
    #0 rojo, 1 amarillo, 2 verde
    def __init__(self, model, pos, turno):
        super().__init__(model.next_id(), model)
        self.model = model
        self.pos = pos
        self.turno = turno
        self.estado = 0
        self.contadorVerde = 0
        self.contadorAmarillo = 0

    def step(self):
        #Esta en rojo y ya es su turno de estar en verde
        if(self.estado == 0 and self.model.turnoSemaforos == self.turno):
            self.estado = 2
            return

        #Esta en verde y aun no alcanza el tiempo
        if(self.estado == 2 and self.contadorVerde < self.model.tiempoVerde):
            self.contadorVerde += 1
            return

        #Esta en verde y ya alcanzo el tiempo
        if(self.estado == 2 and self.contadorVerde == self.model.tiempoVerde):
            self.contadorVerde = 0
            self.estado = 1
            return

        #Esta en amarillo y aun no alcanza el tiempo
        if(self.estado == 1 and self.contadorAmarillo < self.model.tiempoAmarillo):
            self.contadorAmarillo += 1
            return

        #Esta en amarillo y ya alcanzo el tiempo
        if(self.estado == 1 and self.contadorAmarillo == self.model.tiempoAmarillo):
            self.contadorAmarillo = 0
            self.estado = 0
            self.model.siguienteTurnoSemaforo()
            return 