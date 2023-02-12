using UnityEditor;

[CustomEditor(typeof(Movement))]
public class EnemyStatsEditor : Editor
{
    // The various categories the editor will display the variables in 
    public enum DisplayCategory
    {
        FirstPerson, _2D, ThirdPerson
    }

    // The enum field that will determine what variables to display in the Inspector
    public DisplayCategory categoryToDisplay;

    // The function that makes the custom editor work
    public override void OnInspectorGUI()
    {
        // Display the enum popup in the inspector
        categoryToDisplay = (DisplayCategory) EditorGUILayout.EnumPopup("Display", categoryToDisplay);

        EditorGUILayout.Space(); 
        
        // Switch statement to handle what happens for each category
        switch (categoryToDisplay)
        {
            case DisplayCategory.FirstPerson:
                DisplayFirstPersonInfo(); 
                break;
			default:
				break;
        }
        serializedObject.ApplyModifiedProperties();
    }

    // When the categoryToDisplay enum is at "Basic"
    void DisplayFirstPersonInfo()
    {
		/*show allways wariables*/
		
		//general
		EditorGUILayout.PropertyField(serializedObject.FindProperty("objectWithModel"));
		
		//camera
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraPosition"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("sensativity"));
		
		//groundCheck
		EditorGUILayout.PropertyField(serializedObject.FindProperty("groundLayer"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("groundDrag"));
		
		SerializedProperty useCayoty = serializedObject.FindProperty("useCayotyTime");
        EditorGUILayout.PropertyField(useCayoty);
		if (useCayoty.boolValue)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("cayotyTime"));
		
		//speeds
		EditorGUILayout.PropertyField(serializedObject.FindProperty("walkSpeed"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerSpeedInAir"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("upperSpeedInAir"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("noInputNoSpeedGround"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("noInputNoSpeedAir"));
		
		SerializedProperty smoothSpeed = serializedObject.FindProperty("smoothSpeed");
		EditorGUILayout.PropertyField(smoothSpeed);
		if (smoothSpeed.boolValue)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothSpeedAccelerationGround"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothSpeedAccelerationAir"));
			SerializedProperty drasticSmoothSpeed = serializedObject.FindProperty("drasticSmoothSpeed");
			EditorGUILayout.PropertyField(drasticSmoothSpeed);
			if (drasticSmoothSpeed.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("drastic"));
		}
		
		//gravity
		EditorGUILayout.PropertyField(serializedObject.FindProperty("normalGravitation"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fallingGravitation"));
		
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox("Abilities", MessageType.None);
		
		//jump
		SerializedProperty jump = serializedObject.FindProperty("jump");
		EditorGUILayout.PropertyField(jump);
		if (jump.boolValue)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("holdJumpButton"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeight"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpDelay"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpRememberTime"));
			//duble jump
			SerializedProperty dubbleJump = serializedObject.FindProperty("dubbleJump");
			EditorGUILayout.PropertyField(dubbleJump);
			if (dubbleJump.boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("ammountOfJumpsInAir"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("dubbleJumpHeight"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("dubbleJumpDelay"));
			}
			//jump cut
			SerializedProperty jumpCut = serializedObject.FindProperty("jumpCut");
			EditorGUILayout.PropertyField(jumpCut);
			if (jumpCut.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCutStrength"));
			//jump speed
			SerializedProperty jumpSpeedManipulations = serializedObject.FindProperty("jumpSpeedManipulations");
			EditorGUILayout.PropertyField(jumpSpeedManipulations);
			if (jumpSpeedManipulations.boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("increaseSpeedAfterJumpBy"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maxObtainableSpeedByJump"));
			}
		}
		
		//input
		if (jump.boolValue)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpKey"));
		
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox("Debug", MessageType.None);
		
		//Gizmos
		SerializedProperty gizmosA = serializedObject.FindProperty("gizmosAllways");
        EditorGUILayout.PropertyField(gizmosA);
		SerializedProperty gizmosS = serializedObject.FindProperty("gizmosSelected");
        EditorGUILayout.PropertyField(gizmosS);
		if (gizmosA.boolValue || gizmosS.boolValue)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheck"));
		}
		
		//Debug variables
        SerializedProperty debug = serializedObject.FindProperty("debug");
        EditorGUILayout.PropertyField(debug);
        if (debug.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rb"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraHolder"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("cam"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("orientation"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckLength"));
			
			SerializedProperty debugText = serializedObject.FindProperty("debugText");
			EditorGUILayout.PropertyField(debugText);
			if (debugText.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("debugTextPrefab"));
        }
    }
}
