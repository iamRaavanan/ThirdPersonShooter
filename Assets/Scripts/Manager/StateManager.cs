using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
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

        public bool _Vaulting;
        public BezierCurve _VaultCurve;
        public BezierCurve _ClimbCurve;

        public float _Stance;
        public float _CoverPercentage;
        public CoverPosition _CoverPosition;
        public bool _UnderCover;
        public int _CoverDirection;
        public bool _CanAnim;

        public bool _MeleeWeapon;

        public float _Horizontal;
        public float _Vertical;
        public Vector3 _LookPosition;
        public Vector3 _LookHitPosition;
        public LayerMask _LayerMask;

        public AudioManager _AudioManager;

        [HideInInspector]
        public int _WeaponAnimType;

        [HideInInspector]
        public ShootingHandler _ShootingHandler;
        [HideInInspector]
        public AnimationHandler _AnimationHanlder;
        [HideInInspector]
        public WeaponManager _WeaponManager;
        [HideInInspector]
        public IKHandler _IKHander;
        [HideInInspector]
        public PlayerMovement _PlayerMovement;
        [HideInInspector]
        public TimeManager _TimeManager;

        [HideInInspector]
        public GameObject _Model;
        public bool _SwitchCharacter;
        public int _TargetCharIndex;

        private float mTargetStance;
        private bool mClimb;
        private bool mInitVault;
        private Vector3 mCurvePosition;

        private float mPercentage;
        private bool mIgnoreVault;
        public float _CustomFixedDelta;

        private void Start()
        {
            _TimeManager = TimeManager.GetInstance();
            _Model = transform.GetChild(0).gameObject;

            _AudioManager = GetComponent<AudioManager>();
            _ShootingHandler = GetComponent<ShootingHandler>();
            _AnimationHanlder = GetComponent<AnimationHandler>();
            _WeaponManager = GetComponent<WeaponManager>();
            _IKHander = GetComponent<IKHandler>();
            _PlayerMovement = GetComponent<PlayerMovement>();

            if (_AudioManager)
                _AudioManager.Init();
            if (_ShootingHandler)
                _ShootingHandler.Init();
            if (_AnimationHanlder)
                _AnimationHanlder.Init();
            if (_WeaponManager)
                _WeaponManager.Init();
            if (_IKHander)
                _IKHander.Init();
            if (_PlayerMovement)
                _PlayerMovement.Init();

            if (GetComponent<InputHandler>())
            {
                if (ResourceManager.GetInstance())
                {
                    ResourceManager.GetInstance().SwitchCharacterOnIndex(this, _TargetCharIndex);
                }
            }

            if (_VaultCurve)
                _VaultCurve.transform.parent = null;
            if (_ClimbCurve)
                _ClimbCurve.transform.parent = null;
        }

        private void FixedUpdate()
        {
            _CustomFixedDelta = _TimeManager.GetCustomFixedDelta();

            if (_ShootingHandler)
                _ShootingHandler.Tick();
            if (_WeaponManager)
                _WeaponManager.Tick();
            if (_AnimationHanlder)
                _AnimationHanlder.Tick();
            if (_AudioManager)
                _AudioManager.Tick();
            if (_IKHander)
                _IKHander.Tick();
            if (_PlayerMovement)
                _PlayerMovement.Tick();

            _OnGround = IsOnGround();
            _Walk = _UnderCover;
            HandleStance();
            HandleVault();
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
         
        public void Vault (bool pClimb = false)
        {
            this.mClimb = false;
            this.mClimb = pClimb;

            BezierCurve InCurve = (pClimb) ? _ClimbCurve : _VaultCurve;

            InCurve.transform.rotation = transform.rotation;
            InCurve.transform.position = transform.position;

            string InDesiredAnimation = (pClimb) ? "Climb" : "Vault";

            _AnimationHanlder._Animator.CrossFade(InDesiredAnimation, 0.2f);
            InCurve.close = false;
            _CoverPercentage = 0;
            _Vaulting = true;
        }

        private void HandleVault ()
        {
            if (_Vaulting)
            {
                BezierCurve InCurve = (mClimb) ? _ClimbCurve : _VaultCurve;
                float InLineLength = InCurve.length;
                float InSpeedModifier = _AnimationHanlder._Animator.GetFloat("CurveSpeed");
                float InSpeed = (mClimb) ? 4 * InSpeedModifier : 6;
                float InMovement = InSpeed * Time.deltaTime;
                float InLerpMovement = InMovement / InLineLength;
                mPercentage += InLerpMovement;
                if (mPercentage > 1)
                {
                    _Vaulting = false;
                }
                Vector3 InTargetPosition = InCurve.GetPointAt(mPercentage);
                transform.position = InTargetPosition;
            }
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
