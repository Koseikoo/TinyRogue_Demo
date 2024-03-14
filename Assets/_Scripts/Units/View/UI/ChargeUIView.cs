using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Views
{
    public class ChargeUIView : MonoBehaviour
    {
        [SerializeField] private Image _chargeImage;
        [SerializeField] private GameObject _backgroundImage;
        public void ShowView(bool filled)
        {
            _chargeImage.DOFade(filled ? 1 : .1f, .1f);
            _backgroundImage.SetActive(true);
            gameObject.SetActive(true);
        }
    }
}