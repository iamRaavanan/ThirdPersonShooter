using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class AnimationHandler : MonoBehaviour
    {
        private Animator mAnimator;
        private StateManager mStateManager;
        private Vector3 mLookDirection;

        private void Start()
        {
            mStateManager = GetComponent<StateManager>();
            SetupAnimator();
        }

        private void FixedUpdate()
        {
            mStateManager._Reloading = mAnimator.GetBool("Reloading");
            mAnimator.SetBool("Aim", mStateManager._Aiming);
            if (!mStateManager._CanRun)
            {
                mAnimator.SetFloat("Forward", mStateManager._Vertical, 0.1f, Time.deltaTime);
                mAnimator.SetFloat("Sideways", mStateManager._Horizontal, 0.1f, Time.deltaTime);
            }
            else
            {
                float InMovement = Mathf.Abs(mStateManager._Vertical) + Mathf.Abs(mStateManager._Horizontal);
                bool InWalk = mStateManager._Walk;
                InMovement = Mathf.Clamp(InMovement, 0, (InWalk || mStateManager._Reloading) ? 0.5f : 1f);
                mAnimator.SetFloat("Forward", InMovement, 0.1f, Time.deltaTime);
            }
            mAnimator.SetBool("Cover", mStateManager._UnderCover);
            mAnimator.SetInteger("CoverDirection", mStateManager._CoverDirection);
            mAnimator.SetBool("CrouchToUpAim", mStateManager._CrouchCover);
            mAnimator.SetFloat("Stance", mStateManager._Stance);
            mAnimator.SetBool("AimAtSides", mStateManager._AimAtSides);
        }

        private void SetupAnimator()
        {
            mAnimator = GetComponent<Animator>();
            Animator[] InAnimatorArr = GetComponentsInChildren<Animator>();
            int length = InAnimatorArr.Length;
            for (int i = 0; i < length; i++)
            {
                if (InAnimatorArr[i] != mAnimator)
                {
                    mAnimator.avatar = InAnimatorArr[i].avatar;
                    Destroy(InAnimatorArr[i]);
                    break;
                }
            }
        }

        public void StartReload ()
        {
            if (!mStateManager._Reloading)
            {
                mAnimator.SetTrigger("Reload");
            }
        }
    }
}