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
            Tile chestSpawnTile = GameStateContainer.Player.Tile.Value.Neighbours.GetMatchingTiles(tile => !tile.HasUnit && !tile.IsWeak).Random();
            InteractableDefinition definition = _unitContainer.GetInteractableDefinition(UnitType.ChestInteractable);
            Interactable chest = _unitFactory.CreateInteractable(definition, chestSpawnTile);
        }
    }
}