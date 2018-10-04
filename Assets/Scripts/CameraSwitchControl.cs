using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThridPersonShooter
{
    public class CameraSwitchControl : MonoBehaviour
    {
        public bool _Fps;
        private bool mSwithControl;

        public StateManager _FpsController;
        public StateManager _TpsController;
        public Transform _TpsCamera;
        public Transform _FpsCamera;

        public static CameraSwitchControl _Instace;

        private void Awake()
        {
            _Instace = this;
        }

        private void Start()
        {
            if (_Fps)
            {
                _TpsController.gameObject.SetActive(false);
                _TpsCamera.gameObject.SetActive(false);
                _FpsController.transform.gameObject.SetActive(true);
                _FpsCamera.gameObject.SetActive(true);
            }
            else
            {
                _TpsController.gameObject.SetActive(true);
                _TpsCamera.gameObject.SetActive(true);
                _FpsController.transform.gameObject.SetActive(false);
                _FpsCamera.gameObject.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if (_Fps)
            {
                _TpsController.transform.position = _FpsController.transform.position;
            }
            else
            {
                _FpsController.transform.position = _TpsController.transform.position;
            }
        }

        public void SwitchToFPS (Vector3 pLookPosition)
        {
            _FpsController.transform.position = _TpsController.transform.position;
            _FpsController.transform.rotation = _TpsController.transform.rotation;
            _FpsController._LookPosition = pLookPosition;

            _FpsController.gameObject.SetActive(true);
            _FpsCamera.transform.gameObject.SetActive(true);

            _TpsController.gameObject.SetActive(false);
            _TpsCamera.gameObject.SetActive(false);

            _Fps = true;
        }

        public void SwitchToTPS(Vector3 pLookPosition)
        {
            _TpsController.transform.position = _FpsController.transform.position;
            _TpsCamera.transform.parent.position = _TpsController.transform.position;

            _TpsController._LookPosition = pLookPosition;
            _TpsController.transform.rotation = _FpsController.transform.rotation;
            _TpsCamera.transform.rotation = _TpsController.transform.rotation;

            _TpsController.gameObject.SetActive(true);
            _TpsCamera.transform.gameObject.SetActive(true);

            _FpsController.gameObject.SetActive(false);
            _FpsCamera.gameObject.SetActive(false);

            _Fps = false;
        }
    }
}
