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
                Enemy enemy = _unitFactory.CreateEnemy(_unitContainer.GetEnemyDefinition(UnitType.SpecterEnemy), tile);
            };

            _unitDeathActions[UnitType.IslandHeart] = tile =>
            {
                tile.Island.IsHeartDestroyed.Value = true;
            };
        }

        public void SetDeathActionFor(GameUnit gameUnit)
        {
            Action<Tile> action = _unitDeathActions.ContainsKey(gameUnit.Type) ? _unitDeathActions[gameUnit.Type] : null;
            if(action != null)
            {
                gameUnit.DeathActions.Add(action);
            }
        }
    }
}