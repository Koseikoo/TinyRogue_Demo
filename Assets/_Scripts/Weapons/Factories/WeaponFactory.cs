using Models;
using Zenject;
using Views;

namespace Factories
{
    public class WeaponFactory
    {
        [Inject] private SwordView _swordPrefab;
        [Inject] private DiContainer _container;

        public (Weapon, SwordView) CreateWeapon(WeaponDefinition definition)
        {
            Weapon weapon = CreateSwordModel(definition);
            SwordView view = CreateSwordView(weapon);
            return (weapon, view);
        }
        
        private Weapon CreateSwordModel(WeaponDefinition definition)
        {
            Weapon weapon = _container.Instantiate<Weapon>(new object[]
            {
                definition.Range,
                definition.MaxAttackCharges,
                definition.MaxMoveCharges
            });

            foreach (var modDefinition in definition.Mods)
                weapon.AddMod(modDefinition.ItemDefinition.Type.GetModInstance(modDefinition.Power));
            
            return weapon;
        }

        private SwordView CreateSwordView(Weapon weapon)
        {
            SwordView view = _container.InstantiatePrefab(_swordPrefab).GetComponent<SwordView>();
            view.Initialize(weapon);
            return view;
        }
    }
}