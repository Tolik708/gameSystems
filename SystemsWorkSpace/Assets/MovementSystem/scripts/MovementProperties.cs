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
		EditorGUILayout.PropertyField(serializedObject.FindProperty("changeSpeedInAir"));
		
		SerializedProperty smoothSpeed = serializedObject.FindProperty("smoothSpeed");
		EditorGUILayout.PropertyField(smoothSpeed);
		if (smoothSpeed.boolValue)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothSpeedSpeed"));
			SerializedProperty drasticSmoothSpeed = serializedObject.FindProperty("drasticSmoothSpeed");
			EditorGUILayout.PropertyField(drasticSmoothSpeed);
			if (drasticSmoothSpeed.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("drastic"));
		}
		
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
