using Container;
using Models;
using Views;
using Zenject;

namespace Factories
{
    public class TrapFactory
    {
        [Inject] private TrapView _trapPrefab;
        
        [Inject] private DiContainer _container;

        public Trap Create(TrapDefinition definition, Tile tile)
        {
            Trap trap = definition.GetTrapInstance();
            TrapView view = _container.InstantiatePrefab(_trapPrefab).GetComponent<TrapView>();
            view.Initialize(trap, tile);
            return trap;
        }
    }
}