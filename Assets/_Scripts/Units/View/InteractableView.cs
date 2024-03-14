using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class InteractableView : MonoBehaviour
    {
        private Interactable _interactable;

        public void Initialize(Interactable interactable)
        {
            _interactable = interactable;

            transform.position = interactable.Tile.Value.WorldPosition;
        }
    }
}