using System;
using System.Collections.Generic;
using Container;
using UniRx;
using UnityEngine;

namespace Models
{
    public class Interactable : GameUnit
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
            List<Tile> neighbourTiles = Tile.Value.Neighbours;
            foreach (Tile neigbour in neighbourTiles)
            {
                if (neigbour.Unit.Value == _player)
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