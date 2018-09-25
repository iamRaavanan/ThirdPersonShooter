using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class StateManager : MonoBehaviour
    {

        public bool _Aiming;
        public bool _CanRun;
        public bool _Walk;
        public bool _Shoot;
        public bool _Reloading;
        public bool _OnGround;

        public float _Horizontal;
        public float _Vertical;
        public Vector3 _LookPosition;
        public Vector3 _LookHitPosition;
        public LayerMask _LayerMask;

        public AudioManager _AudioManager;
        public ShootingHandler _ShootingHandler;
        public AnimationHandler _AnimationHanlder;

        private void Start()
        {
            _AudioManager = GetComponent<AudioManager>();
            _ShootingHandler = GetComponent<ShootingHandler>();
            _AnimationHanlder = GetComponent<AnimationHandler>();
        }

        private void FixedUpdate()
        {
            _OnGround = IsOnGround();
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
