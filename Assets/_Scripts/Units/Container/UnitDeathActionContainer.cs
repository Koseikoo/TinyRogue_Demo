using System;
using System.Collections.Generic;
using Factories;
using Models;
using Zenject;

namespace Container
{
    public class UnitDeathActionContainer
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private EnemyDefinitionContainer _enemyDefinitionContainer;
        [Inject] private UnitRecipeDropContainer _unitRecipeDropContainer;
        
        public Action<Tile> UnlockEndTileAction;

        private Dictionary<UnitType, Action<Tile>> _unitDeathActions = new();

        public UnitDeathActionContainer()
        {
            UnlockEndTileAction = tile =>
            {
                tile.Island.EndTileUnlocked.Value = true;
            };
            
            _unitDeathActions[UnitType.GraveDestructible] = tile =>
            {
                var enemy = _unitFactory.CreateEnemy(_enemyDefinitionContainer.GetEnemyDefinition(UnitType.SpecterEnemy), tile);
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