using System.Collections;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    public static string microphoneDevice;
    public static AudioClip recordedAudio;

    [Header("Settings")]
    public int sampleRate = 44100;
    public float silenceThreshold = 0.02f;
    public float silenceDuration = 1.0f;

    private static bool isRecording;
    private static float silenceTimer;

    public static AudioRecorder Instance;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Initializes Audio Recorder
    /// </summary>
    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    /// <summary>
    /// Starts the recording
    /// </summary>
    public void StartRecording()
    {
        if (isRecording) return;

        Debug.Log("Recording started...");
        isRecording = true;
        silenceTimer = 0f;

        recordedAudio = Microphone.Start(microphoneDevice, false, 30, sampleRate);
        StartCoroutine(CheckForSilence());
    }

    private IEnumerator CheckForSilence()
    {
        float[] samples = new float[1024];

        while (isRecording)
        {
            int micPos = Microphone.GetPosition(microphoneDevice);

            // Get data lower than 1024
            recordedAudio.GetData(samples, micPos > samples.Length ? micPos - samples.Length : 0);

            float maxVolume = 0f;
            foreach (float sample in samples)
            {
                maxVolume = Mathf.Max(maxVolume, Mathf.Abs(sample));
            }

            // if max value is less than threshold, we check for possible case
            if (maxVolume < silenceThreshold)
            {
                silenceTimer += Time.deltaTime;

                if (silenceTimer > silenceDuration)
                {
                    // now we stop the recording
                    StopRecording();
                }
            }
            else
            {
                silenceTimer = 0f;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Stops the recording
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording) return;
        Debug.Log("Recording Stopped (silence detected)!");

        int recordedSamples = Microphone.GetPosition(microphoneDevice);
        Microphone.End(microphoneDevice);

        // Trim the clip to recorded length
        float[] samples = new float[recordedSamples];
        // we store data from recorded audio
        recordedAudio.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create("Trimmed_Clip", recordedSamples, 1, sampleRate, false);
        // we set recorded data to trimmed clip
        trimmedClip.SetData(samples, 0);

        recordedAudio = trimmedClip;
        isRecording = false;
        Debug.Log($"Saved AudioClip: {recordedAudio.samples} samples.");

        StartCoroutine(OpenAIManager.Instance.TranscribeAudioClip(recordedAudio, (string result) =>
        {
            Debug.Log(result);
        }));
    }

    /// <summary>
    /// Plays the recorded audio.
    /// </summary>
    public void PlayRecordedAudio()
    {
        if (recordedAudio == null) return;
        TryGetComponent<AudioSource>(out var source);
        if (source == null)
        {
            Debug.LogError("No audio source attached!");
            return;
        }
        source.clip = recordedAudio;
        source.Play();
    }
}