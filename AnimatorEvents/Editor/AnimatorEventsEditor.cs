using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[CustomEditor (typeof(AnimatorEvents))]
public class AnimatorEventsEditor : Editor {
	AnimatorEvents animatorEvents;

	void OnEnable() {
		animatorEvents = target as AnimatorEvents;
		animatorEvents.animator = animatorEvents.gameObject.GetComponent<Animator>();
		if (animatorEvents.CheckRedudancy())
			return;
	}
	
	public override void OnInspectorGUI () {
		if (!Application.isPlaying) {
			if (GUILayout.Button("Update From Animator"))
				animatorEvents.layers = GetAnimatorLayers();
		}
		
		if (animatorEvents.animator == null)
			return;
		
		if (animatorEvents.layers == null)
			return;
		
		string[] layerNames = GetLayerNames(animatorEvents.animator);
	
		for (int i = 0; i < animatorEvents.layers.Length; i++) {
			
			// Draw Layer Foldout
			animatorEvents.layers[i].foldLayer = EditorGUILayout.Foldout(animatorEvents.layers[i].foldLayer, layerNames[i]);
			if (animatorEvents.layers[i].foldLayer) {
				animatorEvents.layers[i].isListening = EditorGUILayout.Toggle("Listen to Events", animatorEvents.layers[i].isListening);
				
				// Draw States Foldout
				animatorEvents.layers[i].foldStates = EditorGUILayout.Foldout(animatorEvents.layers[i].foldStates, "States(" + animatorEvents.layers[i]._stateKeys.Length.ToString() + ")");
				if (animatorEvents.layers[i].foldStates) {
					EditorGUILayout.LabelField("\t" + "Hash Name", "Unique Name");
					for (var j = 0; j < animatorEvents.layers[i]._stateKeys.Length; j++) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("\t" + animatorEvents.layers[i]._stateKeys[j].ToString(), animatorEvents.layers[i]._stateNames[j]);
						EditorGUILayout.EndHorizontal();
					}
				}
				
				//Draw Transition Foldout
				animatorEvents.layers[i].foldTransitions = EditorGUILayout.Foldout(animatorEvents.layers[i].foldTransitions, "Transitions(" + animatorEvents.layers[i]._transitionKeys.Length.ToString() + ")");
				if (animatorEvents.layers[i].foldTransitions) {
					EditorGUILayout.LabelField("\t" + "Hash Name", "Unique Name");
					for (var k = 0; k < animatorEvents.layers[i]._transitionKeys.Length; k++) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("\t" + animatorEvents.layers[i]._transitionKeys[k].ToString(), animatorEvents.layers[i]._transitionNames[k]);
						EditorGUILayout.EndHorizontal();
					}	
				}
			}	
			
		}
	}
	
	public AnimatorEventLayer[] GetAnimatorLayers() {
		List<AnimatorEventLayer> animatorLayers = new List<AnimatorEventLayer>();
		for (int i = 0; i < GetLayerCount(animatorEvents.animator); i++) {
			animatorLayers.Add (new AnimatorEventLayer (
														GetStateKeys(animatorEvents.animator, i),
														GetStateNames(animatorEvents.animator, i),
														GetTransitionKeys(animatorEvents.animator, i),
														GetTransitionNames(animatorEvents.animator, i)));
		}
		return animatorLayers.ToArray();
	}

	#region Animator Layer Methods
	
	/// <summary>
	/// Number of layers.
	/// </summary>
	/// <returns>
	/// The layer count.
	/// </returns>
	/// <param name='animator'>
	/// Animator.
	/// </param>
	private static int GetLayerCount (Animator animator) {
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		return animatorController.layerCount;
	}
	
	/// <summary>
	/// Get all the layer names.
	/// </summary>
	/// <returns>
	/// The layer names.
	/// </returns>
	/// <param name='animator'>
	/// Animator.
	/// </param>
	private static string[] GetLayerNames (Animator animator) {
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		
		List<string> layerNames = new List<string>();
		
		for (int i = 0; i < animatorController.layerCount; i++)
			layerNames.Add(animatorController.GetLayer(i).name);
		
		return layerNames.ToArray();
	}
	
	#endregion
	
	#region Animator State Methods	
	private static int[] GetStateKeys (Animator animator, int layer) {
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		StateMachine stateMachine = animatorController.GetLayer(layer).stateMachine;				

		return GetStateKeysFromStateMachine(stateMachine);
	}

	private static int[] GetStateKeysFromStateMachine(StateMachine sm)
	{
		List<int> stateKeys = new List<int>();
		
		for (int i = 0; i < sm.stateCount; ++i)
		{
			stateKeys.Add(sm.GetState(i).uniqueNameHash);
		}

		for (int i = 0; i < sm.stateMachineCount; ++i)
		{
			stateKeys.AddRange(GetStateKeysFromStateMachine(sm.GetStateMachine(i)));
		}

		return stateKeys.ToArray();
	}
	
	private static string[] GetStateNames (Animator animator, int layer) {		
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		StateMachine stateMachine = animatorController.GetLayer(layer).stateMachine;

		return GetStateNamesFromStateMachine(stateMachine);
	}

	private static string[] GetStateNamesFromStateMachine(StateMachine sm)
	{
		List<string> stateNames = new List<string>();

		for (int i = 0; i < sm.stateCount; ++i)
		{
			stateNames.Add(sm.GetState(i).uniqueName);
		}

		for (int i = 0; i < sm.stateMachineCount; ++i)
		{
			stateNames.AddRange(GetStateNamesFromStateMachine(sm.GetStateMachine(i)));
		}

		return stateNames.ToArray();
	}
	#endregion
	
	#region Animator Transition Methods
	
	private static int[] GetTransitionKeys (Animator animator, int layer) {
		List<int> transitionKeys = new List<int>();
		
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		StateMachine stateMachine = animatorController.GetLayer(layer).stateMachine;
			
		return GetTransitionKeysFromStateMachine(stateMachine);
	}

	private static int[] GetTransitionKeysFromStateMachine(StateMachine sm)
	{
		List<int> transitionKeys = new List<int>();
		List<Transition> transitions = new List<Transition>();

		for (int i = 0; i < sm.stateCount; ++i)
		{
			transitions.AddRange(sm.GetTransitionsFromState(sm.GetState(i)));
		}

		foreach (Transition transition in transitions)
			transitionKeys.Add(transition.uniqueNameHash);

		for (int i = 0; i < sm.stateMachineCount; ++i)
		{
			transitionKeys.AddRange(GetTransitionKeysFromStateMachine(sm.GetStateMachine(i)));
		}

		return transitionKeys.ToArray();
	}
	
	private static string[] GetTransitionNames (Animator animator, int layer) {
		List<string> transitionNames = new List<string>();
		
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		StateMachine stateMachine = animatorController.GetLayer(layer).stateMachine;
		
		return GetTransitionNamesFromStateMachine(stateMachine);
	}

	private static string[] GetTransitionNamesFromStateMachine(StateMachine sm)
	{
		List<string> transitionNames = new List<string>();
		List<Transition> transitions = new List<Transition>();

		for (int i = 0; i < sm.stateCount; ++i)
		{
			transitions.AddRange(sm.GetTransitionsFromState(sm.GetState(i)));
		}

		foreach (Transition transition in transitions)
			transitionNames.Add(transition.uniqueName);

		for (int i = 0; i < sm.stateMachineCount; ++i)
		{
			transitionNames.AddRange(GetTransitionNamesFromStateMachine(sm.GetStateMachine(i)));
		}

		return transitionNames.ToArray();
	}
	
	/// <summary>
	/// Gets the count of transitions in a layer.
	/// </summary>
	/// <returns>
	/// The transition count.
	/// </returns>
	/// <param name='animator'>
	/// Animator.
	/// </param>
	/// <param name='layer'>
	/// Layer.
	/// </param>
	public static int GetTransitionsCount (Animator animator, int layer) {
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		StateMachine stateMachine = animatorController.GetLayer(layer).stateMachine;

		List<Transition> transitions = new List<Transition>();

		for (int i = 0; i < stateMachine.stateCount; ++i)
		{
			transitions.AddRange(stateMachine.GetTransitionsFromState(stateMachine.GetState(i)));
		}

		return transitions.Count;
	}

	private static int GetTransitionsCountFromStateMachine(StateMachine sm)
	{
		int total = 0;
		List<Transition> transitions = new List<Transition>();

		for (int i = 0; i < sm.stateCount; ++i)
		{
			transitions.AddRange(sm.GetTransitionsFromState(sm.GetState(i)));
		}

		total = transitions.Count;

		for (int i = 0; i < sm.stateMachineCount; ++i)
		{
			total += GetTransitionsCountFromStateMachine(sm.GetStateMachine(i));
		}

		return total;
	}
	
	#endregion
	
	[MenuItem("Component/Miscellaneous/AnimatorEvents")]
    static void AddComponent()
    {
		if (Selection.activeGameObject != null) {
			if (Selection.activeGameObject.GetComponent<AnimatorEvents>() == null)
				Selection.activeGameObject.AddComponent(typeof(AnimatorEvents));
			else
				Debug.LogError("Can have only one AnimatorEvents");
		}
    }
}
