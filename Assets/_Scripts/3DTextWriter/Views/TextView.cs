using Installer;
using UnityEngine;
using Zenject;

namespace Views
{
    public class TextView : MonoBehaviour
    {
        public char Character;
        [SerializeField] private Transform parent;

        [Inject] private TextContainer _textContainer;
        public void Initialize(char character, Vector3 position)
        {
            Character = character;
            transform.position = position;
            gameObject.SetActive(true);
            SpawnCharacter(character);
        }

        public void ResetView()
        {
            gameObject.SetActive(false);
        }
        
        private void SpawnCharacter(char character)
        {
            Instantiate(_textContainer.GetTextPrefab(character), parent);
        }
    }
}