using System;
using System.Collections.Generic;
using Container;
using Factory;
using Installer;
using Models;
using UnityEngine;
using Views;
using Zenject;
using GolemEnemy = Models.GolemEnemy;

namespace Factories
{
    public class UnitFactory
    {
        [Inject] private UnitUIFactory _unitUIFactory;
        [Inject] private UnitViewFactory _unitViewFactory;
        [Inject] private ItemContainer _itemContainer;
        [Inject] private UnitDeathActionContainer _unitDeathActionContainer;
        [Inject] private UnitRecipeDropContainer _unitRecipeDropContainer;
        [Inject] private ModalFactory _modalFactory;

        [Inject] private UnitActionContainer _unitActionContainer;

        [Inject] private DiContainer _container;
        public (Player, PlayerView) CreatePlayer(PlayerDefinition definition, Weapon weapon)
        {
            Player player = CreatePlayerModel(definition, weapon);
            var view = _unitViewFactory.CreatePlayerView(player);
            _unitUIFactory.CreatePlayerUI(player);
            _unitUIFactory.CreateUI(player, view.transform)
                .AddXpBarUI(player);
            return (player, view);
        }
        
        public Enemy CreateEnemy(EnemyDefinition definition, Tile spawnTile)
        {
            int islandLevel = spawnTile.Island.Level;
            Enemy enemy = CreateEnemyModel(definition, spawnTile, islandLevel);
            enemy.Loot = _itemContainer.GetRandomUnitLoot(enemy, enemy.Level+1);
            EnemyView view = _unitViewFactory.CreateEnemyView(enemy);
            view.GetComponent<UnitView>().Initialize(enemy);
            _unitUIFactory.CreateUI(enemy, view.transform)
                .AddStateUI(enemy)
                .AddHealth(enemy)
                .AddTurnDelayUI(enemy);
            return enemy;
        }

        public Unit CreateUnit(UnitDefinition definition, Tile spawnTile)
        {
            Unit unit = CreateUnitModel(definition, spawnTile);
            unit.Loot = _itemContainer.GetRandomUnitLoot(unit, unit.Level + 1);
            UnitView view = _unitViewFactory.CreateUnitView(unit);
            _unitUIFactory.CreateUI(unit, view.transform)
                .AddHealth(unit);
            return unit;
        }

        public Unit CreateSegmentUnit(SegmentUnitDefinition definition, Segment segment)
        {
            Unit unit = definition.Unit.GetInstance();
            unit.Loot = _itemContainer.GetRandomUnitLoot(unit, unit.Level + 1);
            _unitDeathActionContainer.SetDeathActionFor(unit);
            Island island = segment.Tiles[0].Island;
            List<Tile> unitTiles = new();
            for (int i = 0; i < definition.Points.Count; i++)
            {
                Tile tile = island.Tiles.GetClosestTileFromPosition(definition.Points[i].position);
                tile.CurrentUnit.Value = unit;
                unitTiles.Add(tile);
                
                if(i == 0)
                    unit.Tile.Value = unitTiles[i];
                else
                    unit.AdditionalTiles.Add(unitTiles[i]);
            }
            
            
            island.AddUnit(unit);
            segment.AddUnit(unit, false, unitTiles);
            
            definition.View.Initialize(unit);
            return unit;
        }

        public Interactable CreateInteractable(InteractableDefinition definition, Tile spawnTile, Player player)
        {
            if (!definition.Unit.Type.ToString().ToLower().Contains("interact"))
                throw new Exception($"unit {definition.Unit.Type} is not an Interactable");

            Interactable interactable = new Interactable(definition, spawnTile, player);
            _unitDeathActionContainer.SetDeathActionFor(interactable);
            interactable.MaxHealth = 1;
            interactable.Health.Value = 1;
            interactable.InteractButtonText = definition.InteractButtonText;
            interactable.Loot = _itemContainer.GetRandomUnitLoot(interactable, 1);
            interactable.InteractionLogic = _unitActionContainer.GetUnitAction(definition.Unit.Type);
            spawnTile.MoveUnit(interactable);
            if(spawnTile.Island != null)
                spawnTile.Island.AddUnit(interactable);
            
            InteractableView view = _unitViewFactory.CreateInteractableView(interactable);
            view.GetComponent<UnitView>().Initialize(interactable);
            _unitUIFactory.CreateUI(interactable, view.transform)
                .AddInteractionButtonUI(interactable);
            return interactable;
        }

        public Unit CreateObstacle(UnitDefinition definition, Tile spawnTile)
        {
            Unit unit = CreateUnitModel(definition, spawnTile);
            _unitDeathActionContainer.SetDeathActionFor(unit);
            UnitView view = _unitViewFactory.CreateUnitView(unit);
            _unitUIFactory.CreateUI(unit, view.transform);
            return unit;
        }

        private Player CreatePlayerModel(PlayerDefinition definition, Weapon weapon)
        {
            Player player = _container.Instantiate<Player>(
                new object[] {weapon});
            
            player.Type = UnitType.Player;
            player.MaxHealth = definition.Unit.MaxHealth;
            player.Health.Value = definition.Unit.MaxHealth;
            player.Loot = new(0);

            weapon.Owner = player;
            return player;
        }

        private Enemy CreateEnemyModel(EnemyDefinition definition, Tile spawnTile, int islandLevel)
        {
            definition = definition.ScaledWithLevel(islandLevel);
            Enemy enemy = GetEnemyInstance(definition.Unit.Type);
            enemy.DeathActions.Add(tile =>
            {
                var recipe = _unitRecipeDropContainer.TryUnlockRecipe(enemy.Type);
                if(recipe != null)
                    _modalFactory.CreateUnlockRecipeModal(recipe.Output);
            });
            
            _unitDeathActionContainer.SetDeathActionFor(enemy);
            enemy.Type = definition.Unit.Type;
            enemy.Level = islandLevel;
            enemy.MaxHealth = definition.Unit.MaxHealth;
            enemy.Health.Value = definition.Unit.MaxHealth;
            enemy.AttackRange = definition.AttackRange;
            enemy.ScanRange = definition.ScanRange;
            enemy.TurnDelay = definition.TurnDelay;
            enemy.Tile.Value = spawnTile;
            enemy.DropXp = definition.Unit.DropXp;
            spawnTile.MoveUnit(enemy);
            
            spawnTile.Island.AddUnit(enemy);

            foreach (var modDefinition in definition.Mods)
                enemy.AddMod(modDefinition.ItemDefinition.Type.GetModInstance(modDefinition.Power));

            return enemy;
        }

        private Unit CreateUnitModel(UnitDefinition definition, Tile spawnTile)
        {
            Unit unit = definition.GetInstance();
            
            _unitDeathActionContainer.SetDeathActionFor(unit);
            unit.MaxHealth = definition.MaxHealth;
            unit.Health.Value = definition.MaxHealth;
            unit.Tile.Value = spawnTile;
            unit.DropXp = definition.DropXp;
            spawnTile.MoveUnit(unit);
            spawnTile.Island.AddUnit(unit);
            return unit;
        }
        
        private Enemy GetEnemyInstance(UnitType type)
        {
            return type switch
            {
                UnitType.TestEnemy => _container.Instantiate<SimpleEnemy>(),
                UnitType.SpiderEnemy => _container.Instantiate<SimpleEnemy>(),
                UnitType.WolfEnemy => _container.Instantiate<WolfEnemy>(),
                UnitType.Mushroom => _container.Instantiate<MushroomEnemy>(),
                UnitType.Rat => _container.Instantiate<SimpleEnemy>(),
                UnitType.Orc => _container.Instantiate<SimpleEnemy>(),
                UnitType.GolemEnemy => _container.Instantiate<GolemEnemy>(),
                UnitType.SpecterEnemy => _container.Instantiate<GhostEnemy>(),
                UnitType.FishermanMiniBoss => _container.Instantiate<FishermanMiniBoss>(),
                UnitType.WerewolfBoss => _container.Instantiate<WerewolfBoss>(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}