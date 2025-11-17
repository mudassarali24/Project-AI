using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;

public class OpenAIManager : MonoBehaviour
{
    private const string openAI_API_KEY = "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";

    private const string transcribeEndpoint = "https://api.openai.com/v1/audio/transcriptions";
    private const string generationEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string speechEndpoint = "https://api.openai.com/v1/audio/speech?format=wav";

    private const string gpt_model = "gpt-4o-mini";


    [Header("Debug")]
    [TextArea]
    public string transcribedText;

    [TextArea]
    public string aiReply;
    public AudioSource audioSource;


    public static OpenAIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Transcribes audio clip to text
    /// </summary>
    /// <param name="clip">Clip to transcribe</param>
    /// <param name="onTranscriptionReady">Transcribed Text.</param>
    /// <returns></returns>
    public IEnumerator TranscribeAudioClip(AudioClip clip, Action<string> onTranscriptionReady)
    {
        // we convert the audioclip as wav byte[]
        byte[] wavData = SavWav.GetWav(clip);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("model", "whisper-1"));
        formData.Add(new MultipartFormFileSection("file", wavData, "speech.wav", "audio/wav"));

        // Now we send request
        UnityWebRequest www = UnityWebRequest.Post(transcribeEndpoint, formData);
        www.SetRequestHeader("Authorization", $"Bearer {openAI_API_KEY}");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Transcription failed: {www.error}");
            onTranscriptionReady?.Invoke(null);
        }
        else
        {
            var json = www.downloadHandler.text;
            WhisperResponse resp = JsonUtility.FromJson<WhisperResponse>(json);
            onTranscriptionReady?.Invoke(resp.text);
            transcribedText = resp.text;
            yield return new WaitForSeconds(0.2f); // wait little bit
            StartCoroutine(GetAIResponse(transcribedText, (string response) =>
            {
                Debug.Log(response);
            }));
            // Debug.Log($"Received transcription response: {resp.text}");
        }
    }

    /// <summary>
    /// Generates text based on prompt.
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="npcPersonality"></param>
    /// <param name="callback"></param>
    /// <returns>AI Reply</returns>
    public IEnumerator GetAIResponse(string prompt, Action<string> callback, string npcPersonality = "")
    {
        if (string.IsNullOrEmpty(npcPersonality))
        {
            // setting default personality
            npcPersonality = "You are an NPC in a game. Reply as a game character without breaking immersion in only one or two lines.";
        }

        // json body
        JObject jsonBody = new JObject
        {
            ["model"] = gpt_model,
            ["messages"] = new JArray
            {
                new JObject
                {
                    ["role"] = "system",
                    ["content"] = npcPersonality
                },
                new JObject
                {
                    ["role"] = "user",
                    ["content"] = prompt
                }
            }
        };

        string bodyString = jsonBody.ToString();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyString);

        // Create & Send Request
        UnityWebRequest request = new UnityWebRequest(generationEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {openAI_API_KEY}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error generating text: " + request.error + "\n" + request.downloadHandler.text);
            callback?.Invoke(null);
            yield break;
        }

        // Parse json response
        string json = request.downloadHandler.text;
        JObject parsed = JObject.Parse(json);

        string _aiReply = parsed["choices"][0]["message"]["content"].ToString();
        callback?.Invoke(_aiReply);
        aiReply = _aiReply;
        yield return new WaitForSeconds(0.2f); // wait a little bit
        StartCoroutine(SpeakText(aiReply, PlayGeneratedAudioClip));
    }

    /// <summary>
    /// Speaks the text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="callback"></param>
    /// <returns>Audio clip with voice.</returns>
    public IEnumerator SpeakText(string text, Action<AudioClip> callback)
    {
        // json body
        var json = @"{
            ""model"": ""gpt-4o-mini-tts"",
            ""voice"": ""alloy"",
            ""input"": """ + text.Replace("\"", "\\\"") + @""",
            ""response_format"": ""pcm""
        }";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Create & Send Request
        UnityWebRequest request = new UnityWebRequest(speechEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {openAI_API_KEY}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("TTS Error: " + request.error + "\n" + request.downloadHandler.text);
            callback?.Invoke(null);
            yield break;
        }

        // Get WAV file data
        byte[] audioData = request.downloadHandler.data;

        try
        {
            AudioClip clip = SavWav.CreateAudioClipFromPCM(audioData, "AI_Response");
            callback?.Invoke(clip);
            Debug.Log("MP3 converted to Wav!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to Convert MP3 to WAV: " + ex.Message);
        }
    }



    public void GenerateTranscribedTextResponse()
    {
        if (string.IsNullOrEmpty(transcribedText)) return;
        StartCoroutine(GetAIResponse(transcribedText, (string response) =>
        {
            Debug.Log(response);
        }));
    }


    public void SpeakGeneratedText()
    {
        if (string.IsNullOrEmpty(aiReply)) return;
        StartCoroutine(SpeakText(aiReply, PlayGeneratedAudioClip));
    }

    private void PlayGeneratedAudioClip(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.Play();
    }
}

[Serializable]
public class WhisperResponse
{
    public string text;
}