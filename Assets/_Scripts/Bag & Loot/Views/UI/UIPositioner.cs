using UnityEngine;

namespace Views
{
    public class UIPositioner : MonoBehaviour
    {
        public Vector3 Position { get; private set; }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }
        
        private void LateUpdate()
        {
            transform.position = UIHelper.Camera.WorldToScreenPoint(Position);
        }
    }
}