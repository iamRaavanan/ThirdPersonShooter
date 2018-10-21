using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class WeaponManager : MonoBehaviour
    {
        public int _MaxWeapons = 2;
        public List<WeaponReferenceBase> _AvailableWeapons = new List<WeaponReferenceBase>();

        public int _WeaponIndex;
        public List<WeaponReferenceBase> _WeaponList = new List<WeaponReferenceBase>();

        private WeaponReferenceBase _CurrentWeapon;
        private IKHandler mIkHandler;
        private ShootingHandler mShootingHandler;
        private AudioManager mAudioManager;
        [HideInInspector]
        public StateManager _StateManager;
        public WeaponReferenceBase _Unarmed;

        public void Init()
        {
            _StateManager = GetComponent<StateManager>();
            mIkHandler = GetComponent<IKHandler>();
            mShootingHandler = GetComponent<ShootingHandler>();
            mAudioManager = GetComponent<AudioManager>();

            _AvailableWeapons.Add(_WeaponList[0]);
            _WeaponIndex = 0;

            CloseAllWeapons();

            //_Unarmed._AnimType = 10;
            //_Unarmed._MeleeWeapon = true;

            SwitchWeapon(_WeaponIndex);
        }

        public void Tick()
        {
            if (Input.GetKeyUp (KeyCode.Q))
            {
                SwitchWeapon(_WeaponIndex);
                _WeaponIndex = (_WeaponIndex < _WeaponList.Count - 1) ? _WeaponIndex++ : 0;
            }
            if (Input.GetKeyDown (KeyCode.H))
            {
                if (!_StateManager._MeleeWeapon)
                {
                    SwitchWeaponWithTarget(_Unarmed);
                    _StateManager._MeleeWeapon = true;
                }
                else
                {
                    SwitchWeaponWithTarget(_AvailableWeapons[_WeaponIndex]);
                    _StateManager._MeleeWeapon = false;
                }
            }
        }

        public void SwitchWeapon (int pChoosenIndex)
        {
            if (pChoosenIndex > _AvailableWeapons.Count - 1)
            {
                pChoosenIndex = _WeaponIndex = 0;
                
            }
            //WeaponReferenceBase InTargetWeapon = 
            SwitchWeaponWithTarget(_WeaponList[pChoosenIndex]);
            _WeaponIndex = pChoosenIndex;
        }

        private void SwitchWeaponWithTarget(WeaponReferenceBase weaponReferenceBase)
        {
            if (_CurrentWeapon != null)
            {
                if (_CurrentWeapon._WeaponModel != null)
                {
                    _CurrentWeapon._WeaponModel.SetActive(false);
                    _CurrentWeapon._IkHolder.SetActive(false);
                }
                if (_CurrentWeapon._HolsterWeapon)
                {
                    _CurrentWeapon._HolsterWeapon.SetActive(true);
                }
            }

            WeaponReferenceBase InNewWeapon = weaponReferenceBase;

            mIkHandler._RightHandIKTarget = (InNewWeapon._RightHandTarget) ? InNewWeapon._RightHandTarget : null;
            mIkHandler._RightHandIKRotation = (InNewWeapon._RightHandRotation) ? InNewWeapon._RightHandRotation : null;
            mIkHandler._LeftHandIKTarget = (InNewWeapon._LeftHandTarget) ? InNewWeapon._LeftHandTarget : null;

            mIkHandler._OverrideLookTarget = (InNewWeapon._LookTarget) ? InNewWeapon._LookTarget : null;
            mShootingHandler._ModelAnimator = (InNewWeapon._ModelAnimator) ? InNewWeapon._ModelAnimator : null;

            mIkHandler._LeftElbowTarget = (InNewWeapon._LeftElbowTarget) ? InNewWeapon._LeftElbowTarget : null;
            mIkHandler._RightElbowTarget = (InNewWeapon._RightElbowTarget) ? InNewWeapon._RightElbowTarget : null;

            mIkHandler._LHIK_dis_NotAiming = InNewWeapon._Dis_LHIK_NotAiming;

            if (mIkHandler._LHIK_dis_NotAiming)
            {
                mIkHandler._LeftHandIKWeight = 0;
            }
            mShootingHandler._FireRate = InNewWeapon._WeaponStats._FireRate;
            mShootingHandler._WeaponAnimator = InNewWeapon._IkHolder.GetComponent<Animator>();
            mShootingHandler._BulletSpawnPoint = InNewWeapon._BulletSpawner;
            mShootingHandler._CurrentBullets = InNewWeapon._WeaponStats._CurrentBullets;
            mShootingHandler._MagazineBullets = InNewWeapon._WeaponStats._MaxBullets;
            mShootingHandler._CaseSpawn = InNewWeapon._CasingSpawner;
            mShootingHandler._Muzzle = InNewWeapon._Muzzle;

            mAudioManager._GunSounds.clip = InNewWeapon._WeaponStats._ShootSound;
            mShootingHandler._CarryingAmmo = InNewWeapon._CarryingAmmo;
            //_WeaponIndex = pChoosenIndex;
            if (InNewWeapon._WeaponModel)
                InNewWeapon._WeaponModel.SetActive(true);
            if (InNewWeapon._IkHolder)
                InNewWeapon._IkHolder.SetActive(true);

            _StateManager._WeaponAnimType = InNewWeapon._AnimType;
            _StateManager._MeleeWeapon = InNewWeapon._MeleeWeapon;

            _CurrentWeapon = InNewWeapon;
        }

        public void CloseAllWeapons()
        {
            for (int i = 0; i < _WeaponList.Count; i++)
            {
                ParticleSystem[] InMuzzleParticles = _WeaponList[i]._WeaponModel.GetComponentsInChildren<ParticleSystem>();
                _WeaponList[i]._Muzzle = InMuzzleParticles;

                _WeaponList[i]._WeaponModel.SetActive(false);
                _WeaponList[i]._IkHolder.SetActive(false);
            }
        }

        public List<SharableObject> GetSharableObjects ()
        {
            List<SharableObject> InSharableObjects = new List<SharableObject>();

            SharableObject[] InObjects = GetComponentsInChildren<SharableObject>();
            foreach (SharableObject obj in InObjects)
            {
                InSharableObjects.Add(obj);
            }
            foreach (WeaponReferenceBase weaponbase in _WeaponList)
            {
                if (weaponbase._HolsterWeapon)
                {
                    bool InWasActive = weaponbase._HolsterWeapon.activeInHierarchy;
                    weaponbase._HolsterWeapon.SetActive(true);
                    if (weaponbase._HolsterWeapon.GetComponent<SharableObject>())
                    {
                        SharableObject InObj = weaponbase._HolsterWeapon.GetComponent<SharableObject>();
                        if (!InSharableObjects.Contains(InObj))
                        {
                            InSharableObjects.Add(InObj);
                        }
                    }
                    if (!InWasActive)
                    {
                        weaponbase._HolsterWeapon.SetActive(false);
                    }
                }
                if (weaponbase._WeaponModel)
                {
                    bool InWasActive = weaponbase._WeaponModel.activeInHierarchy;
                    weaponbase._WeaponModel.SetActive(true);
                    if (weaponbase._WeaponModel.GetComponent<SharableObject>())
                    {
                        SharableObject InObject = weaponbase._WeaponModel.GetComponent<SharableObject>();
                        if (!InSharableObjects.Contains (InObject))
                        {
                            InSharableObjects.Add(InObject);
                        }
                    }
                    if (!InWasActive)
                    {
                        weaponbase._WeaponModel.SetActive(false);
                    }
                }
            }
            return InSharableObjects;
        }
    }

    [System.Serializable]
    public class WeaponReferenceBase
    {
        public string _WeaponID;
        public GameObject _WeaponModel;
        public Animator _ModelAnimator;
        public GameObject _IkHolder;
        public Transform _RightHandTarget;
        public Transform _RightHandRotation;
        public Transform _LeftHandTarget;
        public Transform _LookTarget;
        public ParticleSystem[] _Muzzle;
        public Transform _BulletSpawner;
        public Transform _CasingSpawner;
        public WeaponStats _WeaponStats;
        public Transform _RightElbowTarget;
        public Transform _LeftElbowTarget;
        public int _AnimType;

        public bool _Dis_LHIK_NotAiming;

        public int _CarryingAmmo = 60;
        public int _MaxAmmo = 60;
        public GameObject _PickablePrefab;
        public GameObject _HolsterWeapon;
        public bool _MeleeWeapon = false;
    }

    [System.Serializable]
    public class WeaponStats
    {
        public int _CurrentBullets;
        public int _MaxBullets;
        public float _FireRate;
        public AudioClip _ShootSound;
    }
}
