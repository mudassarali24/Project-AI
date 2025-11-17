using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OpenAIManager))]
public class OpenAIManagerEditor : Editor
{
    private OpenAIManager openAIManager => ((OpenAIManager)target);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Send to GPT", GUILayout.Width(200)))
        {
            openAIManager.GenerateTranscribedTextResponse();
        }

        GUILayout.Space(5);


        if (GUILayout.Button("Speak Reply", GUILayout.Width(200)))
        {
            openAIManager.SpeakGeneratedText();
        }

        EditorGUILayout.EndHorizontal();
    }
}