using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class ShootingHandler : MonoBehaviour
    {
        private StateManager mStateManager;        
        private float mTimer;
        private bool mShoot;
        private bool mDontShoot;
        private bool mEmptyGun;
        [HideInInspector]
        public Animator _WeaponAnimator;
        [HideInInspector]
        public Animator _ModelAnimator;
        [HideInInspector]
        public float _FireRate;
        [HideInInspector]
        public Transform _BulletSpawnPoint;
        [HideInInspector]
        public GameObject _SmokeParticle;
        [HideInInspector]
        public ParticleSystem[] _Muzzle;

        public GameObject _CastingPrefab;

        [HideInInspector]
        public Transform _CaseSpawn;

        public int _MagazineBullets = 0;
        public int _CarryingAmmo;
        public int _CurrentBullets = 30;

        public void Init()
        {
            mStateManager = GetComponent<StateManager>();
        }

        public void Tick()
        {
            if (!mStateManager._MeleeWeapon)
            {
                Shooting();
            }            
        }

        private void Shooting()
        {
            mShoot = mStateManager._Shoot;

            if (_ModelAnimator != null)
            {
                _ModelAnimator.SetBool("Shoot", false);
                _ModelAnimator.SetBool("Empty", (_CurrentBullets > 0) ? false : true);
            }

            if (mShoot)
            {
                if (mTimer <= 0)
                {
                    _WeaponAnimator.SetBool("Shoot", false);
                    if (_CurrentBullets > 0)
                    {
                        mEmptyGun = false;
                        mStateManager._AudioManager.PlayGunSound();

                        if (_ModelAnimator != null)
                        {
                            _ModelAnimator.SetBool("Shoot", true);
                        }
                        _WeaponAnimator.SetBool("Shoot", true);
                        GameObject InGO = Instantiate(_CastingPrefab, _CaseSpawn.position, _CaseSpawn.rotation) as GameObject;
                        Rigidbody InRigidbody = InGO.GetComponent<Rigidbody>();
                        InRigidbody.AddForce(transform.right.normalized * 2 + Vector3.up * 1.3f, ForceMode.Impulse);
                        InRigidbody.AddRelativeTorque(InGO.transform.right * 1.5f, ForceMode.Impulse);

                        mStateManager._ActualShooting = true;

                        for (int i = 0; i < _Muzzle.Length; i++)
                        {
                            _Muzzle[i].Emit(1);
                        }

                        RaycastShoot();
                        _CurrentBullets = _CurrentBullets - 1;
                    }
                    else
                    {
                        if (mEmptyGun)
                        {
                            mStateManager._AnimationHanlder.StartReload();
                            _CurrentBullets = _MagazineBullets;
                        }
                        else
                        {
                            mStateManager._AudioManager.PlayEffect("EmptyGun");
                            mEmptyGun = true;
                        }
                    }
                    mTimer = _FireRate;
                }
                else
                {
                    mStateManager._ActualShooting = false;
                    _WeaponAnimator.SetBool("Shoot", true);
                    mTimer -= mStateManager._CustomFixedDelta;
                }
            }
            else
            {
                mTimer -= (mTimer > 0) ? mStateManager._CustomFixedDelta : 0;
                if (_WeaponAnimator != null)
                {
                    _WeaponAnimator.SetBool("Shoot", false);
                }
                mStateManager._ActualShooting = false;
            }
        }

        private void RaycastShoot()
        {
            Vector3 InDirection = mStateManager._LookHitPosition - _BulletSpawnPoint.position;
            RaycastHit InHit;

            if (Physics.Raycast (_BulletSpawnPoint.position, InDirection, out InHit, 100, mStateManager._LayerMask))
            {
                GameObject InGO = Instantiate(_SmokeParticle, InHit.point, Quaternion.identity) as GameObject;
                InGO.transform.LookAt(_BulletSpawnPoint.position);
                if (InHit.transform.GetComponent<ShootingRangeTarget>())
                {
                    InHit.transform.GetComponent<ShootingRangeTarget>().HitTarget();
                }
            }
        }
    }
}