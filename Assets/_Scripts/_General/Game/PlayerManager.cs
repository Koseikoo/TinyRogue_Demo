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
        [Inject] private WeaponFactory _weaponFactory;
        [Inject] private CameraFactory _cameraFactory;

        [Inject] private CameraModel _cameraModel;
        
        private Player _player;
        private Weapon _weapon;

        public Player Player => _player;
        public Weapon Weapon => _weapon;
        
        public Player SpawnPlayerWithWeapon(WeaponDefinition weaponDefinition, PlayerDefinition playerDefinition)
        {
            (Weapon weapon, SwordView weaponView) = _weaponFactory.CreateWeapon(weaponDefinition);
            (Player player, PlayerView playerView) = _unitFactory.CreatePlayer(playerDefinition, weapon);

            _weapon = weapon;
            _player = player;
            
            _cameraFactory.CreateCamera(_player);
            
            _player.Bag.AddLoot(new(0, null, null, 
                new()
                {
                    new Resource(ItemType.MonsterResource, 10, ResourceType.Monster),
                    new Resource(ItemType.WoodResource, 10, ResourceType.Monster),
                    new Resource(ItemType.StoneResource, 10, ResourceType.Monster),
                    new Resource(ItemType.PlantResource, 10, ResourceType.Monster),
                }));
            return _player;
        }
    
        public void DestroyPlayer()
        {
            if (_player != null)
            {
                _player.IsDestroyed.Value = true;
                _weapon.IsDestroyed.Value = true;
                _cameraModel.DestroyCommand.Execute();
                _weapon.Dispose();
            }
            _player = null;
            _weapon = null;
        }
    }
}