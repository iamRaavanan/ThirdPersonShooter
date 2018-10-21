using System.Collections;
using UnityEngine;

namespace Raavanan
{
    public class CloseReload_AnimBehavior : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("Reload", true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("Reloading", false);
        }
    }
}
