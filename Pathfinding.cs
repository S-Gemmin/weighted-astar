using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding : MonoBehaviour
{
    private PathRequestManager requestManager;
    private Grid grid;
    private Heap<Node> openSet;
    private HashSet<Node> closedSet;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
        openSet = new Heap<Node>(grid.MaxSize);
        closedSet = new HashSet<Node>();
    }
    // starts the pathfinding process by calling the FindPath() coroutine with the provided 
    // start and target positions.
    public void StartFindPath(Vector2 startPos, Vector2 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    // Coroutine that implements the A* algorithm to find the optimal path from the start node to 
    // the target node. It returns a list of waypoints that represent the path, and a boolean 
    // indicating whether the pathfinding process was successful.
    private IEnumerator FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Vector2[] waypoints = new Vector2[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (!startNode.walkable && !targetNode.walkable)
            yield break;

        openSet.Clear();
        closedSet.Clear();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveRoot();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathSuccess = true;
                break;
            }

            foreach (Node neighbour in currentNode.neighbours)
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour =
                    currentNode.gCost
                    + GetDistance(currentNode, neighbour)
                    + neighbour.movementPenalty;

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) 
                    {
                        openSet.Add(neighbour);
                    } 
                    else 
                    {
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        yield return null;
        
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
          
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    // Given the start and end nodes of a path, this function retraces the path by iterating through 
    // the parents of each node from the end node to the start node. It returns an array of waypoints 
    // representing the path.
    private Vector2[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector2[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    // Simplifies the path by removing any unnecessary waypoints. It checks the direction between each 
    // waypoint in the path and only adds a new waypoint if the direction changes. The function returns 
    // an array of simplified waypoints.
    private Vector2[] SimplifyPath(List<Node> path)
    {
        if (path.Count < 1)
            return new Vector2[0];

        List<Vector2> waypoints = new List<Vector2>();
        waypoints.Add(path[0].worldPosition);

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 dirNew = new Vector2(path[i + 1].gridX - path[i].gridX, path[i + 1].gridY - path[i].gridY);
            Vector2 dirOld = new Vector2(path[i].gridX - path[i - 1].gridX, path[i - 1].gridY - path[i].gridY);
            
            if (dirNew != dirOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
        }

        waypoints.Add(path[path.Count - 1].worldPosition);
        return waypoints.ToArray();
    }

    // Calculates the distance between two nodes using the Manhattan distance heuristic. 
    // This function is used to estimate the distance between two nodes in the A* algorithm.
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return dstX > dstY 
        ? 14 * dstY + 10 * (dstX - dstY) 
        : 14 * dstX + 10 * (dstY - dstX);
    }
}
