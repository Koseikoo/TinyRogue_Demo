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

        [Inject]
        private WeaponFactory _weaponFactory;

        [Inject] private UnitActionContainer _unitActionContainer;
        [Inject] private UnitContainer _unitContainer;

        [Inject] private DiContainer _container;
        public (Player, PlayerView) CreatePlayer(PlayerDefinition definition)
        {
            Player player = CreatePlayerModel(definition);
            PlayerView view = _unitViewFactory.CreatePlayerView(player);
            _unitUIFactory.CreatePlayerUI(player);
            _unitUIFactory.CreateUI(player, view.transform)
                .AddXpUI(player);
            return (player, view);
        }
        
        public Enemy CreateEnemy(EnemyDefinition definition, Tile spawnTile)
        {
            Enemy enemy = GetEnemyInstance(definition.Type);
            
            AssignUnitData(enemy, definition);
            AssignEnemyData(enemy, definition);
            BindUnitToIsland(enemy, spawnTile);
            
            EnemyView view = _unitViewFactory.CreateEnemyView(enemy, definition);
            view.GetComponent<UnitView>().Initialize(enemy);
            _unitUIFactory.CreateUI(enemy, view.transform)
                .AddStateUI(enemy)
                .AddHealth(enemy)
                .AddTurnDelayUI(enemy);
            return enemy;
        }

        public Interactable CreateInteractable(InteractableDefinition definition, Tile spawnTile)
        {
            Interactable interactable = _container.Instantiate<Interactable>(new object[] {spawnTile});
            
            AssignUnitData(interactable, definition);
            AssignInteractableData(interactable, definition);
            BindUnitToIsland(interactable, spawnTile);
            
            InteractableView view = _unitViewFactory.CreateInteractableView(interactable, definition);
            view.GetComponent<UnitView>().Initialize(interactable);
            _unitUIFactory.CreateUI(interactable, view.transform)
                .AddInteractionButtonUI(interactable);
            return interactable;
        }
        
        public Unit CreateUnit(UnitDefinition definition, Tile spawnTile)
        {
            Unit unit = _container.Instantiate<Unit>();
            AssignUnitData(unit, definition);
            BindUnitToIsland(unit, spawnTile);
            
            UnitView view = _unitViewFactory.CreateUnitView(unit, definition);
            _unitUIFactory.CreateUI(unit, view.transform)
                .AddHealth(unit);
            return unit;
        }

        private void AssignUnitData(Unit unit, UnitDefinition definition)
        {
            unit.Type = definition.Type;
            unit.MaxHealth = definition.MaxHealth;
            unit.Health.Value = definition.MaxHealth;
            unit.IsInvincible.Value = definition.Invincible;
            unit.DropXp = definition.DropXp;
            unit.IncreaseComboWithDeath = true;
            
            _unitDeathActionContainer.SetDeathActionFor(unit);
            unit.Loot = _itemContainer.GetRandomUnitLoot(unit, unit.Level + 1);
            
            unit.DeathActions.Add(tile =>
            {
                RecipeDefinition recipe = _unitRecipeDropContainer.TryUnlockRecipe(unit.Type);
                if (recipe != null)
                {
                    _modalFactory.CreateUnlockRecipeModal(recipe.Output);
                }
            });
        }

        private void AssignEnemyData(Enemy enemy, EnemyDefinition definition)
        {
            enemy.AttackRange = definition.AttackRange;
            enemy.ScanRange = definition.ScanRange;
            enemy.TurnDelay = definition.TurnDelay;
            
            foreach (ModDefinition modDefinition in definition.Mods)
                enemy.AddMod(modDefinition.ItemDefinition.Type.GetModInstance(modDefinition.Power));
        }

        private void AssignInteractableData(Interactable interactable, InteractableDefinition definition)
        {
            interactable.InteractButtonText = definition.InteractButtonText;
            interactable.InteractionLogic = _unitActionContainer.GetUnitAction(definition.Type);

            
            interactable.DeathActions.Add(tile =>
            {
                WeaponData weapon = new(WeaponName.BaseSword, WeaponSkillHelper.BasePattern, 2);
                _weaponFactory.CreateWeaponView(weapon, tile);
            });
        }
        
        private void BindUnitToIsland(Unit unit, Tile tile)
        {
            if (tile.Island != null)
            {
                unit.Level = tile.Island.Level;
                tile.Island.AddUnit(unit);
            }
            
            tile.MoveUnit(unit);
        }
        
        private Player CreatePlayerModel(PlayerDefinition definition)
        {
            Player player = _container.Instantiate<Player>();
            
            player.Type = UnitType.Player;
            player.MaxHealth = definition.Unit.MaxHealth;
            player.Health.Value = definition.Unit.MaxHealth;
            player.Loot = new Loot(0);
            return player;
        }


        private Enemy GetEnemyInstance(UnitType type)
        {
            return type switch
            {
                UnitType.TestEnemy => _container.Instantiate<SimpleEnemy>(),
                UnitType.SpiderEnemy => _container.Instantiate<SimpleEnemy>(),
                UnitType.WolfEnemy => _container.Instantiate<WolfEnemy>(),
                UnitType.BigWolfEnemy => _container.Instantiate<WolfEnemy>(),
                UnitType.MushroomEnemy => _container.Instantiate<MushroomEnemy>(),
                UnitType.RatEnemy => _container.Instantiate<SimpleEnemy>(),
                UnitType.OrcEnemy => _container.Instantiate<OrcEnemy>(),
                UnitType.GolemEnemy => _container.Instantiate<GolemEnemy>(),
                UnitType.SpecterEnemy => _container.Instantiate<GhostEnemy>(),
                UnitType.FishermanMiniBoss => _container.Instantiate<FishermanMiniBoss>(),
                UnitType.WerewolfBoss => _container.Instantiate<WerewolfBoss>(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}