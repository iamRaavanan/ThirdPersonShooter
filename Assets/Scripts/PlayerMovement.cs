using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class PlayerMovement : MonoBehaviour
    {
        private InputHandler mInputHandler;
        private StateManager mStateManager;
        private Rigidbody mRigidbody;

        private Vector3 mLookPosition;
        private Vector3 mLookDirection;
        private Vector3 mStoreDirection;

        private PhysicMaterial mZFriction;
        private PhysicMaterial mMFriction;
        private Collider mCollider;

        private float mHorizontal;
        private float mVertical;


        public float _RunSpeed = 3f;
        public float _WalkSpeed = 1.5f;
        public float _AimSpeed = 1f;
        public float _SpeedMultiplier = 10f;
        public float _RotateSpeed = 2f;
        public float _TurnSpeed = 5f;

        private void Start()
        {
            mInputHandler = GetComponent<InputHandler>();
            mStateManager = GetComponent<StateManager>();
            mRigidbody = GetComponent<Rigidbody>();
            mCollider = GetComponent<Collider>();

            mZFriction = mMFriction = new PhysicMaterial();
            mZFriction.dynamicFriction = mZFriction.staticFriction = 0;
            mMFriction.dynamicFriction = mMFriction.staticFriction = 1;
        }

        private void FixedUpdate()
        {
            mLookPosition = mStateManager._LookPosition;
            mLookDirection = mLookPosition - transform.position;
            mHorizontal = mStateManager._Horizontal;
            mVertical = mStateManager._Vertical;

            bool InGround = mStateManager._OnGround;

            if (mHorizontal != 0 || mVertical != 0 || !InGround)
            {
                mCollider.material = mZFriction;
            }
            else
            {
                mCollider.material = mMFriction;
            }
            Vector3 InVertical = mInputHandler._CameraTransform.forward * mVertical;
            Vector3 InHorizontal = mInputHandler._CameraTransform.right * mHorizontal;

            InVertical.y = InHorizontal.y = 0;

            HandleMovement(InHorizontal, InVertical, InGround);
            HandleRotation(InHorizontal, InVertical, InGround);

            mRigidbody.drag = (InGround) ? 4 : 0;
        }

        private void HandleMovement(Vector3 pHorizontal, Vector3 pVertical, bool inGround)
        {
            if (inGround)
            {
                mRigidbody.AddForce((pVertical + pHorizontal).normalized * GetSpeed());
            }
        }

        private void HandleRotation(Vector3 pHorizontal, Vector3 pVertical, bool inGround)
        {
            if (mStateManager._Aiming)
            {
                mLookDirection.y = 0;
                //Debug.Log("mLookDirection: " + mLookDirection);
                //Quaternion InTargetRotation = Quaternion.LookRotation(mLookDirection);
                //transform.rotation = Quaternion.Slerp(mRigidbody.rotation, InTargetRotation, Time.deltaTime * _RotateSpeed);
            }
            else
            {
                mStoreDirection = transform.position + pHorizontal + pVertical;
                Vector3 InDirection = mStoreDirection - transform.position;
                InDirection.y = 0;

                if (mHorizontal != 0 || mVertical != 0)
                {
                    float InAngle = Vector3.Angle(transform.forward, InDirection);
                    if (InAngle != 0)
                    {
                        float InAngle1 = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(InDirection));
                        if (InAngle1 != 0)
                        {
                            mRigidbody.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(InDirection), _TurnSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }

        private float GetSpeed ()
        {
            float InSpeed = 0;
            if (mStateManager._Aiming)
            {
                InSpeed = _AimSpeed;
            }
            else
            {
                InSpeed = ((mStateManager._Walk || mStateManager._Reloading) ? _WalkSpeed : _RunSpeed);
            }
            InSpeed *= _SpeedMultiplier;
            return InSpeed;
        }
    }
}
