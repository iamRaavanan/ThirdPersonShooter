using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class ShootingHandler : MonoBehaviour
    {
        private StateManager mStateManager;        
        private float mTimer;
        private bool mShoot;
        private bool mDontShoot;
        private bool mEmptyGun;

        public Animator _WeaponAnimator;
        public float _FireRate;
        public Transform _BulletSpawnPoint;
        public GameObject _SmokeParticle;
        public ParticleSystem[] _Muzzle;
        public GameObject _CastingPrefab;
        public Transform _CaseSpawn;
        public int _CurrentBullets = 30;

        private void Start()
        {
            mStateManager = GetComponent<StateManager>();
        }

        private void Update()
        {
            mShoot = mStateManager._Shoot;
            if (mShoot)
            {
                if (mTimer <= 0)
                {
                    _WeaponAnimator.SetBool("Shoot", false);
                    if (_CurrentBullets > 0)
                    {
                        mEmptyGun = false;
                        mStateManager._AudioManager.PlayGunSound();

                        GameObject InGO = Instantiate(_CastingPrefab, _CaseSpawn.position, _CaseSpawn.rotation) as GameObject;
                        Rigidbody InRigidbody = InGO.GetComponent<Rigidbody>();
                        InRigidbody.AddForce(transform.right.normalized * 2 + Vector3.up * 1.3f, ForceMode.Impulse);
                        InRigidbody.AddRelativeTorque(InGO.transform.right * 1.5f, ForceMode.Impulse);

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
                            _CurrentBullets = 30;
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
                    _WeaponAnimator.SetBool("Shoot", true);
                    mTimer -= Time.deltaTime;
                }
            }
            else
            {
                mTimer = -1;
                _WeaponAnimator.SetBool("Shoot", false);
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