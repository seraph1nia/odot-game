namespace Game;

public static class CueSynthesis
{
    public const int SampleRate = 22050;
    // Original PCM: decaying sine with a fixed-seed noise transient, 0.12 seconds.
    public static byte[] Generate(int cue)
    {
        int samples = SampleRate * 12 / 100;
        var pcm = new byte[samples * 2]; uint noise = 0x12345678;
        double frequency = cue switch { 0 => 660, 1 => 880, 2 => 220, 3 => 330, 4 => 440, _ => 110 };
        for (int n = 0; n < samples; n++)
        {
            noise ^= noise << 13; noise ^= noise >> 17; noise ^= noise << 5;
            double time = n / (double)SampleRate, envelope = Math.Pow(1 - n / (double)samples, 3);
            double transient = ((noise & 65535) / 32767.5 - 1) * (cue >= 2 ? 0.35 : 0.08);
            short sample = (short)(Math.Clamp((Math.Sin(2 * Math.PI * frequency * time) * .65 + transient) * envelope, -1, 1) * 6000);
            pcm[n * 2] = (byte)(sample & 255); pcm[n * 2 + 1] = (byte)((sample >> 8) & 255);
        }
        return pcm;
    }
}
