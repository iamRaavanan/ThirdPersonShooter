using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class InputHandler : MonoBehaviour
    {
        public float _Horizontal;
        public float _Vertical;
        public float _Mouse1;
        public float _Mouse2;
        public float _Fire3;
        public float _MiddleMouse;
        public float _MouseX;
        public float _MouseY;

        [HideInInspector]
        public FreeCameraLook _CameraProperties;
        [HideInInspector]
        public Transform _CameraPivot;
        [HideInInspector]
        public Transform _CameraTransform;

        private CrosshairManager mCrosshairManager;
        private ShakeCamera mShakeCamera;
        private StateManager mStateManager;

        public float _NormalFOV = 60;
        public float _AimingFOV = 40;
        public float _CameraNormalZ = -2;
        public float _CameraAimingZ = -0.86f;
        public float _ShakeRecoil = 0.5f;
        public float _ShakeMovement = 0.3f;
        public float _ShakeMin = 0.1f;

        private float mTargetFOV;
        private float mCurrentFOV;
        private float mTargetZ;
        private float mActualZ;
        private float mCurrentZ;
        private LayerMask mLayerMask;
        private float mTargetShake;
        private float mCurrentShake;

        public CameraPropertyValues _CameraValues;

        public bool _LeftPivot;
        public bool _ChangePivot;
        public bool _Crouch;

        public bool _FPSMode;
        private bool mCanSwitch;
        private CameraSwitchControl mCamSwitchControl;

        private void Start()
        {
            mCrosshairManager = CrosshairManager.GetInstance();
            _CameraProperties = FreeCameraLook.GetInstance();
            _CameraPivot = _CameraProperties.transform.GetChild(0);
            _CameraTransform = _CameraPivot.GetChild(0);
            mShakeCamera = _CameraPivot.GetComponentInChildren<ShakeCamera>();

            mStateManager = GetComponent<StateManager>();
            mLayerMask = ~(1 << gameObject.layer);
            mStateManager._LayerMask = mLayerMask;

            mCamSwitchControl = CameraSwitchControl._Instace;
            if (mCamSwitchControl != null)
            {
                mCanSwitch = true;
            }
            _CameraValues._StartingPivotPosition = _CameraPivot.localPosition;
        }

        private void FixedUpdate()
        {
            HandleInput();
            UpdateStates();
            HandleShake();

            // Camera look out point
            Ray InRay = new Ray(_CameraTransform.position, _CameraTransform.forward);
            mStateManager._LookPosition = InRay.GetPoint(20);
            RaycastHit InHit;
            if (Physics.Raycast(InRay.origin, InRay.direction, out InHit, 100, mLayerMask))
            {
                mStateManager._LookHitPosition = InHit.point;
            }
            else
            {
                mStateManager._LookHitPosition = mStateManager._LookPosition;
            }
            if (!_FPSMode)
            {
                // Check for an obstacle infront of camera
                CameraCollision(mLayerMask);

                mCurrentZ = Mathf.Lerp(mCurrentZ, mActualZ, Time.deltaTime * 15);
                _CameraTransform.localPosition = Vector3.forward * mCurrentZ;

                // Camera value on Cover Position & Camera pivot position update based on that
                _CameraValues._TargetCameraOffset = Vector3.zero;

                float InPivotX = (!_LeftPivot) ? _CameraValues._StartingPivotPosition.x : -_CameraValues._StartingPivotPosition.x;
                float InPivotY = (!mStateManager._Crouching) ? _CameraValues._StartingPivotPosition.y : (_CameraValues._StartingPivotPosition.y - 0.5f);
                if (mStateManager._UnderCover)
                {
                    //Local pivot position updates based on the where user is looking
                    InPivotX = _CameraValues._StartingPivotPosition.x * mStateManager._CoverDirection;
                    _CameraProperties._UnderCover = _CameraProperties._OverrideTarget = mStateManager._Aiming;
                    _CameraProperties._CoverDirection = mStateManager._CoverDirection;
                    _CameraProperties._OverrideTarget = mStateManager._Aiming;

                    if (mStateManager._Aiming)
                    {
                        Vector3 InLocalPosition = Vector3.zero;
                        if (mStateManager._CrouchCover && !mStateManager._AimAtSides)
                        {
                            InPivotY = _CameraValues._StartingPivotPosition.y;
                            _CameraProperties._CoverAngleMin = -50;
                            _CameraProperties._CoverAngleMax = 50;
                        }
                        else
                        {
                            InLocalPosition = new Vector3(_CameraValues._CoverCameraOffsetX * mStateManager._CoverDirection,
                                0, _CameraValues._CoverCameraOffsetZ);
                            _CameraProperties._CoverAngleMin = (mStateManager._CoverDirection < 0) ? _CameraValues._CoverLeftMinAngle : _CameraValues._CoverRightMinAngle;
                            _CameraProperties._CoverAngleMax = (mStateManager._CoverDirection < 0) ? _CameraValues._CoverLeftMaxAngle : _CameraValues._CoverRightMaxAngle;
                        }
                        Vector3 InWorldPosition = transform.TransformDirection(InLocalPosition);
                        InWorldPosition.y = _CameraValues._CoverCameraOffsetY;
                        _CameraValues._TargetCameraOffset = InWorldPosition;
                    }
                }
                Vector3 InTargetPivotPosition = new Vector3(InPivotX, InPivotY, _CameraValues._StartingPivotPosition.z);
                _CameraPivot.localPosition = Vector3.Lerp(_CameraPivot.localPosition, InTargetPivotPosition, Time.deltaTime * 3);
                _CameraProperties._NewTargetPosition = Vector3.Lerp(_CameraProperties._NewTargetPosition, 
                    _CameraValues._TargetCameraOffset, Time.deltaTime * 3);
            }
        }

        private void HandleInput()
        {
            _Horizontal = Input.GetAxis("Horizontal");
            _Vertical = Input.GetAxis("Vertical");
            _Mouse1 = Input.GetAxis("Fire1");
            _Mouse2 = Input.GetAxis("Fire2");
            _MiddleMouse = Input.GetAxis("Mouse ScrollWheel");
            _MouseX = Input.GetAxis("Mouse X");
            _MouseY = Input.GetAxis("Mouse Y");
            _Fire3 = Input.GetAxis("Fire3");
            _Crouch = Input.GetKeyDown(KeyCode.C);

            if (mCanSwitch)
            {
                if (Input.GetKeyUp (KeyCode.F))
                {
                    Ray InRay = new Ray(_CameraTransform.position, _CameraTransform.forward);
                    Vector3 InLookPosition = InRay.GetPoint(20);
                    if (!_FPSMode)
                    {
                        mCamSwitchControl.SwitchToFPS(InLookPosition);
                    }
                    else
                    {
                        mCamSwitchControl.SwitchToTPS(InLookPosition);
                    }
                }
            }

            if (mStateManager._UnderCover)
            {
                _ChangePivot = false;
                if (mStateManager._CoverPosition._CoverType == CoverPosition.CoverType.E_Half)
                {
                    mStateManager._CrouchCover = true;
                }
                else
                {
                    mStateManager._CrouchCover = false;
                    mStateManager._AimAtSides = false;
                }
                    

                if (mStateManager._Aiming)
                {
                    _Horizontal = 0;
                }
                else
                {
                    if (mStateManager._CoverPercentage > 0.99f)
                    {
                        if (!mStateManager._CoverPosition._IsBlockEndPosition)
                        {
                            mStateManager._CanAnim = true;
                            mStateManager._AimAtSides = true;
                        }
                        _Horizontal = Mathf.Clamp(_Horizontal, 0, 1);
                    }
                    else
                    {
                        if (mStateManager._CoverPercentage < 0.1f)
                        {
                            _Horizontal = Mathf.Clamp(_Horizontal, -1, 0);
                            if (!mStateManager._CoverPosition._IsBlockStartPosition)
                            {
                                mStateManager._CanAnim = true;
                                mStateManager._AimAtSides = true;
                            }
                        }
                        else
                        {
                            mStateManager._AimAtSides = false;
                        }
                    }
                }
            }
            else
            {
                // If not in cover we can proceed with normal aiming and no longer need crouch
                mStateManager._CrouchCover = false;
                mStateManager._CanAnim = false;
                
                if (Input.GetKeyDown (KeyCode.E))
                {
                    _ChangePivot = !_ChangePivot;
                }
                _LeftPivot = _ChangePivot;
            }
        }

        private void UpdateStates()
        {
            mStateManager._CanRun = !mStateManager._Aiming;
            if (!mStateManager._UnderCover)
            {
                if (!mStateManager._DontRun)
                {
                    mStateManager._Walk = (_Fire3 > 0);
                }
                else
                {
                    mStateManager._Walk = true;
                }
            }            
            mStateManager._Horizontal = _Horizontal;
            mStateManager._Vertical = _Vertical;

            if (mStateManager._UnderCover)
            {
                if (mStateManager._CrouchCover)
                {
                    mStateManager._Aiming = (_Mouse2 > 0) ? true : false;
                }
                else
                {
                    mStateManager._Aiming = (_Mouse2 > 0 && mStateManager._CanAnim) ? true : false;
                }                
            }
            else
            {
                mStateManager._Aiming = mStateManager._OnGround && (_Mouse2 > 0);
            }

            if (mStateManager._Aiming)
            {
                mTargetZ = _CameraAimingZ;
                mTargetFOV = _AimingFOV;

                mStateManager._Shoot = (_Mouse1 > 0.5f && !mStateManager._Reloading) ? true : false;
            }
            else
            {
                mStateManager._Shoot = false;
                mTargetZ = _CameraNormalZ;
                mTargetFOV = _NormalFOV;
            }

            if(_Crouch)
            {
                mStateManager._Crouching = !mStateManager._Crouching;
            }

            if (mStateManager._CrouchCover)
            {
                mStateManager._Crouching = true;
            }
        }

        private void HandleShake()
        {
            if (mStateManager._ActualShooting && mStateManager._ShootingHandler._CurrentBullets > 0)
            {
                mTargetShake = _ShakeRecoil;
                _CameraProperties.WiggleCrosshairAndCamera(0.2f);
                mTargetFOV += 5f;
            }
            else
            {
                if (mStateManager._Vertical != 0)
                {
                    mTargetShake = _ShakeMovement;
                }
                else
                {
                    if (mStateManager._Horizontal != 0)
                    {
                        mTargetShake = _ShakeMovement;
                    }
                    else
                    {
                        mTargetShake = _ShakeMin;
                    }
                }
            }
            mCurrentShake = Mathf.Lerp(mCurrentShake, mTargetShake, Time.deltaTime * 10);
            mShakeCamera.positionShakeSpeed = mCurrentShake;

            mCurrentFOV = Mathf.Lerp(mCurrentFOV, mTargetFOV, Time.deltaTime * 5);
            Camera.main.fieldOfView = mCurrentFOV;
        }

        private void CameraCollision(LayerMask pLayerMask)
        {
            // Raycast from pivot of camera
            Vector3 InOrigin = _CameraPivot.TransformPoint(Vector3.zero);
            Vector3 InDirection = _CameraTransform.TransformPoint(Vector3.zero) - _CameraPivot.TransformPoint(Vector3.zero);
            RaycastHit InHit;

            mActualZ = mTargetZ;

            if (Physics.Raycast(InOrigin, InDirection, out InHit, Mathf.Abs(mTargetZ), mLayerMask))
            {
                // Getting distance, if ray hits something
                float InDistance = Vector3.Distance(_CameraPivot.position, InHit.point);
                mActualZ = -InDistance;
            }
        }
    }

    [System.Serializable]
    public class CameraPropertyValues
    {
        public float _CoverCameraOffsetX = 0.2f;
        public float _CoverCameraOffsetZ = -0.2f;
        public float _CoverCameraOffsetY = 0f;

        public Vector3 _TargetCameraOffset;
        public Vector3 _StartingPivotPosition;

        public float _CoverLeftMaxAngle = 30;
        public float _CoverLeftMinAngle = -30;
        public float _CoverRightMaxAngle = 30;
        public float _CoverRightMinAngle = -30;
    }
}
