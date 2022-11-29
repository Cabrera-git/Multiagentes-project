"""
Original code by Baijayanta Roy
https://github.com/BaijayantaRoy/Medium-Article/blob/master/A_Star.ipynb
"""

class Node:
    """
        A node class for A* Pathfinding
        parent is parent of the current Node
        position is current position of the Node in the maze
        g is cost from start to current Node
        h is heuristic based estimated cost for current Node to end Node
        f is total cost of present node i.e. :  f = g + h
    """

    def __init__(self, parent=None, position=None):
        self.parent = parent
        self.position = position

        self.g = 0
        self.h = 0
        self.f = 0
    def __eq__(self, other):
        return self.position == other.position

class AStar:
    """
    A class for A* Pathfinding.
    Args:
        maze: a 2d list with the obstacles+directions
    """

    def __init__(self, maze):
        self.maze = maze


    def return_path(self, current_node):
        """
        This function returns the path of the search
        """
        
        path = []
        no_rows = len(self.maze)

        current = current_node
        while current is not None:
            adjust_x = current.position[1]
            adjust_y = no_rows-1 - current.position[0]
            path.append((adjust_x, adjust_y))
            current = current.parent
        # Return reversed path as we need to show from start to end path
        path = path[::-1]
        return path


    def search(self, cost, start, end):
        """
            Returns a list of tuples as a path from the given start to the given end in the given maze
            :param maze:
            :param cost
            :param start:
            :param end:
            :return:
        """
        n_rows = len(self.maze)-1
        start = [n_rows-start[1], start[0]]
        end = [n_rows-end[1], end[0]]
        # Create start and end node with initized values for g, h and f
        start_node = Node(None, tuple(start))
        start_node.g = start_node.h = start_node.f = 0
        end_node = Node(None, tuple(end))
        end_node.g = end_node.h = end_node.f = 0

        # Initialize both yet_to_visit and visited list
        # in this list we will put all node that are yet_to_visit for exploration. 
        # From here we will find the lowest cost node to expand next
        yet_to_visit_list = []  
        # in this list we will put all node those already explored so that we don't explore it again
        visited_list = [] 
        
        # Add the start node
        yet_to_visit_list.append(start_node)
        
        # Adding a stop condition. This is to avoid any infinite loop and stop 
        # execution after some reasonable number of steps
        outer_iterations = 0
        max_iterations = (len(self.maze) // 2) ** 10

        # what squares do we search. depends on what the maze template tells us
        movements = {
            ">" : [[ 0, 1 ]],   # go right
            "<" : [[ 0, -1]],   # go left
            "v" : [[ 1, 0 ]],   # go down
            "^" : [[-1, 0 ]],   # go up
            "≤" : [[ 0, -1], 
                [ 1, 0 ]],   # go left and down
            "⋜" : [[ 0, -1], 
                [-1, 0 ]],   # go left and up
            "≥" : [[ 0, 1 ], 
                [ 1, 0 ]],   # go right and down
            "⋝" : [[ 0, 1 ], 
                [-1, 0 ]],   # go right and up 
            "ú" : [[ 0, 1 ]],   # go right 
            "ù" : [[ 0, -1]],   # go left
            "Û" : [[-1, 0 ]],   # go up
            "Ǔ" : [[ 1, 0 ]]    # go down
        }


        """
            1) We first get the current node by comparing all f cost and selecting the lowest cost node for further expansion
            2) Check max iteration reached or not . Set a message and stop execution
            3) Remove the selected node from yet_to_visit list and add this node to visited list
            4) Perofmr Goal test and return the path else perform below steps
            5) For selected node find out all children (use move to find children)
                a) get the current postion for the selected node (this becomes parent node for the children)
                b) check if a valid position exist (boundary will make few nodes invalid)
                c) if any node is a wall then ignore that
                d) add to valid children node list for the selected parent
                
                For all the children node
                    a) if child in visited list then ignore it and try next node
                    b) calculate child node g, h and f values
                    c) if child in yet_to_visit list then ignore it
                    d) else move the child to yet_to_visit list
        """
        #find maze has got how many rows and columns 
        no_rows, no_columns = [len(self.maze), len(self.maze[0])]
        
        # Loop until you find the end
        
        while len(yet_to_visit_list) > 0:
            print("working")
            
            # Every time any node is referred from yet_to_visit list, counter of limit operation incremented
            outer_iterations += 1     

            # Get the current node
            current_node = yet_to_visit_list[0]
            current_index = 0
            for index, item in enumerate(yet_to_visit_list):
                if item.f < current_node.f:
                    current_node = item
                    current_index = index
                    
            # if we hit this point return the path such as it may be no solution or 
            # computation cost is too high
            if outer_iterations > max_iterations:
                print ("giving up on pathfinding too many iterations")
                return self.return_path(current_node)

            # Pop current node out off yet_to_visit list, add to visited list
            yet_to_visit_list.pop(current_index)
            visited_list.append(current_node)

            # test if goal is reached or not, if yes then return the path
            # if current_node == end_node:
            if (((current_node.position[0] - end_node.position[0]) ** 2) + 
                ((current_node.position[1] - end_node.position[1]) ** 2)) == 1:
                return self.return_path(current_node)

            # Generate children from all adjacent squares
            children = []

            move = movements[self.maze[current_node.position[0]][current_node.position[1]]]

            for new_position in move: 

                # Get node position
                node_position = (current_node.position[0] + new_position[0], current_node.position[1] + new_position[1])

                # Make sure within range (check if within maze boundary)
                if (node_position[0] > (no_rows - 1) or 
                    node_position[0] < 0 or 
                    node_position[1] > (no_columns -1) or 
                    node_position[1] < 0):
                    continue

                # Make sure walkable terrain
                if self.maze[node_position[0]][node_position[1]] == '#':
                    continue

                # Create new node
                new_node = Node(current_node, node_position)

                # Append
                children.append(new_node)

            # Loop through children
            for child in children:
                
                # Child is on the visited list (search entire visited list)
                if len([visited_child for visited_child in visited_list if visited_child == child]) > 0:
                    continue

                # Create the f, g, and h values
                child.g = current_node.g + cost
                # Heuristic costs calculated here, this is using manhattan distance
                child.h = (abs(child.position[0] - end_node.position[0]) + 
                           abs(child.position[1] - end_node.position[1]))

                child.f = child.g + child.h

                # Child is already in the yet_to_visit list and g cost is already lower
                if len([i for i in yet_to_visit_list if child == i and child.g > i.g]) > 0:
                    continue

                # Add the child to the yet_to_visit list
                yet_to_visit_list.append(child)