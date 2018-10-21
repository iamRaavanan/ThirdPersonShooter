using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class CoverPosition : MonoBehaviour
    {
        public BezierCurve _CurvePath;
        public bool _IsBlockStartPosition;
        public bool _IsBlockEndPosition;

        #region Obsolete
        //public float _Percentage;
        //public Transform _StartTransform;
        //public bool _BlockStartPosition;
        //public Transform _EndTransform;
        //public bool _BlockEndPosition;
        #endregion

        public float _Length;
        public CoverType _CoverType;

        public enum CoverType
        {
            E_Full,
            E_Half
        }

        private void Start()
        {
            //_Length = Vector3.Distance(_StartTransform.position, _EndTransform.position);
            _CurvePath = GetComponentInChildren<BezierCurve>();
            _Length = _CurvePath.length;
        }
    }
}
