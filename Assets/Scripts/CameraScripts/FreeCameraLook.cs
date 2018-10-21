using System;
using UnityEngine;

namespace Raavanan
{
    public class FreeCameraLook : Pivot
    {

        [SerializeField]
        private float                   moveSpeed = 5f;
        [SerializeField]
        private float                   turnSpeed = 1.5f;
        [SerializeField]
        private float                   turnsmoothing = .1f;
        [SerializeField]
        private float                   tiltMax = 75f;
        [SerializeField]
        private float                   tiltMin = 45f;
        [SerializeField]
        private bool                    lockCursor = false;

        private float                   lookAngle;
        private float                   tiltAngle;

        private const float             LookDistance = 100f;

        private float                   smoothX = 0;
        private float                   smoothY = 0;
        private float                   smoothXvelocity = 0;
        private float                   smoothYvelocity = 0;

        public float                    _CrosshairOffsetWiggle = 0.2f;
        public bool                     _OverrideTarget;
        public Vector3                  _NewTargetPosition;

        public float                    _CoverAngleMax;
        public float                    _CoverAngleMin;
        public bool                     _UnderCover;
        public int                      _CoverDirection;

        private CrosshairManager mCrosshairManager;

        //add the singleton
        public static FreeCameraLook instance;

        public static FreeCameraLook GetInstance()
        {
            return instance;
        }

        protected override void Awake()
        {
            instance = this;

            base.Awake();

            cam = GetComponentInChildren<Camera>().transform;
            pivot = cam.parent.parent; //take the correct pivot
        }

        protected override void Start()
        {
            base.Start();

            if (lockCursor)
                Cursor.lockState = CursorLockMode.Locked;

            mCrosshairManager = CrosshairManager.GetInstance();
            _NewTargetPosition = target.position;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            HandleRotationMovement();

        }

        protected override void Follow(float deltaTime)
        {
            Vector3 InInitTargetPos = target.position;
            if (_OverrideTarget)
            {
                InInitTargetPos = _NewTargetPosition;
            }
            else
            {
                _NewTargetPosition = InInitTargetPos;
            }
            Vector3 InTargetPosition = Vector3.Lerp(transform.position, InInitTargetPos, deltaTime * moveSpeed);
            transform.position = InTargetPosition;
        }

        void HandleRotationMovement()
        {
            HandleOffsets();

            float x = Input.GetAxis("Mouse X") + offsetX;
            float y = Input.GetAxis("Mouse Y") + offsetY;

            if (turnsmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnsmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnsmoothing);
            }
            else
            {
                smoothX = x;
                smoothY = y;
            }

            if (!_UnderCover)
            {
                lookAngle += smoothX * turnSpeed;
            }
            else
            {
                float InAngleFromWorldFwd = Vector3.Angle(target.forward, transform.forward);
                int InDot = DotOrientation(InAngleFromWorldFwd);

                float InMaxAngle = (InAngleFromWorldFwd * InDot) + _CoverAngleMax;
                float InMinAngle = (InAngleFromWorldFwd * InDot) + _CoverAngleMin;

                lookAngle += smoothX * turnSpeed;
                lookAngle = Mathf.Clamp(lookAngle, InMinAngle, InMaxAngle);
            }
            if (lookAngle > 360)
            {
                lookAngle = 0;
            }
            if (lookAngle < -360)
            {
                lookAngle = 0;
            }
            transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

            tiltAngle -= smoothY * turnSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, -tiltMin, tiltMax);

            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

            if (x > _CrosshairOffsetWiggle || x < -_CrosshairOffsetWiggle || y > _CrosshairOffsetWiggle || y < -_CrosshairOffsetWiggle)
            {
                WiggleCrosshairAndCamera(0);
            }
        }

        private int DotOrientation(float pAngleFromWorldFwd)
        {
            // Find World Orientation
            float InNSDot = Vector3.Dot(target.forward, Vector3.forward);
            float InWEDot = Vector3.Dot(target.forward, Vector3.right);
            int InReturnVal = 0;
            // North Check
            if (InNSDot > 0)
            {
                // Not North
                if (pAngleFromWorldFwd > 45)
                {
                    InReturnVal = WestOrEast(InWEDot);
                }
                else // North
                {
                    InReturnVal = -_CoverDirection;
                }
            }
            else // South Check
            {
                // Not South
                if (pAngleFromWorldFwd > 45)
                {
                    InReturnVal = WestOrEast(InWEDot);
                }
                else // South
                {
                    InReturnVal = -_CoverDirection;
                }
            }
            return InReturnVal;
        }

        private int WestOrEast(float pWEDot)
        {
            int InReturnVal = 0;
            if (pWEDot < 0)
            {
                // Depending on the cover position of the player
                // we need to switch the multiplier -ve or +ve

                InReturnVal = (_CoverDirection > 0) ? -_CoverDirection : _CoverDirection; ;
            }
            else
            {
                InReturnVal = (_CoverDirection < 0) ? -_CoverDirection : _CoverDirection; ;
            }
            return InReturnVal;
        }

        float offsetX;
        float offsetY;

        void HandleOffsets()
        {
            if (offsetX != 0)
            {
                offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
            }

            if (offsetY != 0)
            {
                offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
            }
        }

        public void WiggleCrosshairAndCamera(float kickback)
        {
            mCrosshairManager.activeCrosshair.WiggleCrosshair();

            offsetY = kickback;
        }


    }
}