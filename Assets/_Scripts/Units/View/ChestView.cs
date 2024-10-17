using Models;
using UnityEngine;

namespace Views
{
    public class ChestView : MonoBehaviour
    {
        private GameUnit _gameUnit;

        public void Initialize(GameUnit gameUnit)
        {
            _gameUnit = gameUnit;
            
            
        }
    }
}