using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource _GunSounds;
        public AudioSource _RunFoley;

        public float _FootStepTimer;
        public float _WalkThreshold;
        public float _RunThreshold;
        public AudioSource _FootStep1;
        public AudioSource _FootStep2;
        public AudioClip[] _FootStepClips;
        public AudioSource _EffectSource;
        public AudioClipList[] _EffectsList;

        private StateManager mStateManager;
        private float mStartingVolumeRun;
        private float mCharacterMovement;

        public void Init()
        {
            mStateManager = GetComponent<StateManager>();
            mStartingVolumeRun = _RunFoley.volume;
            _RunFoley.volume = 0;
        }

        public void Tick()
        {
            mCharacterMovement = Mathf.Abs(mStateManager._Horizontal) + Mathf.Abs(mStateManager._Vertical);
            mCharacterMovement = Mathf.Clamp01(mCharacterMovement);

            float InTargetThreshold = 0;
            if (!mStateManager._Walk && !mStateManager._Aiming && !mStateManager._Reloading)
            {
                _RunFoley.volume = mStartingVolumeRun * mCharacterMovement;
                InTargetThreshold = _RunThreshold;
            }
            else
            {
                InTargetThreshold = _WalkThreshold;
                _RunFoley.volume = Mathf.Lerp(_RunFoley.volume, 0, Time.deltaTime * 2);
            }
            if (mCharacterMovement > 0)
            {
                _FootStepTimer += Time.deltaTime;
                if (_FootStepTimer > InTargetThreshold)
                {

                    _FootStepTimer = 0;
                }
            }
            else
            {
                _FootStep1.Stop();
                _FootStep2.Stop();
            }
        }

        public void PlayGunSound ()
        {
            _GunSounds.Play();
        }

        public void PlayFootStep ()
        {
            int InRandom = 0;
            if (!_FootStep1.isPlaying)
            {
                InRandom = Random.Range(0, _FootStepClips.Length);
                _FootStep1.clip = _FootStepClips[InRandom];
                _FootStep1.Play();
            }
            else
            {
                if (!_FootStep2.isPlaying)
                {
                    InRandom = Random.Range(0, _FootStepClips.Length);
                    _FootStep2.clip = _FootStepClips[InRandom];
                    _FootStep2.Play();
                }
            }
        }

        public void PlayEffect (string pName)
        {
            AudioClip InClip = null;
            for (int i = 0; i < _EffectsList.Length; i++)
            {
                if (string.Equals (_EffectsList[i]._Name, pName))
                {
                    InClip = _EffectsList[i]._Clip;
                    break;
                }
            }
            _EffectSource.clip = InClip;
            _EffectSource.Play();
        }
    }

    [System.Serializable]
    public class AudioClipList
    {
        public string _Name;
        public AudioClip _Clip;
    }
}
