using Models;
using TMPro;
using UnityEngine;
using UniRx;

namespace Views
{
    public class EnemyTurnDelayUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnDelay;
        [SerializeField] private GameObject visual;

        private Enemy _enemy;

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _enemy.CurrentTurnDelay.Subscribe(UpdateTurnDelay).AddTo(this);
        }

        private void UpdateTurnDelay(int currentTurnDelay)
        {
            visual.SetActive(currentTurnDelay > 0);
            turnDelay.text = currentTurnDelay.ToString();
        }
    }
}