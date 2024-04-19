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
        
        private Dictionary<UnitType, Action<Tile>> _unitDeathActions = new();
        public UnitDeathActionContainer()
        {
            _unitDeathActions[UnitType.Grave] = tile =>
            {
                var enemy = _unitFactory.CreateEnemy(_unitContainer.GetEnemyDefinition(UnitType.SpecterEnemy), tile);
            };

            _unitDeathActions[UnitType.IslandHeart] = tile =>
            {
                tile.Island.IsHeartDestroyed.Value = true;
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