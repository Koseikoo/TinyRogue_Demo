using System;
using System.Collections.Generic;
using System.Linq;
using Factories;
using Models;
using Zenject;

namespace Container
{
    public class UnitDeathActionContainer
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitContainer _unitContainer;
        
        public Action<Tile> UnlockEndTileAction;

        private Dictionary<UnitType, Action<Tile>> _unitDeathActions = new();

        public UnitDeathActionContainer()
        {
            UnlockEndTileAction = tile =>
            {
                tile.Island.EndTileUnlocked.Value = true;
            };
            
            _unitDeathActions[UnitType.Grave] = tile =>
            {
                var enemy = _unitFactory.CreateEnemy(_unitContainer.GetEnemyDefinition(UnitType.SpecterEnemy), tile);
            };

            _unitDeathActions[UnitType.Pillar] = tile =>
            {
                Island island = tile.Island;
                Segment segment = island.Segments.FirstOrDefault(segment => segment.Tiles.Contains(tile));

                Unit golem = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.GolemEnemy);
                golem.Damage(1, GameStateContainer.Player, true);
            };
        }

        public void SetDeathActionFor(Unit unit)
        {
            Action<Tile> action = _unitDeathActions.ContainsKey(unit.Type) ? _unitDeathActions[unit.Type] : null;
            if(action != null)
                unit.DeathActions.Add(action);
        }
    }
}