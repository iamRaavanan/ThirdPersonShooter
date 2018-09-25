using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputControl : MonoBehaviour
{
    [SerializeField]
    private float mPanSpeed;
    [SerializeField]
    private Transform mParentTransform;

    private bool mIsRotateEnabled;
    private Vector3 mTargetPosition;
    private float mAngle;

    private void Update()
    {
        if (Input.touchCount == 1 && Input.GetTouch (0).phase == TouchPhase.Moved)
        {
            Vector2 InTouchDeltaPosition = Input.GetTouch(0).deltaPosition;
            if (mIsRotateEnabled)
            {
                mParentTransform.Rotate(new Vector3(0, (InTouchDeltaPosition.x * 5 * Time.deltaTime), 0));
            }
            else
            {
                mTargetPosition = new Vector3(-InTouchDeltaPosition.x * mPanSpeed, InTouchDeltaPosition.y * mPanSpeed, 0);
                transform.Translate(mTargetPosition * Time.deltaTime);
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -10, 21.5f), transform.position.y, Mathf.Clamp(transform.position.z, -21, 21));
            }
        }
        if (Input.touchCount == 2)
        {
            Touch InFirstTouch = Input.GetTouch(0);
            Touch InSecondTouch = Input.GetTouch(1);

            Vector2 InFirstTouchPrevPosition = InFirstTouch.position - InFirstTouch.deltaPosition;
            Vector2 InSecondTouchPrevPosition = InSecondTouch.position - InSecondTouch.deltaPosition;

            float InPrevTouchDeltaMag = (InFirstTouchPrevPosition - InSecondTouchPrevPosition).magnitude;
            float InTouchDeltaMag = (InFirstTouch.position - InSecondTouch.position).magnitude;

            float InDiffDeltaMag = InPrevTouchDeltaMag - InTouchDeltaMag;

            Camera.main.fieldOfView += InDiffDeltaMag * 0.1f;

            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 15.0f, 60.0f);
        }
    }

    public void OnRotationValueChanged ()
    {
        mIsRotateEnabled = !mIsRotateEnabled;
    }
}
