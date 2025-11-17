using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioRecorder))]
public class AudioRecorderEditor : Editor
{
    private AudioRecorder audioRecorder => ((AudioRecorder)target);
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        if (GUILayout.Button("Start Recording", GUILayout.Width(200)))
        {
            audioRecorder.StartRecording();
        }

        /*GUILayout.Space(5);
        
        if (GUILayout.Button("Stop Recording", GUILayout.Width(200)))
        {
            audioRecorder.StopRecording();
        }*/

        GUILayout.Space(5);
        
        if (GUILayout.Button("Play Recording", GUILayout.Width(200)))
        {
            audioRecorder.PlayRecordedAudio();
        }
        EditorGUILayout.EndHorizontal();
    }
}