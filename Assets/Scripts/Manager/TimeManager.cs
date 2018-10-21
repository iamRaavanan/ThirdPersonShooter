using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class TimeManager : MonoBehaviour
    {
        private float mCustomDelta;
        private float mCustomFixedDelta;
        public float _CustomTimescale = 1;

        public static TimeManager _Instance;
        public static TimeManager GetInstance ()
        {
            return _Instance;
        }

        private void Awake()
        {
            _Instance = this;
        }

        private void Start()
        {
            _CustomTimescale = 1;
        }

        private void FixedUpdate()
        {
            mCustomFixedDelta = Time.fixedDeltaTime * _CustomTimescale;
        }

        private void Update()
        {
            mCustomDelta = Time.deltaTime * _CustomTimescale;
        }

        public float GetCustomDelta ()
        {
            return mCustomDelta;
        }

        public float GetCustomFixedDelta ()
        {
            return mCustomFixedDelta;
        }
    }
}
