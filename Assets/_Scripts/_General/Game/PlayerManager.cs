using Container;
using Factories;
using Factory;
using Models;
using UniRx;
using UnityEngine;
using Views;
using Zenject;

namespace Game
{
    public class PlayerManager
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private CameraFactory _cameraFactory;
        [Inject] private SkillContainer _skillContainer;

        [Inject] private CameraModel _cameraModel;
        
        private Player _player;

        public Player Player => _player;
        
        public Player SpawnPlayerWithWeapon(WeaponDefinition weaponDefinition, PlayerDefinition playerDefinition)
        {
            (Player player, PlayerView playerView) = _unitFactory.CreatePlayer(playerDefinition);
            _player = player;
            
            _cameraFactory.CreateCamera(_player);
            
            //_player.Bag.AddResource(new Resource(ItemType.MonsterResource, 10, ResourceType.Monster));
            //_player.Bag.AddResource(new Resource(ItemType.WoodResource, 10, ResourceType.Monster));
            //_player.Bag.AddResource(new Resource(ItemType.StoneResource, 10, ResourceType.Monster));
            //_player.Bag.AddResource(new Resource(ItemType.PlantResource, 10, ResourceType.Monster));
            
            return _player;
        }
    
        public void DestroyPlayer()
        {
            if (_player != null)
            {
                _player.IsDestroyed.Value = true;
                _cameraModel.DestroyCommand.Execute();
            }
            _player = null;
        }
    }
}