using System.Collections.Generic;
using UnityEngine;

// Sis comportaments diferents; la simulacio nomes corre a l'amfitrio.
public class EnemyV140 : MonoBehaviour
{
    public enum Kind { Mimic, Healer, Shield, Mole, Sniper, Banshee }
    public Kind kind;
    public Sprite disguise;
    public ChestRarity rewardRarity = ChestRarity.Rar;
    EnemyHealth health;
    EnemyPathfindingAI ai;
    SpriteRenderer body, shield, shadow;
    Sprite creature;
    LineRenderer beam;
    float timer, healClock, waveUntil;
    int molePhase, shieldHits;
    bool awake, charging, channeling;
    Vector2 aim, facing = Vector2.down;
    Vector3 chestPosition;
    public int Actions { get; private set; }
    public int Interruptions { get; private set; }
    public bool Charging => charging;
    public bool ShieldIntact => kind == Kind.Shield && shieldHits < 3;
    public bool Invulnerable => kind == Kind.Mole && molePhase != 2;
    public byte VisualState => (byte)((awake ? 1 : 0) | (Invulnerable ? 2 : 0) | (ShieldIntact ? 4 : 0) |
        (charging ? 8 : 0) | (channeling ? 16 : 0) | (molePhase == 1 ? 32 : 0) | (Time.time < waveUntil ? 64 : 0));
    public Vector2 Aim => aim;
    public Vector2 Facing => facing;
    byte remoteState;

    void Awake()
    {
        health = GetComponent<EnemyHealth>(); ai = GetComponent<EnemyPathfindingAI>();
        body = GetComponent<SpriteRenderer>(); creature = body.sprite; chestPosition = transform.position;
        timer = kind == Kind.Banshee ? 4f : kind == Kind.Sniper ? 1.2f : 2f;
        if (kind == Kind.Shield)
        {
            shield = Fx.MakeSprite("Escut frontal", transform.position, Fx.Disc, new Color(.45f, .8f, 1f, .8f), "Player", 8);
            shield.transform.SetParent(transform, true);
        }
        if (kind == Kind.Mole)
        {
            shadow = Fx.MakeSprite("Ombra del talp", transform.position, Fx.Disc, new Color(.05f, .03f, .02f, .65f), "Player", 1);
            shadow.transform.SetParent(transform, true);
        }
        if (kind == Kind.Healer || kind == Kind.Sniper)
        {
            var go = new GameObject("Feix"); go.transform.SetParent(transform, false);
            beam = go.AddComponent<LineRenderer>(); beam.material = new Material(Shader.Find("Sprites/Default"));
            beam.positionCount = 2; beam.startWidth = beam.endWidth = .045f; beam.sortingLayerName = "Player"; beam.sortingOrder = 8;
            beam.startColor = beam.endColor = kind == Kind.Healer ? Color.green : new Color(1f, .2f, .3f);
            beam.enabled = false;
        }
        health.Died += Died;
    }

    void Start()
    {
        if (health.IsNetProxy) return;
        ai.Alert();
        ai.mode = kind == Kind.Shield ? EnemyPathfindingAI.Mode.Chase : EnemyPathfindingAI.Mode.KeepDistance;
        ai.preferredDistance = kind == Kind.Healer ? 5f : 6f;
        if (kind == Kind.Mimic || kind == Kind.Mole) { ai.mode = EnemyPathfindingAI.Mode.Hold; ai.speedMultiplier = 0; }
        if (kind == Kind.Mole) ai.enabled = false;
    }

    public void ApplyVisualState(byte state, Vector2 endpoint, Vector2 direction)
    { remoteState = state; aim = endpoint; facing = direction; }

    void LateUpdate()
    {
        byte state = health.IsNetProxy ? remoteState : VisualState;
        if (kind == Kind.Mimic)
        {
            bool revealed = (state & 1) != 0;
            body.sprite = revealed ? creature : disguise;
            var chest = GetComponent<Chest>();
            if (revealed && chest != null && chest.enabled) { chest.NetMarkOpened(); chest.enabled = false; TileNav.RemoveBlocker(chestPosition); }
        }
        if (kind == Kind.Mole)
        {
            var color = body.color; color.a = (state & 2) != 0 ? 0f : 1f; body.color = color;
            shadow.enabled = (state & 32) != 0;
            shadow.transform.position = aim;
            shadow.transform.localScale = Vector3.one * (health.IsNetProxy ? 1f : Mathf.Lerp(.25f, 1.5f, 1f - timer));
        }
        if (shield != null)
        {
            shield.enabled = (state & 4) != 0;
            shield.transform.localPosition = facing * .45f;
            shield.transform.localScale = new Vector3(.13f, .8f, 1f);
            shield.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg);
        }
        if (beam != null)
        {
            beam.enabled = (state & (8 | 16)) != 0;
            beam.SetPosition(0, transform.position); beam.SetPosition(1, aim);
        }
        if (kind == Kind.Banshee && (state & 64) != 0 && waveUntil <= Time.time)
        { waveUntil = Time.time + .45f; Fx.Burst(transform.position, new Color(.65f, .7f, 1f), 18, 5f); }
    }

    void Update()
    {
        if (health.IsNetProxy || health.IsDead) return;
        Transform target = NetCoop.NearestTarget(transform.position); if (target == null) return;
        facing = ((Vector2)target.position - (Vector2)transform.position).normalized;
        if (kind == Kind.Mimic && !awake)
        {
            foreach (var person in Targets()) if (Vector2.Distance(person.position, transform.position) <= 1.1f) { Reveal(person); break; }
            return;
        }
        timer -= Time.deltaTime;
        switch (kind)
        {
            case Kind.Healer: HealAlly(); break;
            case Kind.Mole: Burrow(target); break;
            case Kind.Sniper: Snipe(target); break;
            case Kind.Banshee:
                if (timer <= 0) { timer = 4f; Scream(); }
                break;
            case Kind.Mimic:
                if (timer <= 0 && Vector2.Distance(target.position, transform.position) <= 1.4f)
                { timer = 1.2f; Bite(target); }
                break;
        }
    }

    public bool Reveal(Transform opener = null)
    {
        if (kind != Kind.Mimic) return false;
        if (health.IsNetProxy || awake) return true;
        awake = true; ai.mode = EnemyPathfindingAI.Mode.Chase; ai.speedMultiplier = 1; ai.Alert();
        TileNav.RemoveBlocker(chestPosition);
        var target = opener != null ? opener : NetCoop.NearestTarget(transform.position);
        if (target != null) Bite(target);
        timer = 1.2f;
        return true;
    }

    void Bite(Transform target)
    {
        Actions++; MobSounds.Play("mimic_mossegada", transform.position);
        Fx.Burst(transform.position, new Color(1f, .6f, .25f), 8, 2f);
        if (Vector2.Distance(target.position, transform.position) <= 1.8f) NetCoop.DamagePlayer(target, 3);
    }

    public bool BlockHit(Vector2 direction, bool melee, bool projectile)
    {
        if (Invulnerable) return true;
        if (kind == Kind.Mimic && !awake) Reveal();
        if (!ShieldIntact) return false;
        if (melee)
        {
            shieldHits++; Actions++; MobSounds.Play("escut_resso", transform.position);
            if (shieldHits == 3) Fx.Burst(transform.position, Color.cyan, 10, 2f);
            return true;
        }
        if (projectile && (direction.sqrMagnitude < .001f || Vector2.Dot(direction.normalized, facing) < -.15f))
        { MobSounds.Play("escut_resso", transform.position, .35f); return true; }
        return false;
    }

    public void Damaged()
    {
        if (kind != Kind.Sniper || !charging) return;
        charging = false; timer = 2f; ai.speedMultiplier = 1f; Interruptions++;
    }

    void HealAlly()
    {
        EnemyHealth ally = null; float best = 7f;
        foreach (var candidate in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
        {
            if (candidate == health || candidate.IsDead || !candidate.enabled || candidate.currentHealth >= candidate.maxHealth) continue;
            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance < best && TileNav.LineOfSight(transform.position, candidate.transform.position, 0f, true)) { best = distance; ally = candidate; }
        }
        channeling = ally != null;
        if (ally == null) { healClock = 0; return; }
        aim = ally.transform.position; healClock += Time.deltaTime;
        if (healClock < .5f) return;
        healClock -= .5f; ally.currentHealth = Mathf.Min(ally.maxHealth, ally.currentHealth + 1); Actions++;
        if (timer <= 0) { timer = 1f; MobSounds.Play("sanador_cura", transform.position, .35f); }
    }

    void Snipe(Transform target)
    {
        if (timer > 0) return;
        if (charging)
        {
            charging = false; ai.speedMultiplier = 1; timer = 2.5f; Actions++;
            Vector2 direction = (aim - (Vector2)transform.position).normalized;
            EnemyBullet.Fire((Vector2)transform.position + direction * .4f, direction, 12f, 3, transform, 1f, Color.cyan);
            MobSounds.Play("franctirador_tret", transform.position);
        }
        else if (Vector2.Distance(transform.position, target.position) < 12f && TileNav.LineOfSight(transform.position, target.position, 0f, true))
        {
            charging = true; ai.speedMultiplier = 0; aim = target.position; timer = 1f;
            MobSounds.Play("franctirador_carrega", transform.position, .4f);
        }
    }

    void Burrow(Transform target)
    {
        if (molePhase == 0 && timer > 0)
        {
            Vector2 next = Vector2.MoveTowards(transform.position, target.position, 3f * Time.deltaTime);
            if (TileNav.CircleFree(next, .3f)) { transform.position = next; GetComponent<Rigidbody2D>().position = next; }
        }
        if (timer > 0) return;
        if (molePhase == 0)
        {
            aim = TileNav.CircleFree(target.position, .35f) ? (Vector2)target.position : (Vector2)transform.position;
            molePhase = 1; timer = 1f; MobSounds.Play("talp_cavar", aim, .45f);
        }
        else if (molePhase == 1)
        {
            molePhase = 2; timer = 1.5f; transform.position = aim; GetComponent<Rigidbody2D>().position = aim;
            Actions++; Fx.Burst(aim, new Color(.6f, .4f, .2f), 10, 2f);
            foreach (var person in Targets()) if (Vector2.Distance(person.position, aim) <= .9f) NetCoop.DamagePlayer(person, 2);
        }
        else { molePhase = 0; timer = 2f; }
    }

    void Scream()
    {
        Actions++; waveUntil = Time.time + .45f; MobSounds.Play("banshee_lament", transform.position);
        Fx.Burst(transform.position, new Color(.65f, .7f, 1f), 18, 5f);
        foreach (var person in Targets())
        {
            if (Vector2.Distance(person.position, transform.position) > 4f || !TileNav.LineOfSight(transform.position, person.position, 0f, true)) continue;
            NetCoop.DamagePlayer(person, 1);
            NetCoop.PushPlayer(person, ((Vector2)person.position - (Vector2)transform.position).normalized, 2f);
        }
    }

    static IEnumerable<Transform> Targets()
    {
        var local = FindFirstObjectByType<PlayerMovement>(); if (local != null) yield return local.transform;
        foreach (var avatar in NetShared.Avatars()) if (avatar != null && !avatar.Down) yield return avatar.transform;
    }

    void Died(EnemyHealth dead)
    {
        if (kind != Kind.Mimic || health.IsNetProxy) return;
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        var candidates = new List<WeaponData>();
        ChestRarity reward = (ChestRarity)Mathf.Min(3, (int)rewardRarity + 1);
        foreach (var w in catalog.weapons) if (w != null && w.rarity == reward) candidates.Add(w);
        if (candidates.Count > 0) ItemPickup.Spawn(candidates[Random.Range(0, candidates.Count)], transform.position, transform.position + Vector3.down);
    }

    void OnDestroy()
    {
        if (kind == Kind.Mimic) TileNav.RemoveBlocker(chestPosition);
        if (beam != null && beam.material != null) Destroy(beam.material);
    }
}
