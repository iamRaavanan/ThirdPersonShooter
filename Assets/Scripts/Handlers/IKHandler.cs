using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class IKHandler : MonoBehaviour
    {
        private Animator                    mAnimator;
        private StateManager                mStateManager;
        private float                       mTargetWeight;
        //private Transform                   mAimHelper;

        public float                        _LookWeight = 1f;
        public float                        _BodyWeight = 0.8f;
        public float                        _HeadWeight = 1f;
        public float                        _ClampWeight = 1f;

        public Transform                    _WeaponHolder;
        public Transform                    _RightShoulder;

        public bool                         _EnableTwoHandWield;
        public Vector3                      _SecondHandLookPosition;
        public Transform                    _SecondaryWeaponHolder;
        public Transform                    _LeftShoulder;

        public Transform                    _OverrideLookTarget;

        public Transform                    _RightHandIKTarget;
        public Transform                    _RightHandIKRotation;
        public Transform                    _RightElbowTarget;
        public float                        _RightHandIKWeight;
        private float                       mTargetRHWeight;

        public Transform                    _LeftHandIKTarget;
        public Transform                    _LeftElbowTarget;
        public float                        _LeftHandIKWeight;
        private float                       mTargetLHWeight;

        [HideInInspector]
        public bool                         _LHIK_dis_NotAiming;

        private Transform                   mAimHelperRS;
        private Transform                   mAimHelperLS;

        public void Init()
        {
            mAimHelperRS = new GameObject().transform;
            mAimHelperRS.name = "Right Shoulder Aim Helper";

            mAimHelperLS = new GameObject().transform;
            mAimHelperLS.name = "Left Shoulder Aim Helper";

            //mAimHelper = new GameObject().transform;
            mAnimator = GetComponent<Animator>();
            mStateManager = GetComponent<StateManager>();
        }

        public void Tick()
        {
            if (!mStateManager._MeleeWeapon)
            {
                HandleShoulders();
                AimWeight();
                HandleRightHandIKWeight();
                HandleLeftHandIKWeight();
                HandleShoulderRotation();
            }
            else
            {
                _LookWeight = 0;
            }
        }

        private void AimWeight()
        {
            if (mStateManager._Aiming && !mStateManager._Reloading)
            {
                Vector3 InDirectionTowardsTarget = mAimHelperRS.position - transform.position;
                float InAngle = Vector3.Angle(transform.forward, InDirectionTowardsTarget);
                mTargetWeight = (InAngle < 90) ? 1 : 0;
            }
            else
            {
                mTargetWeight = 0;
            }
            float InMultiplier = (mStateManager._Aiming) ? 5 : 30;
            _LookWeight = Mathf.Lerp(_LookWeight, mTargetWeight, InMultiplier * mStateManager._CustomFixedDelta);
        }

        private void HandleShoulders()
        {
            if (_RightShoulder == null)
            {
                _RightShoulder = mAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
            }
            else
            {
                _WeaponHolder.position = _RightShoulder.position;
            }
        }

        private void HandleRightHandIKWeight ()
        {
            float InMultiplier = 3;
            if (mStateManager._UnderCover)
            {
                mTargetRHWeight = 0;
                if (mStateManager._Aiming)
                {
                    mTargetRHWeight = 1;
                    InMultiplier = 2;
                }
                else
                {
                    InMultiplier = 10;
                }
            }
            else
            {
                _RightHandIKWeight = _LookWeight;
            }

            if (mStateManager._Reloading)
            {
                mTargetRHWeight = 0;
                InMultiplier = 5;
            }
            _RightHandIKWeight = Mathf.Lerp(_RightHandIKWeight, mTargetRHWeight, mStateManager._CustomFixedDelta * InMultiplier);
        }

        private void HandleLeftHandIKWeight ()
        {
            float InMultiplier = 3;
            if (mStateManager._UnderCover)
            {
                if (!_LHIK_dis_NotAiming)
                {
                    _LeftHandIKWeight = 1;
                    InMultiplier = 6;
                }
                else
                {
                    InMultiplier = 10;
                    if (mStateManager._Aiming)
                    {
                        mTargetLHWeight = 1;
                    }
                    else
                    {
                        mTargetLHWeight = _LeftHandIKWeight = 0;
                    }
                }
            }
            else
            {
                if (!_LHIK_dis_NotAiming)
                {
                    _LeftHandIKWeight = 1;
                    InMultiplier = 10;
                }
                else
                {
                    InMultiplier = 10;
                    mTargetLHWeight = (mStateManager._Aiming) ? 1 : 0;
                }
            }

            if (mStateManager._Reloading)
            {
                mTargetLHWeight = 0;
                InMultiplier = 10;
            }
            _LeftHandIKWeight = Mathf.Lerp(_LeftHandIKWeight, mTargetLHWeight, mStateManager._CustomFixedDelta * InMultiplier);
        }

        private void HandleShoulderRotation()
        {
            mAimHelperRS.position = Vector3.Lerp(mAimHelperRS.position, mStateManager._LookPosition, mStateManager._CustomFixedDelta * 5);
            _WeaponHolder.LookAt(mAimHelperRS.position);
            _RightHandIKTarget.parent.transform.LookAt(mAimHelperRS.position);

            if (_EnableTwoHandWield)
            {
                _SecondHandLookPosition = mStateManager._LookPosition;
                mAimHelperLS.position = Vector3.Lerp(mAimHelperLS.position, _SecondHandLookPosition, mStateManager._CustomFixedDelta * 5);
                _SecondaryWeaponHolder.LookAt(mAimHelperLS.position);
            }
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
            else
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }

            if (_RightHandIKTarget)
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, _RightHandIKWeight);
                mAnimator.SetIKPosition(AvatarIKGoal.RightHand, _RightHandIKTarget.position);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, _RightHandIKWeight);
                mAnimator.SetIKRotation(AvatarIKGoal.RightHand, _RightHandIKTarget.rotation);
            }
            else
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                mAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }

            if (_RightElbowTarget)
            {
                mAnimator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, _RightHandIKWeight);
                mAnimator.SetIKHintPosition(AvatarIKHint.RightElbow, _RightElbowTarget.position);
            }
            else
            {
                mAnimator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
            }

            if (_LeftElbowTarget)
            {
                mAnimator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, _LeftHandIKWeight);
                mAnimator.SetIKHintPosition(AvatarIKHint.LeftElbow, _LeftElbowTarget.position);
            }
            else
            {
                mAnimator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            }
        }
    }
}