using System;
using Container;
using UniRx;
using UnityEngine;

namespace Models
{
    public class Interactable : Unit
    {
        public BoolReactiveProperty InInteractionRange = new();
        public Action<Interactable> InteractionLogic;
        public string InteractButtonText;

        private IDisposable _turnSubscription;
        private Player _player;

        private Interactable(Tile tile)
        {
            _player = GameStateContainer.Player;
            Tile.Value = tile;
            
            _turnSubscription = GameStateContainer.TurnState
                .Where(state => state == TurnState.PlayerTurnEnd)
                .Subscribe(_ => UpdateInteractable());
            UpdateInteractable();
        }

        private void UpdateInteractable()
        {
            var neighbourTiles = Tile.Value.Neighbours;
            foreach (var neigbour in neighbourTiles)
            {
                if (neigbour.CurrentUnit.Value == _player)
                {
                    InInteractionRange.Value = true;
                    return;
                }
            }

            InInteractionRange.Value = false;
        }

        public override void Death()
        {
            base.Death();
            _turnSubscription.Dispose();
        }
    }
}