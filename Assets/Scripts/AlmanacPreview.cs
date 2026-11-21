using UnityEngine;
using UnityEngine.UI;

public sealed class AlmanacPreview : MonoBehaviour
{
    public enum Pattern { None, Bolt, Fan, Ring, Charge, Jump, Sweep, Beam, Hazard, Amulet }

    Image subject;
    readonly Image[] motions = new Image[7];
    Image target;
    Sprite projectile;
    Pattern pattern;
    float started;

    public void Build(Image artwork)
    {
        subject = artwork;
        target = UIKit.Picture(transform, Fx.PixelCircle(12), new Vector2(166, -10), new Vector2(45, 45));
        target.color = new Color(.63f, .48f, .28f);
        target.gameObject.SetActive(false);
        for (int i = 0; i < motions.Length; i++)
        {
            motions[i] = UIKit.Picture(transform, Fx.PixelCircle(6), Vector2.zero, new Vector2(25, 25));
            motions[i].gameObject.SetActive(false);
        }
    }

    public void Show(Sprite icon, Sprite shot, Pattern movement, bool unlocked)
    {
        subject.sprite = icon;
        subject.color = icon == null ? Color.clear : unlocked ? Color.white : Color.black;
        subject.rectTransform.anchoredPosition = Vector2.zero;
        subject.rectTransform.localScale = Vector3.one;
        subject.rectTransform.localRotation = Quaternion.identity;
        pattern = unlocked && icon != null ? movement : Pattern.None;
        projectile = shot;
        foreach (var mote in motions) mote.gameObject.SetActive(false);
        target.gameObject.SetActive(pattern != Pattern.None && pattern != Pattern.Amulet);
        started = Time.unscaledTime;
    }

    public static Pattern ForEnemy(EnemyBoss boss, EnemyV140 enemy, bool shooter)
    {
        if (boss != null)
        {
            switch (boss.arenaKind)
            {
                case EnemyBoss.ArenaKind.Crystal: return Pattern.Beam;
                case EnemyBoss.ArenaKind.Knight:
                case EnemyBoss.ArenaKind.Hydra:
                case EnemyBoss.ArenaKind.Cook: return Pattern.Sweep;
                case EnemyBoss.ArenaKind.Worm: return Pattern.Charge;
                case EnemyBoss.ArenaKind.Phoenix: return Pattern.Hazard;
                case EnemyBoss.ArenaKind.Sewer: return Pattern.Ring;
                case EnemyBoss.ArenaKind.Professor: return Pattern.Fan;
                default: return Pattern.Jump;
            }
        }
        if (enemy != null)
        {
            switch (enemy.kind)
            {
                case EnemyV140.Kind.Sniper:
                case EnemyV140.Kind.Healer:
                case EnemyV140.Kind.FloatingEye:
                case EnemyV140.Kind.GiantFrog: return Pattern.Beam;
                case EnemyV140.Kind.Slug:
                case EnemyV140.Kind.Salamander:
                case EnemyV140.Kind.Bomber: return Pattern.Hazard;
                case EnemyV140.Kind.Mole: return Pattern.Jump;
                case EnemyV140.Kind.AncientTurret:
                case EnemyV140.Kind.AshMage: return Pattern.Ring;
                case EnemyV140.Kind.Scorpion: return Pattern.Sweep;
            }
        }
        return shooter ? Pattern.Bolt : Pattern.Charge;
    }

    void Update()
    {
        if (pattern == Pattern.None || subject == null) return;
        float time = (Time.unscaledTime - started) % 2.4f;
        float t = Mathf.Clamp01((time - .65f) / 1.05f);
        var rect = subject.rectTransform;
        bool orbit = pattern == Pattern.Amulet;
        rect.anchoredPosition = orbit ? new Vector2(0, Mathf.Sin(time * 4f) * 10f) : new Vector2(-99f, 0);
        rect.localScale = Vector3.one * (time < .65f ? 1f + Mathf.Sin(time * 18f) * .04f : 1f);
        rect.localRotation = Quaternion.identity;
        if (pattern == Pattern.Charge) rect.anchoredPosition += Vector2.right * Mathf.Sin(t * Mathf.PI) * 215f;
        if (pattern == Pattern.Jump) rect.anchoredPosition += new Vector2(t * 215f, Mathf.Sin(t * Mathf.PI) * 75f);
        if (pattern == Pattern.Sweep) rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI) * -77f);
        int count = pattern == Pattern.Fan ? 4 : pattern == Pattern.Ring || orbit ? 7 :
            pattern == Pattern.Charge || pattern == Pattern.Jump ? 0 : 1;

        for (int i = 0; i < motions.Length; i++)
        {
            var mote = motions[i];
            bool active = i < count && time > .65f && time < 1.7f;
            mote.gameObject.SetActive(active);
            if (!active) continue;
            var m = mote.rectTransform;
            m.localRotation = Quaternion.identity;
            m.sizeDelta = new Vector2(23, 23);
            mote.sprite = projectile != null ? projectile : Fx.PixelCircle(6);
            mote.color = orbit ? UIKit.Gold : new Color(1f, .38f, .38f);
            float angle = i * (2f * Mathf.PI / motions.Length);
            if (pattern == Pattern.Ring || orbit)
            {
                float radius = orbit ? 90f + 15f * t : 20f + 125f * t;
                m.anchoredPosition = rect.anchoredPosition + new Vector2(Mathf.Cos(angle + t), Mathf.Sin(angle + t)) * radius;
            }
            else if (pattern == Pattern.Beam)
            {
                m.sizeDelta = new Vector2(210, 12);
                m.anchoredPosition = new Vector2(35, -4);
                m.localRotation = Quaternion.Euler(0, 0, (t - .5f) * 16f);
            }
            else if (pattern == Pattern.Hazard || pattern == Pattern.Sweep)
            {
                mote.sprite = Fx.Disc;
                mote.color = new Color(1f, .27f, .2f, .52f);
                m.anchoredPosition = new Vector2(pattern == Pattern.Hazard ? 161 : 35, -12);
                m.sizeDelta = Vector2.one * (25f + Mathf.Sin(t * Mathf.PI) * 110f);
            }
            else m.anchoredPosition = new Vector2(-18f + 205f * t, (i - (count - 1) * .5f) * 38f * t);
        }

        if (target.gameObject.activeSelf)
            target.color = t > .72f && time < 1.7f ? UIKit.Red : new Color(.63f, .48f, .28f);
    }
}
