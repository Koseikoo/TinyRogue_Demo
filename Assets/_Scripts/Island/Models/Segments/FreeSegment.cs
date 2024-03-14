using Container;
using Factories;
using UnityEngine;
using Zenject;

namespace Models
{
    public class FreeSegment : Segment
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private InteractableDefinitionContainer _interactableDefinitionContainer;

        public FreeSegment(SegmentDefinition definition, Vector3 position = default) : base(definition, position)
        {
        }

        protected override void CheckSegmentCompleteCondition()
        {
            
        }

        public override void SegmentCompleteAction(Transform parent)
        {
            var chestSpawnTile = Tiles.GetMatchingTiles(tile => !tile.HasUnit).PickRandom();
            var definition = _interactableDefinitionContainer.GetInteractableDefinition(UnitType.ChestInteractable);
            _unitFactory.CreateInteractable(definition, chestSpawnTile, GameStateContainer.Player);
            Debug.Log("Spawn Quest NPC Here");
        }
    }
}