using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class AnimationHandler : MonoBehaviour
    {
        public Animator _Animator;
        private StateManager mStateManager;
        private Vector3 mLookDirection;

        public void Init()
        {
            mStateManager = GetComponent<StateManager>();
            SetupAnimator();
        }

        public void Tick()
        {
            _Animator.speed = 1 * mStateManager._TimeManager._CustomTimescale;
            mStateManager._Reloading = _Animator.GetBool("Reloading");
            _Animator.SetBool("Aim", mStateManager._Aiming);
            _Animator.SetBool("OnGround", (!mStateManager._Vaulting) ? mStateManager._OnGround : true);
            _Animator.SetInteger("WeaponType", mStateManager._WeaponAnimType);

            _Animator.SetBool("ExitLocomotion", (mStateManager._Aiming || mStateManager._UnderCover) ? true : false);

            if (!mStateManager._CanRun)
            {
                _Animator.SetFloat("Forward", mStateManager._Vertical, 0.1f, mStateManager._CustomFixedDelta);
                _Animator.SetFloat("Sideways", mStateManager._Horizontal, 0.1f, mStateManager._CustomFixedDelta);
            }
            else
            {
                float InMovement = Mathf.Abs(mStateManager._Vertical) + Mathf.Abs(mStateManager._Horizontal);
                bool InWalk = mStateManager._Walk;
                InMovement = Mathf.Clamp(InMovement, 0, (InWalk || mStateManager._Reloading) ? 0.5f : 1f);
                _Animator.SetFloat("Forward", InMovement, 0.1f, mStateManager._CustomFixedDelta);
            }
            _Animator.SetBool("Cover", mStateManager._UnderCover);
            _Animator.SetInteger("CoverDirection", mStateManager._CoverDirection);
            _Animator.SetBool("CrouchToUpAim", mStateManager._CrouchCover);
            _Animator.SetFloat("Stance", mStateManager._Stance);
            _Animator.SetBool("AimAtSides", mStateManager._AimAtSides);
        }

        public void SetupAnimator(Animator pTargetAnimator = null)
        {
            _Animator = GetComponent<Animator>();
            if (_Animator == null)
            {
                Animator[] InAnimatorArr = GetComponentsInChildren<Animator>();
                int length = InAnimatorArr.Length;
                for (int i = 0; i < length; i++)
                {
                    if (InAnimatorArr[i] != _Animator)
                    {
                        _Animator.avatar = InAnimatorArr[i].avatar;
                        Destroy(InAnimatorArr[i]);
                        break;
                    }
                }
            }
            else
            {
                //_Animator.avatar = pTargetAnimator.avatar;
                //Destroy(pTargetAnimator);
            }
        }

        public void StartReload ()
        {
            if (!mStateManager._Reloading)
            {
                _Animator.SetTrigger("Reload");
            }
        }
    }
}