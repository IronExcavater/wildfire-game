using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public enum CameraMode
        {
            Auto,
            Manual
        }

        private CameraMode _mode = CameraMode.Auto;

        public CameraMode Mode
        {
            get => _mode;
            private set
            {
                _mode = value;
                if (value == CameraMode.Manual) _inactivityTimer = 0;
            }
        }

        [Header("Auto Control")]
        public float autoZoom = 100f;
        public float autoTilt = 45f;
        public float autoYaw = 0f;
        public float autoSpeed = 2f;

        [Header("Manual Control")]
        public float panSpeed = 1f;
        public float rotateSpeed = 10f;
        public float zoomSpeed = 20f;

        [Header("Control Bounds")]
        public float minZoom = 20f;
        public float maxZoom = 200f;
        public float minTilt = 20f;
        public float maxTilt = 80f;
        public float manualTimeout = 5f;

        private Camera _camera;
        private Vector3 _smoothTargetPosition;
        private Vector3 _smoothCameraPosition;
        public Bounds targetBounds;

        [Header("Input Actions")]
        public InputActionReference panAction;
        public InputActionReference rotateAction;
        public InputActionReference zoomAction;
        private float _inactivityTimer;

        private Vector3 _targetPosition;
        private float _targetZoom;
        private float _targetTilt;
        private float _targetYaw;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            panAction?.action.Enable();
            rotateAction?.action.Enable();
            zoomAction?.action.Enable();
        }

        private void OnDisable()
        {
            panAction?.action.Disable();
            rotateAction?.action.Disable();
            zoomAction?.action.Disable();
        }

        private void Update()
        {
            HandleInput();

            switch (Mode)
            {
                case CameraMode.Auto:
                    _targetPosition = new Vector3(targetBounds.center.x, 0, targetBounds.center.z);
                    _targetTilt = autoTilt;
                    _targetYaw = autoYaw;
                    _targetZoom = autoZoom;
                    break;
                case CameraMode.Manual:
                    _inactivityTimer += Time.deltaTime;
                    if (_inactivityTimer > manualTimeout)
                        Mode = CameraMode.Auto;
                    break;
            }

            ApplyCameraTransform();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(targetBounds.center, targetBounds.size);
        }

        private void HandleInput()
        {
            var panInput = panAction.action.ReadValue<Vector2>();
            var rotateInput = rotateAction.action.ReadValue<Vector2>();
            var zoomInput = zoomAction.action.ReadValue<float>();

            if (panInput != Vector2.zero || rotateInput != Vector2.zero || zoomInput != 0f)
            {
                Mode = CameraMode.Manual;

                var right = Quaternion.Euler(0, _targetYaw, 0) * Vector3.right;
                var forward = Quaternion.Euler(0, _targetYaw, 0) * Vector3.forward;

                _targetPosition += panSpeed * (_targetZoom / 100) * (right * panInput.x + forward * panInput.y);
                _targetPosition = ClampToBounds(_targetPosition);

                _targetYaw += rotateInput.x * rotateSpeed * Time.deltaTime;
                _targetTilt = Mathf.Clamp(_targetTilt - rotateInput.y * rotateSpeed * Time.deltaTime, minTilt, maxTilt);

                _targetZoom = Mathf.Clamp(_targetZoom - zoomInput * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            }
        }

        private Vector3 ClampToBounds(Vector3 vector)
        {
            var halfFOV = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var visibleDistance = _targetPosition.y * Mathf.Tan(halfFOV);

            var halfX = visibleDistance * _camera.aspect;
            var halfZ = visibleDistance;

            vector.x = Mathf.Clamp(vector.x, targetBounds.min.x + halfX, targetBounds.max.x - halfX);
            vector.z = Mathf.Clamp(vector.z, targetBounds.min.z + halfZ, targetBounds.max.z - halfZ);
            return vector;
        }

        private void ApplyCameraTransform()
        {

            _smoothTargetPosition = Vector3.Lerp(_smoothTargetPosition, _targetPosition, autoSpeed * Time.deltaTime);
            _smoothCameraPosition = Vector3.Lerp(_smoothCameraPosition,
                _smoothTargetPosition + Quaternion.Euler(_targetTilt, _targetYaw, 0) * new Vector3(0, _targetZoom, 0),
                autoSpeed * Time.deltaTime);

            transform.position = _smoothCameraPosition;
            transform.LookAt(_smoothTargetPosition);
        }
    }
}
