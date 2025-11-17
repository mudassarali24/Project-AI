using System;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public static class SavWav
{
    public static string ffmpegPath = "";
    const int HEADER_SIZE = 44;

    public static byte[] GetWav(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] wav = ConvertAndWrite(samples, clip.channels, clip.frequency);
        return wav;
    }

    private static byte[] ConvertAndWrite(float[] samples, int channels, int sampleRate)
    {
        MemoryStream stream = new MemoryStream();
        // Leave 44 bytes for header
        stream.Position = HEADER_SIZE;

        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        // Convert to PCM16
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * short.MaxValue);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        stream.Write(bytesData, 0, bytesData.Length);

        // Write header
        stream.Position = 0;
        // ChunkID “RIFF”
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes((int)(stream.Length - 8)), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
        stream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat = PCM
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(sampleRate * channels * 2), 0, 4); // ByteRate
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2); // BlockAlign
        stream.Write(BitConverter.GetBytes((short)16), 0, 2); // BitsPerSample
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes((int)(samples.Length * 2)), 0, 4);

        return stream.ToArray();
    }

    /// <summary>
    /// Converts raw PCM bytes (16-bit signed) to Unity AudioClip
    /// </summary>
    /// <param name="pcmData">PCM byte array</param>
    /// <param name="sampleRate">Sample rate of the PCM</param>
    /// <param name="channels">Number of channels</param>
    /// <param name="clipName">Name of the clip</param>
    /// <returns>AudioClip ready to play</returns>
    public static AudioClip CreateAudioClipFromPCM(byte[] pcmData, string clipName = "TTSClip", int sampleRate = 24000, int channels = 1)
    {
        int totalSamples = pcmData.Length / 2; // 16-bit = 2 bytes per sample
        float[] audioData = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            audioData[i] = sample / 32768f; // convert to float [-1,1]
        }

        AudioClip clip = AudioClip.Create(clipName, totalSamples / channels, channels, sampleRate, false);
        clip.SetData(audioData, 0);

        return clip;
    }
}