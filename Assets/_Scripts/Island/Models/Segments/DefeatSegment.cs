using System.Linq;
using Container;
using DG.Tweening;
using Factories;
using UnityEngine;
using Views;
using Zenject;

namespace Models
{
    public class DefeatSegment : Segment
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitContainer _unitContainer;
        public DefeatSegment(SegmentView definition, Tile centerTile) : base(definition, centerTile)
        {
            
        }

        public override void SegmentCompleteAction(Transform parent)
        {
            var chestSpawnTile = Tiles.GetMatchingTiles(tile => !tile.HasUnit).PickRandom();
            var definition = _unitContainer.GetInteractableDefinition(UnitType.ChestInteractable);
            _unitFactory.CreateInteractable(definition, chestSpawnTile);
        }
    }
}