using System.Linq;
using Container;
using Factories;
using UnityEngine;
using Views;
using Zenject;

namespace Models
{
    public class DefeatSegment : Segment
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private InteractableDefinitionContainer _interactableDefinitionContainer;
        public DefeatSegment(SegmentView definition, Vector3 position = default) : base(definition, position)
        {
            
        }

        public override void SegmentCompleteAction(Transform parent)
        {
            var chestSpawnTile = Tiles.GetMatchingTiles(tile => !tile.HasUnit).PickRandom();
            var definition = _interactableDefinitionContainer.GetInteractableDefinition(UnitType.ChestInteractable);
            _unitFactory.CreateInteractable(definition, chestSpawnTile, GameStateContainer.Player);
        }
    }
}