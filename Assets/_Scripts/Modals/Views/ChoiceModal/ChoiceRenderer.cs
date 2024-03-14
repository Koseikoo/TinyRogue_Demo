using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class ChoiceRenderer : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button logicButton;
        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void Render(Choice choice)
        {
            logicButton.onClick.RemoveAllListeners();
            logicButton.onClick.AddListener(() => choice.Logic());

            text.text = choice.Text;
            icon.sprite = choice.Icon;
            gameObject.SetActive(true);
        }
    }
}