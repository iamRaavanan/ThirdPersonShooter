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
        private StateManager mState;

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

        private void Start()
        {
            mCrosshairManager = CrosshairManager.GetInstance();
            _CameraProperties = FreeCameraLook.GetInstance();
            _CameraPivot = _CameraProperties.transform.GetChild(0);
            _CameraTransform = _CameraPivot.GetChild(0);
            mShakeCamera = _CameraPivot.GetComponentInChildren<ShakeCamera>();

            mState = GetComponent<StateManager>();
            mLayerMask = ~(1 << gameObject.layer);
            mState._LayerMask = mLayerMask;
        }

        private void FixedUpdate()
        {
            HandleInput();
            UpdateState();
            HandleShake();

            // Camera look out point
            Ray InRay = new Ray(_CameraTransform.position, _CameraTransform.forward);
            mState._LookPosition = InRay.GetPoint(20);
            RaycastHit InHit;
            if (Physics.Raycast(InRay.origin, InRay.direction, out InHit, 100, mLayerMask))
            {
                mState._LookHitPosition = InHit.point;
            }
            else
            {
                mState._LookHitPosition = mState._LookPosition;
            }

            // Check for an obstacle infront of camera
            CameraCollision(mLayerMask);

            mCurrentZ = Mathf.Lerp(mCurrentZ, mActualZ, Time.deltaTime * 15);
            _CameraTransform.localPosition = Vector3.forward * mCurrentZ;
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
        }

        private void UpdateState()
        {
            mState._Aiming = mState._OnGround && (_Mouse2 > 0);
            mState._CanRun = !mState._Aiming;
            mState._Walk = (_Fire3 > 0);
            mState._Horizontal = _Horizontal;
            mState._Vertical = _Vertical;

            if (mState._Aiming)
            {
                mTargetZ = _CameraAimingZ;
                mTargetFOV = _AimingFOV;

                if (_Mouse1 > 0.5f && !mState._Reloading)
                {
                    mState._Shoot = true;
                }
                else
                {
                    mState._Shoot = false;
                }
            }
            else
            {
                mState._Shoot = false;
                mTargetZ = _CameraNormalZ;
                mTargetFOV = _NormalFOV;
            }
        }

        private void HandleShake()
        {
            if (mState._Shoot && mState._ShootingHandler._CurrentBullets > 0)
            {
                mTargetShake = _ShakeRecoil;
                _CameraProperties.WiggleCrosshairAndCamera(0.2f);
                mTargetFOV += 5f;
            }
            else
            {
                if (mState._Vertical != 0)
                {
                    mTargetShake = _ShakeMovement;
                }
                else
                {
                    if (mState._Horizontal != 0)
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
}
