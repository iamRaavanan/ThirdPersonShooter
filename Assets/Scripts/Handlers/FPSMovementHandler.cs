using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class FPSMovementHandler : MonoBehaviour
    {
        private StateManager mStateManager;
        private Rigidbody mRigidbody;

        private Vector3 mLookPosition;
        private Vector3 mLookDirection;
        private Vector3 mStorePosition;

        private PhysicMaterial mZFriction;
        private PhysicMaterial mMFriction;
        private Collider mCollider;

        private float mHorizontal;
        private float mVertical;

        public Transform _FpsCameraHolder;

        public float _RunSpeed = 3f;
        public float _WalkSpeed = 1.5f;
        public float _AimSpeed = 1f;
        public float _SpeedMultiplier = 10f;
        public float _RotateSpeed = 2f;
        public float _TurnSpeed = 5f;

        private void Start()
        {
            mRigidbody = GetComponent<Rigidbody>();
            mStateManager = GetComponent<StateManager>();
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
            Vector3 InVertical = _FpsCameraHolder.forward * mVertical;
            Vector3 InHorizontal = _FpsCameraHolder.right * mHorizontal;

            InVertical.y = InHorizontal.y = 0;

            HandleMovement(InHorizontal, InVertical, InGround);
            HandleRotation();

            mRigidbody.drag = (InGround) ? 4 : 0;
        }

        private void HandleMovement(Vector3 pHorizontal, Vector3 pVertical, bool inGround)
        {
            if (inGround)
            {
                mRigidbody.AddForce((pVertical + pHorizontal).normalized * GetSpeed());
            }
        }

        private void HandleRotation()
        {
            mLookDirection.y = 0;
            mLookDirection += transform.position;
            transform.LookAt(mLookDirection);
        }

        private float GetSpeed()
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
