using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class SegmentUnit
    {
        public Unit Unit;
        public List<Tile> OccupiedTiles;
        public bool TrackUnit;

        public SegmentUnit(Unit unit, List<Tile> occupiedTiles, bool trackUnit)
        {
            Unit = unit;
            OccupiedTiles = occupiedTiles == null ? new() : new(occupiedTiles);
            TrackUnit = trackUnit;
        }
    }
}