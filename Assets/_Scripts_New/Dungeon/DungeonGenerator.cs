using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Views;
using Zenject;

namespace TinyRogue
{
    [System.Serializable]
    public class ArchipelConfig
    {
        public int Islands;
        public float IslandBoundsMargin;
        public float separationAmount;
        public int extraConnections;
        
        [Header("Island")]
        public int Segments;
        public Vector2 SizeRange;
        [Range(0, 1)] public float Deformation;
        [Range(0, 1)] public float LastDeformationWeight;

        [Header("Height")]
        public int HeightLevels;
        public float HillSizeMult;
    }

    public class IslandConnection : IComparable<IslandConnection>
    {
        public Island Source;
        public Island Destination;
        public float Weight;

        public IslandConnection(Island source, Island destination, float weight)
        {
            Source = source;
            Destination = destination;
            Weight = weight;
        }
        
        public int CompareTo(IslandConnection other)
        {
            return Weight.CompareTo(other.Weight);
        }
    }
    public class DungeonGenerator
    {
        private System.Random _random = new();
        private ArchipelConfig _config;

        public DungeonGenerator(ArchipelConfig config)
        {
            _config = config;
        }

        public Archipel GenerateDungeon()
        {
            List<Island> islandList = CreateIslands();
            Archipel archipel = CreateArchipel(islandList);
            CreateTiles(archipel);
            archipel.SetStartAndEndTile();

            return archipel;
        }
        
        public List<IslandConnection> KruskalsMST(List<Island> islandList)
        {
            Graph graph = new Graph(islandList, GetConnectionsByDistance(islandList));
            return graph.KruskalMST(_config.extraConnections);
        }

        private List<Island> CreateIslands()
        {
            List<Island> islandList = new();

            for (int i = 0; i < _config.Islands; i++)
            {
                float islandSize = Mathf.Lerp(_config.SizeRange.x, _config.SizeRange.y, (float)_random.NextDouble());
                List<Vector3> islandOutline = GetIslandOutline(islandSize);
                Bounds islandBounds = GetIslandBound(islandOutline);

                Island island = new(islandSize, islandBounds, islandOutline);
                islandList.Add(island);
            }
            
            SeparateIslands(islandList);
            return islandList;
        }

        private Archipel CreateArchipel(List<Island> islandList)
        {
            List<IslandConnection>islandConnections = KruskalsMST(islandList)
                .Select(edge => new IslandConnection(edge.Source, edge.Destination, edge.Weight)).ToList();
            
            Bounds archipelBounds = GetArchipelBound(islandList);
            Archipel archipel = new(archipelBounds, islandList, islandConnections);

            return archipel;
        }

        private void CreateTiles(Archipel archipel)
        {
            Vector3[,] grid = CreateGrid(archipel.Bounds);
            Dictionary<Vector2Int, Tile> tileDict = SetTiles(grid, archipel.Islands);
            LinkTiles(tileDict);
            ConnectIslands(archipel.Connections);
            SetStartTiles(archipel.Islands);
        }

        private void SetStartTiles(List<Island> islandList)
        {
            foreach (Island island in islandList)
            {
                island.StartTile = island.Tiles[0];
            }
        }
        
        private void LinkTiles(Dictionary<Vector2Int, Tile> tiles)
        {
            List<Tile> linkedtiles = new();
            foreach (var kvp in tiles)
            {
                GoThroughPossibleNeighbours(kvp.Key);
            }

            void GoThroughPossibleNeighbours(Vector2Int key)
            {
                if(!tiles.ContainsKey(key))
                    return;
                Tile tile = tiles[key];
                linkedtiles.Add(tile);

                bool uneven = key.y % 2 > 0;
                int offsetLeft = uneven ? key.x - 1 : key.x;
                int offsetRight = uneven ? key.x : key.x + 1;

                List<Tile> neighbours = new();
                if(tiles.TryGetValue(new(offsetLeft, key.y - 1), out Tile bottomLeft))
                    neighbours.Add(bottomLeft);
                    
                if(tiles.TryGetValue(new(offsetRight, key.y - 1), out Tile bottomRight))
                    neighbours.Add(bottomRight);
                    
                if(tiles.TryGetValue(new(key.x - 1, key.y), out Tile left))
                    neighbours.Add(left);
                    
                if(tiles.TryGetValue(new(key.x + 1, key.y), out Tile right))
                    neighbours.Add(right);
                    
                if(tiles.TryGetValue(new(offsetLeft, key.y + 1), out Tile topLeft))
                    neighbours.Add(topLeft);
                    
                if(tiles.TryGetValue(new(offsetRight, key.y + 1), out Tile topRight))
                    neighbours.Add(topRight);

                tile.Neighbours = new(neighbours);
            }
        }

        private void SeparateIslands(List<Island> islands)
        {
            bool anyOverlaps = true;
            while (anyOverlaps)
            {
                anyOverlaps = Separate(islands);
            }
        }

        private Dictionary<Vector2Int, Tile> SetTiles(Vector3[,] grid, List<Island> islandList)
        {
            Dictionary<Vector2Int, Tile> tileDict = new();
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    Vector3 position = grid[x, y];
                    Island island = GetIslandToPosition(position, islandList);
                    
                    if (island != null)
                    {
                        float tileToCenter = Vector3.Distance(island.Bounds.center, position);
                        float t = 1 - Mathf.Clamp01(tileToCenter / (island.Size * _config.HillSizeMult));
                        int height = Mathf.RoundToInt(t * _config.HeightLevels);
                        
                        Tile tile = new(position, island, height);
                        island.Tiles.Add(tile);
                        tileDict[new(x, y)] = tile;
                    }
                }
            }

            return tileDict;
        }

        private void ConnectIslands(List<IslandConnection> connectionList)
        {
            foreach (IslandConnection connection in connectionList)
            {
                Island startIsland = connection.Source;
                Island endIsland = connection.Destination;

                Tile closestToEnd = startIsland.Tiles
                    .OrderBy(t => Vector3.Distance(t.FlatPosition, endIsland.Bounds.center))
                    .First();
                
                Tile closestToStart = endIsland.Tiles
                    .OrderBy(t => Vector3.Distance(t.FlatPosition, startIsland.Bounds.center))
                    .First();
                
                startIsland.AddConnection(closestToEnd, closestToStart);
                endIsland.AddConnection(closestToStart, closestToEnd);
            }
        }

        private Island GetIslandToPosition(Vector3 position, List<Island> islands)
        {
            foreach (Island island in islands.OrderBy(i => Vector3.Distance(i.Bounds.center, position)))
            {
                if (position.IsInsidePolygon(island.Outline.ToArray()))
                    return island;
            }

            return null;
        }
        
        private bool Separate(List<Island> islands)
        {
            bool anyOverlaps = false;
            for (int i = 1; i < islands.Count; i++)
            {
                List<Island> overlappingIslands = GetOverlappingIslands(islands, islands[i]);
                int index = i;
                Vector3 averageDirection = overlappingIslands
                    .Select(island => island.Bounds.center - islands[index].Bounds.center)
                    .Aggregate(Vector3.zero, (cur, vector) => cur + vector);

                if (overlappingIslands.Count > 0)
                    anyOverlaps = true;
                    
                MoveIslandInDirection(islands[i], -averageDirection);
            }

            return anyOverlaps;
        }

        private List<Island> GetOverlappingIslands(List<Island> islands, Island island)
        {
            List<Island> overlappingIslands = islands
                .Where(i => i != island && i.Bounds.Intersects(island.Bounds))
                .ToList();
            return overlappingIslands;
        }

        private List<IslandConnection> GetConnectionsByDistance(List<Island> islands)
        {
            List<IslandConnection> connections = new();
            
            for (int i = 0; i < islands.Count; i++)
            {
                Island currentIsland = islands[i];

                foreach (Island connectedIsland in islands
                             .Where(island => island != currentIsland)
                             .OrderBy(island => Vector3.Distance(island.Bounds.center, currentIsland.Bounds.center)))
                {
                    float connectionDistance =
                        Vector3.Distance(currentIsland.Bounds.center, connectedIsland.Bounds.center);
                    IslandConnection edge = new (currentIsland, connectedIsland, connectionDistance);
                    IslandConnection reversedConnection =
                        connections.FirstOrDefault(conn => conn.Source == edge.Destination && conn.Destination == edge.Source);
                    
                    if(reversedConnection == null)
                        connections.Add(edge);
                }
            }

            return connections
                .OrderBy(conn => conn.Weight)
                .ToList();
        }

        private void MoveIslandInDirection(Island island, Vector3 direction)
        {
            direction.Normalize();
            for (int i = 0; i < island.Outline.Count; i++)
            {
                island.Outline[i] += direction * _config.separationAmount;
            }

            island.Bounds = GetIslandBound(island.Outline);
        }

        private Vector3[,] CreateGrid(Bounds bounds)
        {
            float xLength = bounds.max.x - bounds.min.x;
            float zLength = bounds.max.z - bounds.min.z;
            Vector3[,] gridPositions = new Vector3[
                Mathf.RoundToInt(xLength / Island.XDistance) + 1,
                Mathf.RoundToInt(zLength / Island.ZDistance) + 1];

            for (int x = 0; x < gridPositions.GetLength(0); x++)
            {
                for (int z = 0; z < gridPositions.GetLength(1); z++)
                {
                    Vector3 position = bounds.min + new Vector3(Island.XDistance * x, 0f, Island.ZDistance * z);
                    Vector3 xOffset = default;
                    if (z % 2 == 0)
                        xOffset = Vector3.right * (Island.XDistance * .5f);

                    position += xOffset;

                    gridPositions[x, z] = position;
                }
            }
        
            return gridPositions;
        }
        private Bounds GetIslandBound(List<Vector3> islandOutline)
        {
            float maxX = islandOutline[0].x;
            float maxZ = islandOutline[0].z;
            
            float minX = islandOutline[0].x;
            float minZ = islandOutline[0].z;

            foreach (Vector3 position in islandOutline)
            {
                if (position.x > maxX)
                    maxX = position.x;
                if (position.x < minX)
                    minX = position.x;
                
                if (position.z > maxZ)
                    maxZ = position.z;
                if (position.z < minZ)
                    minZ = position.z;
            }

            float centerX = Mathf.Lerp(minX, maxX, .5f);
            float centerZ = Mathf.Lerp(minZ, maxZ, .5f);

            float xExtend = Mathf.Abs(minX - maxX) + _config.IslandBoundsMargin;
            float zExtend = Mathf.Abs(minZ - maxZ) + _config.IslandBoundsMargin;

            return new Bounds(new(centerX, 0f, centerZ), new(xExtend, 0f, zExtend));
        }

        private Bounds GetArchipelBound(List<Island> islands)
        {
            float maxX = islands[0].Bounds.max.x;
            float maxZ = islands[0].Bounds.max.z;
            
            float minX = islands[0].Bounds.min.x;
            float minZ = islands[0].Bounds.min.z;

            for (int i = 0; i < islands.Count; i++)
            {
                if (islands[i].Bounds.max.x > maxX)
                    maxX = islands[i].Bounds.max.x;
                if (islands[i].Bounds.min.x < minX)
                    minX = islands[i].Bounds.min.x;
                
                if (islands[i].Bounds.max.z > maxZ)
                    maxZ = islands[i].Bounds.max.z;
                if (islands[i].Bounds.min.z < minZ)
                    minZ = islands[i].Bounds.min.z;
            }
            
            float centerX = Mathf.Lerp(minX, maxX, .5f);
            float centerZ = Mathf.Lerp(minZ, maxZ, .5f);

            float xExtend = Mathf.Abs(minX - maxX);
            float zExtend = Mathf.Abs(minZ - maxZ);

            return new Bounds(new(centerX, 0f, centerZ), new(xExtend, 0f, zExtend));
        }

        private List<Vector3> GetIslandOutline(float islandSize)
        {
            List<Vector3> points = new();
            float degreePerSegment = 360f / _config.Segments;
            
            Vector3 startVector = Vector3.forward * islandSize;
            float lastDeformation = 0f;
            for (int i = 0; i < _config.Segments; i++)
            {
                Vector3 vector = startVector.RotateVector(Vector3.up, degreePerSegment * i);
                float deformation = ((float)_random.NextDouble() - .5f) * (_config.Deformation * islandSize);
                deformation = (lastDeformation * _config.LastDeformationWeight) + (deformation * (1 - _config.LastDeformationWeight));
                vector += vector.normalized * deformation;
                lastDeformation = deformation;
                points.Add(vector);
            }

            return points;
        }

        /*
        private void OnDrawGizmos()
        {
            DrawIslandConnections();
            //DrawTestIslands();
            //DrawTestArchipelBounds();
            //DrawTestGrid();
        }

        private void DrawIslandConnections()
        {
            if (testArchipel != null)
            {
                foreach (Island island in testArchipel.Islands)
                {
                    foreach ((Tile start, Tile end) connection in island.Connections)
                    {
                        Gizmos.DrawLine(connection.start.FlatPosition, connection.end.FlatPosition);
                    }
                }
            }
        }

        private void DrawTestIslands()
        {
            if (testIslands is { Count: > 0 })
            {
                for (int island = 0; island < testIslands.Count; island++)
                {
                    List<Vector3> points = testIslands[island].Outline;
                    Bounds bounds = testIslands[island].Bounds;

                    for (int i = 0; i < points.Count; i++)
                    {
                        int next = i + 1;
                        if (next == points.Count)
                            next = 0;
                        Gizmos.DrawLine(points[i], points[next]);
                    }

                    Vector3 bl = bounds.min;
                    Vector3 br = new(bounds.max.x, 0f, bounds.min.z);
                    Vector3 tl = new(bounds.min.x, 0f, bounds.max.z);
                    Vector3 tr = bounds.max;
            
                    Gizmos.DrawLine(bl, br);
                    Gizmos.DrawLine(br, tr);
                    Gizmos.DrawLine(tr, tl);
                    Gizmos.DrawLine(tl, bl);
                }
            }
        }

        private void DrawTestArchipelBounds()
        {
            if (testArchipel != null)
            {
                Bounds bounds = testArchipel.Bounds;
                Vector3 bl = bounds.min;
                Vector3 br = new(bounds.max.x, 0f, bounds.min.z);
                Vector3 tl = new(bounds.min.x, 0f, bounds.max.z);
                Vector3 tr = bounds.max;
                
                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);
                Gizmos.DrawLine(tl, bl);
            }
        }

        private void DrawTestGrid()
        {
            if (testGrid != null)
            {
                float radius = Island.HexagonSize * .6f;
                for (int x = 0; x < testGrid.GetLength(0); x++)
                {
                    for (int y = 0; y < testGrid.GetLength(1); y++)
                    {
                        Gizmos.DrawWireSphere(testGrid[x, y], radius);
                    }
                }
            }
        }
        */
    }
}
