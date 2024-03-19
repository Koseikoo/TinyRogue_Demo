using UnityEngine;
using UnityEngine.Serialization;

namespace Views
{
    public class GoldCoinView : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        public bool MerchantDrop;
        public void Initialize(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);
        }

        public void ResetView()
        {
            MerchantDrop = false;
            gameObject.SetActive(false);
        }
    }
}