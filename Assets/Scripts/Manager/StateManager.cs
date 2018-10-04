using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class StateManager : MonoBehaviour
    {

        public bool _Aiming;
        public bool _CanRun;
        public bool _DontRun;
        public bool _Walk;
        public bool _Shoot;
        public bool _ActualShooting;
        public bool _Reloading;
        public bool _OnGround;

        public bool _Crouching;
        public bool _CrouchCover;
        public bool _AimAtSides;

        public float _Stance;
        public float _CoverPercentage;
        public CoverPosition _CoverPosition;
        public bool _UnderCover;
        public int _CoverDirection;
        public bool _CanAnim;

        public float _Horizontal;
        public float _Vertical;
        public Vector3 _LookPosition;
        public Vector3 _LookHitPosition;
        public LayerMask _LayerMask;

        public AudioManager _AudioManager;
        [HideInInspector]
        public ShootingHandler _ShootingHandler;
        [HideInInspector]
        public AnimationHandler _AnimationHanlder;

        private float mTargetStance;

        private void Start()
        {
            _AudioManager = GetComponent<AudioManager>();
            _ShootingHandler = GetComponent<ShootingHandler>();
            _AnimationHanlder = GetComponent<AnimationHandler>();
        }

        private void FixedUpdate()
        {
            _OnGround = IsOnGround();
            _Walk = _UnderCover;
            HandleStance();
        }

        private void HandleStance()
        {
            mTargetStance = (!_Crouching) ? 1 : 0;
            _Stance = Mathf.Lerp(_Stance, mTargetStance, Time.deltaTime * 3);
        }

        public void GetInCover (CoverPosition pCoverPosition)
        {
            float InDstFromStart = Vector3.Distance (transform.position, pCoverPosition._CurvePath.GetPointAt (0));
            _CoverPercentage = InDstFromStart / pCoverPosition._Length;
            #region Obsolete
            //Vector3 InDirection = pCoverPosition._EndTransform.position - pCoverPosition._StartTransform.position;
            //InDirection.Normalize();
            //Vector3 InTargetPosition = (InDirection * InDstFromStart) + pCoverPosition._StartTransform.position;
            #endregion
            Vector3 InTargetPos = pCoverPosition._CurvePath.GetPointAt(_CoverPercentage);
            StartCoroutine(MoveCoverByPercentage(InTargetPos));
            _CoverPosition = pCoverPosition;
            _UnderCover = true;
        }
         
        private IEnumerator MoveCoverByPercentage (Vector3 pTargetPosition)
        {
            Vector3 InStartPos = transform.position;
            Vector3 InTargetPos = pTargetPosition;
            InTargetPos.y = transform.position.y;
            float InTime = 0;
            while (InTime < 1)
            {
                InTime += Time.deltaTime * 5;
                transform.position = Vector3.Lerp(InStartPos, InTargetPos, InTime);
                yield return null;
            }
        }

        private bool IsOnGround()
        {
            bool InResult = false;
            Vector3 InOrigin = transform.position + (Vector3.up * 0.05f);
            RaycastHit InHit;
            if (Physics.Raycast(InOrigin, -Vector3.up, out InHit, 0.5f, _LayerMask))
            {
                InResult = true;
            }
            return InResult;
        }
    }
}
