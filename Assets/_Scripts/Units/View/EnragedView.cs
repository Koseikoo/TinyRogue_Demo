using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class EnragedView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem enrageFX;
        public void Initialize(Enemy enemy)
        {
            enemy.IsEnraged.Where(b => b).Subscribe(_ => Enrage()).AddTo(this);
        }

        private void Enrage()
        {
            enrageFX.Play();
        }
    }
}