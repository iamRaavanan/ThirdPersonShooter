using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class ResourceManager : MonoBehaviour
    {
        public List<CharacterModel> _CharacterModel = new List<CharacterModel>();

        public void SwitchCharacterOnIndex (StateManager pStateManager, int pTarget)
        {
            StartCoroutine(SwitchCharacter(pStateManager, pTarget));
        }

        private IEnumerator SwitchCharacter (StateManager pStateManager, int pTarget)
        {
            yield return SwitchCharacter(pStateManager, 0);
            yield return SwitchCharacter(pStateManager, pTarget);
        }

        private IEnumerator SwitchCharacterWith (StateManager pStateManager,  int pTarget)
        {
            if (!pStateManager._Model.activeInHierarchy)
            {
                pStateManager._Model.SetActive(true);
            }
            List<SharableObject> InAvailableObjects = pStateManager._WeaponManager.GetSharableObjects();
            List<SharableAssetInfo> InAvailableAssetInfo = new List<SharableAssetInfo>();

            foreach (SharableObject obj in InAvailableObjects)
            {
                SharableAssetInfo InAssetInfo = new SharableAssetInfo();
                InAssetInfo._Object = obj.gameObject;
                InAssetInfo._Posistion = obj.transform.localPosition;
                InAssetInfo._Rotation = obj.transform.localRotation;
                InAssetInfo._Scale = obj.transform.localScale;
                InAssetInfo._ParentBone = obj._ParentBone;
                InAssetInfo._IsActive = InAssetInfo._Object.activeInHierarchy;
                InAssetInfo._Object.SetActive(false);
                InAssetInfo._Object.transform.parent = null;
                InAvailableAssetInfo.Add(InAssetInfo);
            }
            GameObject InNewModel = Instantiate(_CharacterModel[pTarget]._Prefab, Vector3.zero, Quaternion.identity) as GameObject;
            InNewModel.transform.parent = pStateManager.transform;
            InNewModel.transform.localPosition = Vector3.zero;
            InNewModel.transform.localRotation = Quaternion.Euler(Vector3.zero);

            if (pTarget == 0)
            {
                InNewModel.SetActive(false);
            }

            GameObject InPrevModel = pStateManager._Model;
            pStateManager._Model = InNewModel;
            pStateManager._AnimationHanlder.SetupAnimator(InNewModel.GetComponent<Animator>());

            Destroy(InPrevModel);

            pStateManager._AnimationHanlder._Animator.Rebind();

            for (int i = 0;  i < InAvailableAssetInfo.Count; i++)
            {
                Transform InTransform = InAvailableAssetInfo[i]._Object.transform;
                InTransform.parent = pStateManager._AnimationHanlder._Animator.GetBoneTransform(InAvailableAssetInfo[i]._ParentBone);
                InTransform.localPosition = InAvailableAssetInfo[i]._Posistion;
                InTransform.localRotation = InAvailableAssetInfo[i]._Rotation;
                InTransform.localScale = InAvailableAssetInfo[i]._Scale;
                if (InAvailableAssetInfo[i]._IsActive)
                {
                    InAvailableAssetInfo[i]._Object.SetActive(true);
                }                
            }
            yield return null;
        }

        private int GetCharacterModelIndex (string pId)
        {
            int InValue = 0;

            for (int i = 0; i < _CharacterModel.Count; i++)
            {
                if (string.Equals (_CharacterModel[i]._Id, pId))
                {
                    InValue = i;
                    break;
                }
            }
            return InValue;
        }

        public static ResourceManager _Instance;
        public static ResourceManager GetInstance ()
        {
            return _Instance;
        }

        private void Awake()
        {
            _Instance = this;
        }
    }

    [System.Serializable]
    public class CharacterModel
    {
        public string _Id;
        public GameObject _Prefab;
    }

    [System.Serializable]
    public class SharableAssetInfo
    {
        public GameObject _Object;
        public Vector3 _Posistion;
        public Quaternion _Rotation;
        public Vector3 _Scale;
        public bool _IsActive;
        public HumanBodyBones _ParentBone;
    }
}