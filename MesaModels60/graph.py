class Graph:
    def __init__(self, size):
        self.adjacency_matrix = []
        for i in range(size):
            self.adjacency_matrix.append([0 for i in range(size)])
        self.size = size
        
    def add_edge(self, v1, v2):
        if v1 == v2:
            print("Connecting to same vertex")
        self.adjacency_matrix[v1][v2] = 1
        self.adjacency_matrix[v2][v1] = 1
        
    def remove_edge(self, v1, v2):
        if self.adjacency_matrix[v1][v2] == 0:
            print("No edge between %d and %d" % (v1, v2))
            return
        self.adjacency_matrix[v1][v2] = 0
        self.adjacency_matrix[v2][v1] = 0
    
    def __len__(self):
        return self.size
    
    def print_matrix(self):
        for row in self.adjacency_matrix:
            print(row)