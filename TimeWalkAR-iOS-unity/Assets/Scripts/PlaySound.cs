using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : StateMachineBehaviour {


		override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			animator.gameObject.GetComponent<AudioSource>().Play();
		}

	}
