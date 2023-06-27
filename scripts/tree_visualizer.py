import pydot
from typing import *

orders = [
    ("S1", "S2"),
    ("S1", "B"),
    ("S2", "C")
]

events = ["A", "S1", "S2", "B", "C"]

graph = pydot.Dot("my_graph", graph_type="graph") 
total_node = 0


def dfs(parent: pydot.Node, visited: Set[str]):
    global total_node
    global graph
    for e in events:
        if e not in visited:
            satisfied = True
            for (pre, post) in orders:
                if post == e and pre not in visited:
                    satisfied = False
            if satisfied:
                new_node = pydot.Node(str(total_node))
                total_node += 1
                graph.add_node(new_node)
                edge = pydot.Edge(parent, new_node)
                edge.set_label(e)
                graph.add_edge(edge)
                visited.add(e)
                dfs(new_node, visited)
                visited.remove(e)


new_node = pydot.Node(str(total_node))
dfs(pydot.Node("root"), set())

graph.write_png("out.png")

