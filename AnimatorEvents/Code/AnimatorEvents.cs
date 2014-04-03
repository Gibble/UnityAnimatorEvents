using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[RequireComponent (typeof (Animator))]
public class AnimatorEvents : MonoBehaviour {
	
	[HideInInspector]
	public Animator animator;
	
	public AnimatorEventLayer[] layers;
	
	#region Events and Delegates
	public delegate void StateChangeHandler (int layer, AnimatorStateInfo previous, AnimatorStateInfo current);
	public event StateChangeHandler OnStateChangeStep;
	public event StateChangeHandler OnStateChanged;
	
	public delegate void TransitionHandler (int layer, AnimatorTransitionInfo transitionInfo);
	public event TransitionHandler OnTransition;
	#endregion

	private bool m_bRunOnce = false;
	
	void Start () {
		foreach (AnimatorEventLayer animatorLayer in layers)
			animatorLayer.MakeDictionaries();
	}

	void LateUpdate () {

		if (!m_bRunOnce)
		{
			m_bRunOnce = true;
			return;
		}

		for ( int layer = 0; layer < layers.Length; layer++) {
			if (layers[layer].isListening) {
				// State Change Verification
				layers[layer].currentState = animator.GetCurrentAnimatorStateInfo(layer);

				// Last State Change
				if (layers[layer].previousState.nameHash == layers[layer].currentState.nameHash &&
 					layers[layer]._stateIsInFlux == true &&
					!animator.IsInTransition(layer))
				{
					if (OnStateChanged != null)
						OnStateChanged(layer, layers[layer].previousState, layers[layer].currentState);
					layers[layer]._stateIsInFlux = false;
				}

				// Intermediate State Changes
				if (layers[layer].previousState.nameHash != layers[layer].currentState.nameHash) {
					if (OnStateChangeStep != null)
						OnStateChangeStep (layer, layers[layer].previousState, layers[layer].currentState);
					layers[layer].previousState = layers[layer].currentState;
					layers[layer]._stateIsInFlux = true;
				}
				
				// Transition Change Verification
				if (animator.IsInTransition(layer)) {
					if (OnTransition != null)
						OnTransition(layer, animator.GetAnimatorTransitionInfo(layer));
				}
			}
		}
	}
	
#if UNITY_EDITOR
	public bool CheckRedudancy() {
		AnimatorEvents exisitingAnimatorEvents = GetComponent<AnimatorEvents>();
		
		if (exisitingAnimatorEvents != this && exisitingAnimatorEvents != null) {
			Debug.LogError("There can be only one AnimatorEvents per Animator");
			DestroyImmediate(this);
			return true;
		}
		return false;
	}
#endif
}
