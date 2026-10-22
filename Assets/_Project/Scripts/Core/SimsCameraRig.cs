using UnityEngine;
using UnityEngine.InputSystem;

namespace CrimeLife.Core
{
    // 심즈식 탑다운 카메라: WASD 이동, Q/E 회전, 휠 줌, Space로 대상에 복귀.
    // 이 오브젝트는 피벗(바닥 위 바라보는 지점)이고, 카메라는 피벗 주위를 도는 위치로 계산한다.
    public class SimsCameraRig : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _target;

        [Header("이동")]
        [SerializeField] private float _panSpeed = 12f;

        [Header("회전/줌")]
        [SerializeField] private float _rotateSpeed = 90f;
        [SerializeField] private float _pitch = 50f;
        [SerializeField] private float _distance = 14f;
        [SerializeField] private float _minDistance = 5f;
        [SerializeField] private float _maxDistance = 30f;
        [SerializeField] private float _zoomSpeed = 1.5f;

        private float _yaw = 45f;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            if (_target != null) transform.position = _target.position;
        }

        private void LateUpdate()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null)
            {
                float rotate = (keyboard.eKey.isPressed ? 1f : 0f) - (keyboard.qKey.isPressed ? 1f : 0f);
                _yaw += rotate * _rotateSpeed * Time.deltaTime;

                Vector2 move = new Vector2(
                    (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                    (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));

                // 줌 아웃할수록 빠르게 움직여야 체감 속도가 같다
                float speed = _panSpeed * (_distance / 14f) * Time.deltaTime;
                Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);
                transform.position += yawRotation * new Vector3(move.x, 0f, move.y) * speed;

                if (keyboard.spaceKey.wasPressedThisFrame && _target != null)
                    transform.position = _target.position;
            }

            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (scroll != 0f)
                    _distance = Mathf.Clamp(_distance - Mathf.Sign(scroll) * _zoomSpeed, _minDistance, _maxDistance);
            }

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            _camera.transform.SetPositionAndRotation(
                transform.position - rotation * Vector3.forward * _distance, rotation);
        }
    }
}
