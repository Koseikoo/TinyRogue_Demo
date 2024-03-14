using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class Node
{
    public Tile Tile;
    public Node parentNode;

    public float G;
    public float H;
    public float F { get => G + H; }

    public Node(Tile nodeTile)
    {
        Tile = nodeTile;
    }
}

public static class AStar
{
    public static List<Tile> FindPath(Tile startTile, Tile endTile, Func<Tile, bool> tileCondition = null)
    {
        List<Node> open = new();
        List<Node> closed = new();
        open.Add(startTile.Node);

        while (open.Count > 0)
        {
            Node curNode = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].F < curNode.F || open[i].F == curNode.F && open[i].H < curNode.H)
                    curNode = open[i];
            }
            open.Remove(curNode);
            closed.Add(curNode);

            if (curNode == endTile.Node)
            {
                return AssignPath();
            }

            Vector3 currentPosition = curNode.Tile.WorldPosition;
            foreach (Tile tile in curNode.Tile.Neighbours)
            {
                Node neighbour = tile.Node;
                if (closed.Contains(neighbour)) continue;

                float curGCost = curNode.G + Vector3.Distance(currentPosition, tile.WorldPosition);
                if (curGCost < neighbour.G || !open.Contains(neighbour))
                {
                    neighbour.G = curGCost;
                    neighbour.H = Vector3.Distance(currentPosition, endTile.WorldPosition);
                    neighbour.parentNode = curNode;

                    if (!open.Contains(neighbour))
                    {
                        bool conditionMet = tileCondition?.Invoke(tile) ?? true;
                        if (tile == endTile || conditionMet)
                            open.Add(neighbour);
                    }
                }
            }
        }
        return null;

        List<Tile> AssignPath()
        {
            List<Tile> path = new();

            Node n = endTile.Node;
            while (n != startTile.Node)
            {
                Tile tile = n.Tile;
                path.Insert(0, tile);
                n = n.parentNode;
            }

            return path;
        }
    }
}