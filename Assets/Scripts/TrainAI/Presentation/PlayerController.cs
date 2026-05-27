using TrainAI.SO.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TrainAI.Presentation
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] float moveSpeed = 4f;
        [SerializeField] float gravity = -12f;
        [SerializeField] PlayerStateRSO playerState;

        [Header("Camera")]
        [SerializeField] Transform cameraRig;

        [Header("Mobile")]
        [SerializeField] Joystick joystick;

        [Header("Sprint")]
        [SerializeField] float sprintSpeedMult = 1.6f;
        [SerializeField] float joystickSprintHoldTime = 5f;
        [SerializeField] float sprintTransitionSpeed = 2f;
        [SerializeField] float joystickAngleTolerance = 60f;

        CharacterController _cc;
        Vector3 _velocity;
        float _currentSpeedMult = 1f;
        float _joystickHoldTimer = 0f;
        Vector2 _lastJoystickDir = Vector2.zero;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (cameraRig == null)
            {
                var rig = GameObject.Find("CameraRig");
                if (rig != null) cameraRig = rig.transform;
            }
        }

        void Start()
        {
            // Joystick lives on the persistent UICanvas (DontDestroyOnLoad scene),
            // so it isn't available at Awake before this scene's Start. Resolve here.
            if (joystick == null)
                joystick = Object.FindFirstObjectByType<Joystick>(FindObjectsInactive.Include);

            // If player state exists and last position is essentially zero (new game),
            // try to spawn at the designated spawn point.
            if (playerState != null && playerState.lastWorldPos.sqrMagnitude < 0.1f)
            {
                var spawnPoint = GameObject.Find("Vitri_xuat_hien_Player");
                if (spawnPoint != null)
                {
                    // Temporarily disable CharacterController to warp
                    if (_cc != null) _cc.enabled = false;
                    
                    // Add an offset in Y axis so they spawn slightly above the marker 
                    // and safely fall down, avoiding clipping under the floor.
                    Vector3 safePos = spawnPoint.transform.position + Vector3.up * 2f;
                    
                    transform.position = safePos;
                    transform.rotation = spawnPoint.transform.rotation;
                    playerState.lastWorldPos = safePos;
                    
                    if (_cc != null) _cc.enabled = true;
                }
            }
        }

        void Update()
        {
            Vector2 input = ReadMove();

            // Sprint Logic
            bool isKbSprinting = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            bool isJoystickSprinting = false;

            if (input.sqrMagnitude < 0.01f)
            {
                _joystickHoldTimer = 0f;
            }
            else
            {
                if (Vector2.Angle(_lastJoystickDir, input.normalized) > joystickAngleTolerance)
                    _joystickHoldTimer = 0f;
                else
                    _joystickHoldTimer += Time.deltaTime;
                    
                _lastJoystickDir = input.normalized;
                
                if (_joystickHoldTimer >= joystickSprintHoldTime)
                    isJoystickSprinting = true;
            }

            bool isSprinting = isKbSprinting || isJoystickSprinting;
            float targetMult = isSprinting ? sprintSpeedMult : 1f;
            _currentSpeedMult = Mathf.Lerp(_currentSpeedMult, targetMult, Time.deltaTime * sprintTransitionSpeed);

            Vector3 forward, right;
            if (cameraRig != null)
            {
                forward = Vector3.ProjectOnPlane(cameraRig.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraRig.right, Vector3.up).normalized;
                if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
                if (right.sqrMagnitude < 0.01f) right = Vector3.right;
            }
            else
            {
                forward = Vector3.forward;
                right = Vector3.right;
            }

            Vector3 wish = (forward * input.y + right * input.x) * (moveSpeed * _currentSpeedMult);
            if (wish.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(wish), 0.2f);

            _velocity.x = wish.x;
            _velocity.z = wish.z;
            _velocity.y = _cc.isGrounded ? -2f : _velocity.y + gravity * Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);

            if (playerState != null) playerState.lastWorldPos = transform.position;
        }

        Vector2 ReadMove()
        {
            Vector2 v = Vector2.zero;

            // Block movement input if user is typing in a UI input field
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null && es.currentSelectedGameObject != null)
            {
                var obj = es.currentSelectedGameObject;
                if (obj.GetComponent("TMP_InputField") != null || obj.GetComponent("InputField") != null)
                    return v;
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  v.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
            }
            var gp = Gamepad.current;
            if (gp != null && v == Vector2.zero) v = gp.leftStick.ReadValue();
            if (joystick != null && v == Vector2.zero) v = joystick.Direction;
            return Vector2.ClampMagnitude(v, 1f);
        }
    }
}
