using UnityEngine;

// Sons de les armes generats per codi (estil retro) i un reproductor 2D compartit
public static class WeaponSounds
{
    const int SampleRate = 44100;


    private static AudioClip shot, throwClip, impact;

    // Reprodueix un so en 2D (mateix volum a tot arreu), amb el to un poc aleatori
    public static void Play(AudioClip clip, float volume = 0.6f, float pitchVariation = 0.08f)
    {
        GameAudio.Play(clip,volume,SoundCategory.Weapons,pitchVariation);
    }

    public static AudioClip ShootClip(WeaponData w)
    {
        if (w.shootSound != null) return w.shootSound;
        return w.sound == WeaponSound.Llancament ? Throw : Shot;
    }

    public static AudioClip ImpactClip(WeaponData w)
    {
        if (w.impactSound != null) return w.impactSound;
        return w.sound == WeaponSound.Llancament ? Impact : null; // Les bales no fan "toc"
    }

    // "Bang" curt: esclat de soroll + colp greu
    public static AudioClip Shot
    {
        get
        {
            if (shot == null)
            {
                int n = (int)(SampleRate * 0.14f);
                float[] d = new float[n];
                System.Random rnd = new System.Random(11);
                float lp = 0f;
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / SampleRate;
                    float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    lp += (noise - lp) * 0.5f;
                    float thump = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(160f, 60f, t / 0.14f) * t);
                    d[i] = (lp * Mathf.Exp(-t * 45f) * 0.8f + thump * Mathf.Exp(-t * 25f) * 0.6f) * 0.6f;
                }
                shot = Make("Tret", d);
            }
            return shot;
        }
    }

    // "Fiuu": soroll filtrat que puja i baixa
    public static AudioClip Throw
    {
        get
        {
            if (throwClip == null)
            {
                const float dur = 0.2f;
                int n = (int)(SampleRate * dur);
                float[] d = new float[n];
                System.Random rnd = new System.Random(5);
                float lp = 0f;
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / SampleRate;
                    float k = t / dur;
                    float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    lp += (noise - lp) * Mathf.Lerp(0.05f, 0.3f, k); // S'obri el filtre: puja el to
                    d[i] = lp * Mathf.Sin(k * Mathf.PI) * 1.4f;
                }
                throwClip = Make("Llancament", d);
            }
            return throwClip;
        }
    }

    // "Toc" de pedra: colp greu i curt amb un click
    public static AudioClip Impact
    {
        get
        {
            if (impact == null)
            {
                int n = (int)(SampleRate * 0.12f);
                float[] d = new float[n];
                System.Random rnd = new System.Random(9);
                for (int i = 0; i < n; i++)
                {
                    float t = (float)i / SampleRate;
                    float tone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(220f, 80f, t / 0.12f) * t);
                    float click = (float)(rnd.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 200f);
                    d[i] = (tone * Mathf.Exp(-t * 35f) * 0.7f + click * 0.5f) * 0.7f;
                }
                impact = Make("Impacte", d);
            }
            return impact;
        }
    }

    static AudioClip Make(string name, float[] data)
    {
        AudioClip c = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        c.SetData(data, 0);
        return c;
    }
}
