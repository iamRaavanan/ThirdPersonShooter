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

        // Cover Vairables
        public float _CoverAcceleration = 0.5f;
        public float _CoverMaxSpeed = 2;
        private List<CoverPosition> mIgnoreCover = new List<CoverPosition>();

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

            if (!mStateManager._UnderCover)
            {
                ProcessMovementRotation();

                if (mHorizontal != 0 || mVertical != 0)
                {
                    //LookForCover();
                    SearchForCover();
                }
            }
            else
            {
                if (!mStateManager._Aiming)
                {
                    HandleCoverMovement();
                }
                GetOffFromCover();
            }
        }

        private void ProcessMovementRotation()
        {
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

        private void HandleCoverMovement ()
        {
            if (mHorizontal != 0)
            {
                mStateManager._CoverDirection = (mHorizontal < 0) ? -1 : 1;
            }
            // Now, cover path is not linear, this logic is now obsolete
            #region Obsolete
            //Quaternion InTargetRotation = Quaternion.LookRotation(mStateManager._CoverPosition._StartTransform.forward);
            //transform.rotation = Quaternion.Slerp(transform.rotation, InTargetRotation, Time.deltaTime * _RotateSpeed);
            #endregion

            float InWallLength = mStateManager._CoverPosition._Length;

            float InMovement = ((mHorizontal * _CoverAcceleration) * _CoverMaxSpeed) * Time.deltaTime;

            float InLerpMovement = InMovement / InWallLength;

            mStateManager._CoverPercentage -= InLerpMovement;

            mStateManager._CoverPercentage = Mathf.Clamp01(mStateManager._CoverPercentage);

            Vector3 InCurvePathPosition = mStateManager._CoverPosition._CurvePath.GetPointAt(mStateManager._CoverPercentage);
            //Vector3 InTargetPos = Vector3.Lerp(mStateManager._CoverPosition._StartTransform.position, mStateManager._CoverPosition._EndTransform.position, mStateManager._CoverPercentage);
            InCurvePathPosition.y = transform.position.y;

            HandleCurveRotation();

            transform.position = InCurvePathPosition;   /*InTargetPos*/
        }

        private void HandleCurveRotation()
        {
            float InFwdPercentage = mStateManager._CoverPercentage + 0.1f;
            InFwdPercentage = (InFwdPercentage > 0.99f) ? 1.0f : InFwdPercentage;

            Vector3 InCurrentPos = mStateManager._CoverPosition._CurvePath.GetPointAt(mStateManager._CoverPercentage);
            Vector3 InForwardPos = mStateManager._CoverPosition._CurvePath.GetPointAt(InFwdPercentage);
            Vector3 InDirection = Vector3.Cross(InCurrentPos, InForwardPos);
            InDirection.y = 0;

            Quaternion InTargetRotation = Quaternion.LookRotation(InDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, InTargetRotation, Time.deltaTime * _RotateSpeed);
        }

        private void LookForCover ()
        {
            Vector3 InOrigin = transform.position + Vector3.up / 2;
            Vector3 InDirection = transform.forward;
            RaycastHit InHit;
            if (Physics.Raycast (InOrigin, InDirection, out InHit, 2))
            {
                CoverPosition InHitCoverPos = InHit.transform.GetComponent<CoverPosition>();
                if (InHitCoverPos)
                {
                    if (!mIgnoreCover.Contains (InHitCoverPos))
                    {
                        mStateManager.GetInCover(InHitCoverPos);
                        mIgnoreCover.Add(InHitCoverPos);
                    }
                }
            }
        }

        private void GetOffFromCover ()
        {
            if (mVertical < -0.5f)
            {
                if (!mStateManager._Aiming)
                {
                    mStateManager._CoverPosition = null;
                    mStateManager._UnderCover = false;
                    StartCoroutine("ClearIgnoreList");
                }
            }
        }

        private void SearchForCover ()
        {
            Vector3 InOrigin = transform.position + Vector3.up / 2;
            Vector3 InDirection = transform.forward;
            RaycastHit InHit;

            // Update raycast distance 
            if (Physics.Raycast (InOrigin, InDirection, out InHit, 2))
            {
                CoverPosition InCoverPosition = InHit.transform.GetComponentInParent<CoverPosition>();
                if (InCoverPosition)
                {
                    if (!mIgnoreCover.Contains (InCoverPosition))
                    {
                        mStateManager.GetInCover(InCoverPosition);
                        mIgnoreCover.Add(InCoverPosition);
                    }
                }
            }
        }

        private IEnumerator ClearIgnoreList ()
        {
            yield return new WaitForSeconds(1);
            mIgnoreCover.Clear();
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
            if (mStateManager._Aiming && !mStateManager._UnderCover)
            {
                mLookDirection.y = 0;
                //Debug.Log("mLookDirection: " + mLookDirection);
                Quaternion InTargetRotation = Quaternion.LookRotation(mLookDirection);
                transform.rotation = Quaternion.Slerp(mRigidbody.rotation, InTargetRotation, Time.deltaTime * _RotateSpeed);
            }
            else
            {
                if (!mStateManager._UnderCover)
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
