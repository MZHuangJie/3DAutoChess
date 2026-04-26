using UnityEngine;

namespace AutoChess.Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 12f;
        [SerializeField] private float height = 10f;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 25f;

        private float currentAngle = 0f;

        void Start()
        {
            UpdatePosition();
        }

        void LateUpdate()
        {
            if (Input.GetMouseButton(1)) // Right mouse drag
            {
                currentAngle += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                distance -= scroll * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            UpdatePosition();
        }

        void UpdatePosition()
        {
            if (target == null) return;
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Sin(rad) * distance, height, -Mathf.Cos(rad) * distance);
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }
}
