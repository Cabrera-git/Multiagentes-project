from mesa import Agent

class Grass(Agent):
    def __init__(self, model, pos):
        super().__init__(model.next_id(), model)
        self.pos = pos

class Street(Agent):
    def __init__(self, model, pos, tipo):
        super().__init__(model.next_id(), model)
        self.pos = pos
        self.tipo = tipo
