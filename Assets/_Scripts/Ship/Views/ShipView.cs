using Factories;
using Factory;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace Views
{
    public class ShipView : MonoBehaviour
    {
        [SerializeField] private MerchantView merchantView;
        [SerializeField] private BlackSmithView blackSmithView;
        [SerializeField] private ShipDefinition _shipDefinition;

        [Inject] private LootFactory _lootFactory;
        [Inject] private ModalFactory _modalFactory;
        public ShipDefinition ShipDefinition => _shipDefinition;
        
        public void Initialize(Ship ship)
        {
            ship.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => DestroyShip(ship))
                .AddTo(this);
            
            merchantView.Initialize(ship.Merchant, _lootFactory);
            blackSmithView.Initialize(ship.BlackSmith, _modalFactory);
        }

        private void DestroyShip(Ship ship)
        {
            foreach (var unit in ship.Units)
            {
                unit.IsDestroyed.Value = true;
            }
            Destroy(gameObject);
        }
    }
}