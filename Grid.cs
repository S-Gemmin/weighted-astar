using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private LayerMask unwalkableMask;

    [SerializeField]
    private Vector2 gridWorldSize;
    
    [SerializeField]
    private float nodeRadius;

    [SerializeField]
    private TerrainType[] walkableRegions;

    [SerializeField]
    private int defaultPenalty;

    private Node[,] grid;
    private LayerMask walkableMask;
    private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize => gridSizeX * gridSizeY;

    // This function creates the grid of nodes that will be used for pathfinding. It loops through 
    // each node in the grid and checks whether it is walkable or not. It also sets the movement 
    // penalty for each node based on the walkable region it belongs to.
    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomPos = (Vector2)transform.position - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomPos + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                bool walkable = Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask) == null;
                int movementPenalty = defaultPenalty;

                if (walkable)
                {
                    RaycastHit2D hit = Physics2D.CircleCast(worldPoint, nodeRadius, Vector2.up, nodeRadius, walkableMask);

                    if (hit)
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                }
                
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }

        foreach (Node node in grid) 
        {
            node.neighbours = GetNeighbours(node);
        }
    }

    // This function returns a list of neighboring nodes for a given node. It loops through the 
    // neighboring nodes and checks if they are within the grid boundaries.
    private List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                    
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // This function takes a world position and returns the node at that position in the grid. It first 
    // converts the world position to a percentage of the grid size, then uses that percentage to 
    // determine the grid coordinates of the node.
    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        float percentX = worldPos.x / gridWorldSize.x + 0.5f;
        float percentY = worldPos.y / gridWorldSize.y + 0.5f;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    // This function is called when the object is selected in the scene view. It draws gizmos in the 
    // scene view to visualize the grid and the walkable and unwalkable nodes.
    private void OnDrawGizmos() 
    {
        if (grid == null)
            return;

        Gizmos.DrawWireCube(transform.position, gridWorldSize);

        foreach (Node node in grid) 
        {
            Gizmos.color = node.walkable ? Color.white : Color.red;
            Gizmos.DrawWireCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
        }
    }

    // This is a serializable class that defines a walkable region. It contains a layer mask that 
    // defines the terrain type for the region, and a movement penalty that affects the cost of 
    // moving through the region.
    [Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
