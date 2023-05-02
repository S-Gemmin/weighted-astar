using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public List<Node> neighbours = new List<Node>();
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;
    private int heapIndex;

    // This represents the total cost of moving from the starting node to the end node through this particular node.
    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector2 worldPosition, int gridX, int gridY, int movementPenalty)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        this.movementPenalty = movementPenalty;
    }

    // The heapIndex is used in the Heap class to keep track of the node's position in the heap.
    public int HeapIndex 
    {
        get => heapIndex;
        set { heapIndex = value; }
    }
    
    // This function compares the fCost and hCost values of two nodes and returns a negative, 
    // positive, or zero value based on their priority.
    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        
        return -compare;
    }
}
