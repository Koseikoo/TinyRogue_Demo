using System;
using System.Collections.Generic;
using Models;

namespace TinyRogue
{
    public class Graph
    {
        private List<Island> Islands;
        private List<IslandConnection> Edges;
    
        public Graph(List<Island> islands, List<IslandConnection> connections)
        {
            Islands = islands;
            Edges = connections;
        }
        
        public List<IslandConnection> KruskalMST(int extraEdges)
        {
            List<IslandConnection> result = new List<IslandConnection>();
            Dictionary<Island, Island> parent = new Dictionary<Island, Island>();
            Dictionary<Island, int> rank = new Dictionary<Island, int>();
    
            foreach (var island in Islands)
            {
                parent[island] = island;
                rank[island] = 0;
            }
    
            Edges.Sort();
    
            int e = 0;
            int i = 0;
    
            while (e < Islands.Count - 1 && i < Edges.Count)
            {
                IslandConnection nextEdge = Edges[i++];
                Island x = Islands[Find(parent, nextEdge.Source)];
                Island y = Islands[Find(parent, nextEdge.Destination)];
    
                if (x != y)
                {
                    result.Add(nextEdge);
                    e++;
                    Union(parent, rank, x, y);
                }
            }

            int addedEdges = 0;
            foreach (IslandConnection edge in Edges)
            {
                if (!result.Contains(edge))
                {
                    result.Add(edge);
                    addedEdges++;
                }
                
                if(addedEdges == extraEdges)
                    break;
            }
    
            return result;
        }

        private int Find(Dictionary<Island, Island> parent, Island i)
        {
            if (parent[i] == i)
                return Islands.IndexOf(i);
            return Find(parent, parent[i]);
        }
    
        private void Union(Dictionary<Island, Island> parent, Dictionary<Island, int> rank, Island x, Island y)
        {
            Island xroot = Islands[Find(parent, x)];
            Island yroot = Islands[Find(parent, y)];
    
            if (rank[xroot] < rank[yroot])
                parent[xroot] = yroot;
            else if (rank[xroot] > rank[yroot])
                parent[yroot] = xroot;
            else
            {
                parent[yroot] = xroot;
                rank[xroot]++;
            }
        }
    }
}