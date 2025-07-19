using Load;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

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
        public float manualTimeout = 5f;

        [Header("Control Bounds")]
        public float minZoom = 20f;
        public MinMax tiltRange = new(20f, 80f);

        [Header("Control Smoothing")]
        public float targetSmoothing = 2f;
        public float cameraSmoothing = 80f;

        private Camera _camera;
        private CameraSettings _cameraSettings;
        private Vector3 _smoothTargetPosition;
        private Vector3 _smoothCameraPosition;
        public Vector3? FocusTarget;
        public Bounds cameraBounds;

        [Header("Input Actions")]
        public InputActionReference panAction;
        public InputActionReference rotateAction;
        public InputActionReference zoomAction;
        private float _inactivityTimer;

        private Vector3 _targetPosition, _currentPosition;
        private float _targetZoom, _currentZoom;
        private float _targetTilt, _currentTilt;
        private float _targetYaw, _currentYaw;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _cameraSettings = SaveManager.Settings.Value.Camera.Value;
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
                    var focus = ClampPosition(FocusTarget ?? cameraBounds.center);
                    _targetPosition = new Vector3(focus.x, 0, focus.z);
                    _targetTilt = autoTilt;
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
            Gizmos.DrawWireCube(cameraBounds.center, cameraBounds.size);
        }

        private void HandleInput()
        {
            var panInput = panAction.action.ReadValue<Vector2>() * (_cameraSettings.InvertPan.Value ? -1 : 1);
            var rotateInput = rotateAction.action.ReadValue<Vector2>() * (_cameraSettings.InvertRotate.Value ? -1 : 1);
            var zoomInput = zoomAction.action.ReadValue<float>() * (_cameraSettings.InvertZoom.Value ? -1 : 1);

            if (panInput != Vector2.zero || rotateInput != Vector2.zero || zoomInput != 0f)
            {
                Mode = CameraMode.Manual;

                var right = Quaternion.Euler(0, _targetYaw, 0) * Vector3.right;
                var forward = Quaternion.Euler(0, _targetYaw, 0) * Vector3.forward;

                _targetPosition += _cameraSettings.PanSpeed.Value * (_targetZoom / 100) * (right * panInput.x + forward * panInput.y);
                _targetPosition = ClampPosition(_targetPosition);

                _targetYaw += rotateInput.x * _cameraSettings.RotateSpeed.Value * Time.deltaTime;
                _targetTilt = tiltRange.Clamp(_targetTilt - rotateInput.y * _cameraSettings.RotateSpeed.Value * Time.deltaTime);

                _targetZoom = ClampZoom(_targetZoom - zoomInput * _cameraSettings.ZoomSpeed.Value * Time.deltaTime);
            }
        }

        private Vector3 ClampPosition(Vector3 vector)
        {
            var halfFOV = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var visibleDistance = _targetZoom * Mathf.Tan(halfFOV);

            var halfX = visibleDistance * _camera.aspect;
            var halfZ = visibleDistance;

            vector.x = Mathf.Clamp(vector.x, cameraBounds.min.x + halfX, cameraBounds.max.x - halfX);
            vector.z = Mathf.Clamp(vector.z, cameraBounds.min.z + halfZ, cameraBounds.max.z - halfZ);
            return vector;
        }

        private float ClampZoom(float zoom)
        {
            var halfFOV = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;

            var maxVisibleDepth = Mathf.Tan(halfFOV);
            var maxVisibleWidth = Mathf.Tan(halfFOV) * _camera.aspect;

            var maxZoomZ = (cameraBounds.size.z / 2f) / maxVisibleDepth;
            var maxZoomX = (cameraBounds.size.x / 2f) / maxVisibleWidth;

            var maxAllowedZoom = Mathf.Min(maxZoomX, maxZoomZ);

            return Mathf.Clamp(zoom, minZoom, maxAllowedZoom);
        }

        private void ApplyCameraTransform()
        {
            _currentPosition = Vector3.Lerp(_currentPosition, _targetPosition, targetSmoothing * Time.deltaTime);
            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, targetSmoothing * Time.deltaTime);
            _currentTilt = Mathf.Lerp(_currentTilt, _targetTilt, targetSmoothing * Time.deltaTime);
            _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, targetSmoothing * Time.deltaTime);

            _smoothTargetPosition = Vector3.Lerp(_smoothTargetPosition, _currentPosition, targetSmoothing * Time.deltaTime);
            _smoothCameraPosition = Vector3.Lerp(_smoothCameraPosition,
                _smoothTargetPosition + Quaternion.Euler(_currentTilt, _currentYaw, 0) * new Vector3(0, _currentZoom, 0),
                cameraSmoothing * Time.deltaTime);

            transform.position = _smoothCameraPosition;
            transform.LookAt(_smoothTargetPosition);
        }
    }
}
