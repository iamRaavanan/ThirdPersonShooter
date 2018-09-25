using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class IKHandler : MonoBehaviour
    {
        private Animator                    mAnimator;
        private StateManager                mStateManager;
        private float                       mTargetWeight;
        private Transform                   mAimHelper;

        public float                        _LookWeight = 1f;
        public float                        _BodyWeight = 0.8f;
        public float                        _HeadWeight = 1f;
        public float                        _ClampWeight = 1f;

        public Transform                    _WeaponHolder;
        public Transform                    _RightShoulder;
        public Transform                    _OverrideLookTarget;
        public Transform                    _RightHandIKTarget;
        public float                        _RightHandIKWeight;
        public Transform                    _LeftHandIKTarget;
        public float                        _LeftHandIKWeight;

        private void Start()
        {
            mAimHelper = new GameObject().transform;
            mAnimator = GetComponent<Animator>();
            mStateManager = GetComponent<StateManager>();
        }

        private void FixedUpdate()
        {
            if (_RightShoulder == null)
            {
                _RightShoulder = mAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
            }
            else
            {
                _WeaponHolder.position = _RightShoulder.position;
            }

            if (mStateManager._Aiming && !mStateManager._Reloading)
            {
                Vector3 InDirectionTowardsTarget = mAimHelper.position - transform.position;
                float InAngle = Vector3.Angle(transform.forward, InDirectionTowardsTarget);
                mTargetWeight = (InAngle < 90) ? 1 : 0;
            }
            else
            {
                mTargetWeight = 0;
            }
            float InMultiplier = (mStateManager._Aiming) ? 5 : 30;
            _LookWeight = Mathf.Lerp(_LookWeight, mTargetWeight, InMultiplier * Time.deltaTime);
            _RightHandIKWeight = _LookWeight;
            _LeftHandIKWeight = 1 - mAnimator.GetFloat("LeftHandIKWeightOverride");
            HandleShoulderRotation();
        }

        private void HandleShoulderRotation()
        {
            mAimHelper.position = Vector3.Lerp(mAimHelper.position, mStateManager._LookPosition, Time.deltaTime * 5);
            _WeaponHolder.LookAt(mAimHelper.position);
            _RightHandIKTarget.parent.transform.LookAt(mAimHelper.position);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            mAnimator.SetLookAtWeight(_LookWeight, _BodyWeight, _HeadWeight, _ClampWeight);
            Vector3 InFilterDirection = mStateManager._LookPosition;
            mAnimator.SetLookAtPosition((_OverrideLookTarget != null) ? _OverrideLookTarget.position : InFilterDirection);

            if (_LeftHandIKTarget)
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _LeftHandIKWeight);
                mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, _LeftHandIKTarget.position);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _LeftHandIKWeight);
                mAnimator.SetIKRotation(AvatarIKGoal.LeftHand, _LeftHandIKTarget.rotation);
            }

            if (_RightHandIKTarget)
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, _RightHandIKWeight);
                mAnimator.SetIKPosition(AvatarIKGoal.RightHand, _RightHandIKTarget.position);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, _RightHandIKWeight);
                mAnimator.SetIKRotation(AvatarIKGoal.RightHand, _RightHandIKTarget.rotation);
            }
        }
    }
}