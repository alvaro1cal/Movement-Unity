using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AdvancedController))]
public class ControllerEditor : Editor {

	private bool setup = true;
	private bool jump = false;
	private bool climb = false;
	private bool fight = false;
	private bool move = false;
	private string[] inputs;
	private int SelectedHorizontalMoveInput;
	private int SelectedVerticalMoveInput;
	private int SelectedJumpInput;
	private int SelectedFightInput;
	AdvancedController myTarget;

	void OnEnable() {
		SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
		SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");
		inputs = new string[axesProperty.arraySize];
		SerializedProperty axisProperty;
		for(int i = 0; i < axesProperty.arraySize; i++){
			axisProperty = axesProperty.GetArrayElementAtIndex(i);
			inputs[i] = GetChildProperty(axisProperty, "m_Name").stringValue;
		}

		myTarget = (AdvancedController)target;
		for(int i = 0; i < inputs.Length; i++){
			if(inputs[i].Equals(myTarget.HorizontalMoveInput)){
				SelectedHorizontalMoveInput = i;
			}
			if(inputs[i].Equals(myTarget.VerticalMoveInput)){
				SelectedVerticalMoveInput = i;
			}
			if(inputs[i].Equals(myTarget.JumpInput)){
				SelectedJumpInput = i;
			}
			if(inputs[i].Equals(myTarget.FightInput)){
				SelectedFightInput = i;
			}
		}
	}

	public override void OnInspectorGUI()
	{
		Rect rect = EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Setup"))
		{
			setup = true;
			jump = false;
			climb = false;
			fight = false;
			move = false;
		}
		if(GUILayout.Button("Jump"))
		{
			setup = false;
			jump = true;
			climb = false;
			fight = false;
			move = false;
		}
		if(GUILayout.Button("Climb"))
		{
			setup = false;
			jump = false;
			climb = true;
			fight = false;
			move = false;
		}
		if(GUILayout.Button("Fight"))
		{
			setup = false;
			jump = false;
			climb = false;
			fight = true;
			move = false;
		}
		if(GUILayout.Button("Move"))
		{
			setup = false;
			jump = false;
			climb = false;
			fight = false;
			move = true;
		}
		EditorGUILayout.EndHorizontal();

		rect = EditorGUILayout.BeginHorizontal();
		GUILayout.Label("");
		EditorGUILayout.EndHorizontal();
		rect.height += 2;

		GUIStyle style = new GUIStyle();
		style.padding = new RectOffset(10,0,2,0);
		style.normal.background = MakeText(400,400, (Color) new Color32(22,160,133,255));

		//GUI.Box(rect,"Jump", style);
		//GUI.color = oldColor;
		if(setup){
			myTarget.capsuleColliderCenter = EditorGUILayout.Vector3Field("Center", myTarget.capsuleColliderCenter);
			myTarget.radius = EditorGUILayout.FloatField("Radius", myTarget.radius);
			myTarget.height = EditorGUILayout.FloatField("Height", myTarget.height);
			myTarget.groundLayer = EditorGUILayout.LayerField("Ground layer", myTarget.groundLayer);

		}
		if(jump){
			myTarget.allowJump = EditorGUILayout.Toggle("Jump", myTarget.allowJump);
			myTarget.totalOfJumps = EditorGUILayout.IntField("Number of jumps", myTarget.totalOfJumps);
			myTarget.jumpUpTag = EditorGUILayout.TagField("Jump Up Tag", myTarget.jumpUpTag); 
			myTarget.allowFloat = EditorGUILayout.Toggle("Float", myTarget.allowFloat);
			myTarget.allowAutomaticJump = EditorGUILayout.Toggle("Automatic jump", myTarget.allowAutomaticJump);
			myTarget.jumpObstacleTag = EditorGUILayout.TagField("Aut. Jump Tag", myTarget.jumpObstacleTag); 
			myTarget.allowWallJjump = EditorGUILayout.Toggle("Wall jump", myTarget.allowWallJjump);
			myTarget.jumpWallTag = EditorGUILayout.TagField("Wall Jump Tag", myTarget.jumpWallTag); 
			SelectedJumpInput = EditorGUILayout.Popup("Jump Input", SelectedJumpInput, inputs);
			myTarget.JumpInput = inputs[SelectedJumpInput];
		}
		if(climb){
			myTarget.climbTag = EditorGUILayout.TagField("Climb Tag", myTarget.climbTag); 
			myTarget.climbDistance = EditorGUILayout.FloatField("Minimum distance for climb", myTarget.climbDistance);
			myTarget.offsetHandClimb = EditorGUILayout.FloatField("Offset hand", myTarget.offsetHandClimb);
		}
		if(fight){
			myTarget.allowFight = EditorGUILayout.Toggle("Fight", myTarget.allowFight);
			SelectedFightInput = EditorGUILayout.Popup("Fight Input", SelectedFightInput, inputs);
			myTarget.FightInput = inputs[SelectedFightInput];
		}
		if(move){
			myTarget.infinityRunner = EditorGUILayout.Toggle("Infinity Runner", myTarget.infinityRunner);
			myTarget.allowCrouch = EditorGUILayout.Toggle("Crouch", myTarget.allowCrouch);
			SelectedHorizontalMoveInput = EditorGUILayout.Popup("Horizontal Input", SelectedHorizontalMoveInput, inputs);
			SelectedVerticalMoveInput = EditorGUILayout.Popup("Vertical Input", SelectedVerticalMoveInput, inputs);
			myTarget.HorizontalMoveInput = inputs[SelectedHorizontalMoveInput];
			myTarget.VerticalMoveInput = inputs[SelectedVerticalMoveInput];
		}
	}

	private Texture2D MakeText(int width, int height, Color color){
		Color[] pix = new Color[width * height];
		for(int i = 0; i < pix.Length; ++i){
			pix[i] = color;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}

	private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
	{
		SerializedProperty child = parent.Copy();
		child.Next(true);
		do
		{
			if (child.name == name) return child;
		}
		while (child.Next(false));
		return null;
	}
}
