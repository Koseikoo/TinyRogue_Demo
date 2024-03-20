using Models;
using UnityEngine;

namespace Factories
{
    public class HexGridFactory
    {
        public Vector3[,] Create(Bounds bounds, float hexagonSize)
        {
            Vector3[,] gridPositions = new Vector3[(int)(bounds.size.z / hexagonSize),
                (int)(bounds.size.x / hexagonSize)];

            for (int y = 0; y < gridPositions.GetLength(1); y++)
            {
                for (int x = 0; x < gridPositions.GetLength(0); x++)
                {
                    Vector3 position = bounds.min + new Vector3(Island.XDistance * x, 0f, Island.ZDistance * y);
                    Vector3 xOffset = default;
                    if (y % 2 == 0)
                        xOffset = Vector3.right * (Island.XDistance * .5f);

                    position += xOffset;

                    gridPositions[x, y] = position;
                }
            }
        
            return gridPositions;
        }
    }
}