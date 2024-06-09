using Factory;
using Models;
using TinyRogue;
using Zenject;

namespace Factories
{
    public class WeaponFactory
    {
        [Inject]
        private WeaponView _weaponViewPrefab;
        [Inject]
        private ModalFactory _modalFactory;
        [Inject]
        private DiContainer _container;

        public void CreateWeaponView(WeaponData data, Tile spawnTile)
        {
            WeaponView view = _container.InstantiatePrefab(_weaponViewPrefab).GetComponent<WeaponView>();
            view.Initialize(data);
            Tile dropTile = view.DropWeapon(spawnTile);
            _modalFactory.CreateWeaponDataModal(data, dropTile);
        }
    }
}