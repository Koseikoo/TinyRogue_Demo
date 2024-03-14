using TMPro;
using UnityEngine;
using DG.Tweening;
using Factory;
using UniRx;
using Zenject;

namespace Views
{
    public class DeathModalView : MonoBehaviour
    {
        [SerializeField] private GameObject tryAgainButton;
        [SerializeField] private TextMeshProUGUI earnedHeritageText;

        [Inject] private ModalFactory _modalFactory;

        public void Initialize()
        {
            Sequence sequence = DOTween.Sequence();

            sequence.AppendInterval(.5f).OnComplete(() => tryAgainButton.SetActive(true));
            earnedHeritageText.text = PersistentPlayerState.CurrentRunHeritage.Value.ToString();
            
            PersistentPlayerState.ApplyHeritage();
            
            GameStateContainer.GameState
                .Where(state => state != GameState.Dead)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);
        }

        public void TryAgain()
        {
            GameStateContainer.GameState.Value = GameState.CharacterCreation;
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
        }
    }
}