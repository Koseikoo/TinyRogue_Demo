using UnityEngine;

namespace Views
{
    public class GoldCoinView : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        public void Initialize(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);
        }

        public void ResetView()
        {
            gameObject.SetActive(false);
        }
    }
}