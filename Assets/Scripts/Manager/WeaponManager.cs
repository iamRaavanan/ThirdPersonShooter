using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class WeaponManager : MonoBehaviour
    {
        public int _WeaponIndex;
        public List<WeaponReferenceBase> _WeaponList = new List<WeaponReferenceBase>();

        private WeaponReferenceBase _CurrentWeapon;
        private IKHandler mIkHandler;
        private ShootingHandler mShootingHandler;
        private AudioManager mAudioManager;

        private void Start()
        {
            mIkHandler = GetComponent<IKHandler>();
            mShootingHandler = GetComponent<ShootingHandler>();
            mAudioManager = GetComponent<AudioManager>();
            CloseAllWeapons();
            SwitchWeapon(_WeaponIndex);
        }

        private void Update()
        {
            if (Input.GetKeyUp (KeyCode.Q))
            {
                SwitchWeapon(_WeaponIndex);
                _WeaponIndex = (_WeaponIndex < _WeaponList.Count - 1) ? _WeaponIndex++ : 0;
            }
        }

        public void SwitchWeapon (int pChoosenIndex)
        {
            if (_CurrentWeapon != null)
            {
                _CurrentWeapon._WeaponModel.SetActive(false);
                _CurrentWeapon._IkHolder.SetActive(false);
            }

            WeaponReferenceBase InNewWeapon = _WeaponList[pChoosenIndex];

            mIkHandler._RightHandIKTarget = InNewWeapon._RightHandTarget;
            mIkHandler._LeftHandIKTarget = InNewWeapon._LeftHandTarget;

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

            _WeaponIndex = pChoosenIndex;
            InNewWeapon._WeaponModel.SetActive(true);
            InNewWeapon._IkHolder.SetActive(true);
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
    }

    [System.Serializable]
    public class WeaponReferenceBase
    {
        public string _WeaponID;
        public GameObject _WeaponModel;
        public Animator _ModelAnimator;
        public GameObject _IkHolder;
        public Transform _RightHandTarget;
        public Transform _LeftHandTarget;
        public Transform _LookTarget;
        public ParticleSystem[] _Muzzle;
        public Transform _BulletSpawner;
        public Transform _CasingSpawner;
        public WeaponStats _WeaponStats;
        public Transform _RightElbowTarget;
        public Transform _LeftElbowTarget;
        public bool _Dis_LHIK_NotAiming;
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
