using System.Collections.Generic;
using UnityEngine;

// Sis comportaments diferents; la simulacio nomes corre a l'amfitrio.
public class EnemyV140 : MonoBehaviour
{
    public enum Kind { Mimic, Healer, Shield, Mole, Sniper, Banshee, Salamander, Slug, Scorpion, SnowWolf, Bomber, AncientTurret, GoldenBeetle, FloatingEye, GiantFrog, AshMage } // v1.4.1: sis enemics i quatre microbosses
    public Kind kind;
    public Sprite disguise;
    public ChestRarity rewardRarity = ChestRarity.Rar;
    public GameObject minion;            // Ull Flotant: els ullets que invoca
    EnemyHealth health;
    EnemyPathfindingAI ai;
    SpriteRenderer body, shield, shadow;
    Sprite creature;
    LineRenderer beam;
    float timer, healClock, waveUntil;
    int molePhase, shieldHits;
    Collider2D[] moleColliders;
    bool awake, charging, channeling;
    Vector2 aim, facing = Vector2.down;
    Vector3 chestPosition;
    public int Actions { get; private set; }
    public int Interruptions { get; private set; }
    public bool Charging => charging;
    public bool ShieldIntact => (kind == Kind.Shield && shieldHits < 3) || kind == Kind.Scorpion || (kind == Kind.AshMage && orbsLeft > 0); // Mag de Cendra: l escut son les orbes
    public bool IsMicroboss => kind >= Kind.GoldenBeetle;
    public bool Invulnerable => (kind == Kind.Mole && molePhase != 2) || (kind == Kind.GoldenBeetle && phase == 1) || (kind == Kind.FloatingEye && phase == 0);
    public byte VisualState => (byte)((awake ? 1 : 0) | (Invulnerable ? 2 : 0) | (ShieldIntact ? 4 : 0) |
        (charging ? 8 : 0) | (channeling ? 16 : 0) | (molePhase == 1 ? 32 : 0) | (Time.time < waveUntil ? 64 : 0) | (IsWolfLeader ? 128 : 0));
    public Vector2 Aim => aim;
    public Vector2 Facing => facing;
    byte remoteState;

    void Awake()
    {
        health = GetComponent<EnemyHealth>(); ai = GetComponent<EnemyPathfindingAI>();
        body = GetComponent<SpriteRenderer>(); creature = body.sprite; chestPosition = transform.position;
        timer = kind == Kind.Banshee ? 4f : kind == Kind.Sniper ? 1.2f : 2f;
        if (kind == Kind.Shield || kind == Kind.Scorpion)
        {
            shield = Fx.MakeSprite("Escut frontal", transform.position, Fx.Disc, new Color(.45f, .8f, 1f, .8f), "Player", 8);
            shield.transform.SetParent(transform, true);
        }
        if (kind == Kind.Mole)
        {
            shadow = Fx.MakeSprite("Ombra del talp", transform.position, Fx.Disc, new Color(.05f, .03f, .02f, .65f), "Player", 1);
            shadow.transform.SetParent(transform, true);
        }
        if (kind == Kind.Healer || kind == Kind.Sniper || kind == Kind.Scorpion || kind == Kind.FloatingEye || kind == Kind.GiantFrog)
        {
            var go = new GameObject("Feix"); go.transform.SetParent(transform, false);
            beam = go.AddComponent<LineRenderer>(); beam.material = new Material(Shader.Find("Sprites/Default"));
            beam.positionCount = 2; beam.startWidth = beam.endWidth = .045f; beam.sortingLayerName = "Player"; beam.sortingOrder = 8;
            beam.startColor = beam.endColor = kind == Kind.Healer ? Color.green : kind == Kind.Scorpion ? new Color(1f, .6f, .1f) : kind == Kind.FloatingEye ? new Color(.8f, .3f, 1f) : kind == Kind.GiantFrog ? new Color(1f, .55f, .65f) : new Color(1f, .2f, .3f);
            if (kind == Kind.FloatingEye) beam.startWidth = beam.endWidth = .1f;
            beam.enabled = false;
        }
        health.Died += Died;
        if (IsMicroboss) MicrobossBar.Attach(gameObject, MicrobossName(kind), MicrobossColor(kind)); // barra flotant, a tots els equips
    }

    void Start()
    {
        if (health.IsNetProxy) return;
        ai.Alert();
        ai.mode = kind == Kind.Shield || kind == Kind.Salamander || kind == Kind.Slug || kind == Kind.Scorpion || kind == Kind.SnowWolf
            || kind == Kind.GoldenBeetle || kind == Kind.GiantFrog
            ? EnemyPathfindingAI.Mode.Chase : EnemyPathfindingAI.Mode.KeepDistance;
        ai.preferredDistance = kind == Kind.Healer ? 5f : kind == Kind.FloatingEye || kind == Kind.AshMage ? 5f : 6f;
        if (IsMicroboss) MobSounds.Play("microboss_entrada", transform.position, .6f);
        if (kind == Kind.Mimic || kind == Kind.Mole || kind == Kind.AncientTurret) { ai.mode = EnemyPathfindingAI.Mode.Hold; ai.speedMultiplier = 0; }
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
            bool hidden = (state & 2) != 0;
            var color = body.color; color.a = hidden ? 0f : 1f; body.color = color;
            // v1.4.1: sota terra no té cos. Abans el collider seguia actiu i es menjava les bales
            // («tirs fantasma»). Ara les bales passen i sols es pot colpejar quan ix (fase 2).
            if (moleColliders == null) moleColliders = GetComponentsInChildren<Collider2D>(true);
            foreach (var c in moleColliders) if (c != null && c.enabled == hidden) c.enabled = !hidden;
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
        if ((kind == Kind.Banshee || kind == Kind.SnowWolf) && (state & 64) != 0 && waveUntil <= Time.time)
        { waveUntil = Time.time + .45f; Fx.Burst(transform.position, new Color(.65f, .7f, 1f), 18, 5f); }
        if (kind == Kind.SnowWolf)
        {   // el líder es distingeix: una mica més gran i blavós
            bool leader = (state & 128) != 0;
            body.color = leader ? new Color(.72f, .85f, 1f) : Color.white;
            transform.localScale = Vector3.one * (leader ? 1.2f : 1f);
        }
        if (kind == Kind.AshMage) ShowOrbs((state & 4) != 0);
        if (kind == Kind.GoldenBeetle && (state & 8) != 0) body.transform.Rotate(0f, 0f, 720f * Time.deltaTime); // fet bola
        else if (kind == Kind.GoldenBeetle) body.transform.rotation = Quaternion.identity;
        if (kind == Kind.FloatingEye) { var c = body.color; c.a = (state & 2) != 0 ? .55f : 1f; body.color = c; } // tancat: apagat
        if (kind == Kind.Salamander) body.color = (state & 8) != 0 ? new Color(1f, .55f, .35f) : Color.white; // s'encén abans de calar foc
    }

    void Update()
    {
        if (health.IsNetProxy || health.IsDead) return;
        Transform target = NetCoop.NearestTarget(transform.position); if (target == null) return;
        if (!(kind == Kind.Scorpion && charging)) // l'Escorpí no gira mentre prepara la cua
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
            case Kind.Salamander: Salamander(target); break;
            case Kind.Slug: Slug(); break;
            case Kind.Scorpion: Scorpion(target); break;
            case Kind.SnowWolf: Wolf(target); break;
            case Kind.Bomber: Bomber(target); break;
            case Kind.AncientTurret: AncientTurret(target); break;
            case Kind.GoldenBeetle: Beetle(target); break;
            case Kind.FloatingEye: Eye(target); break;
            case Kind.GiantFrog: Frog(target); break;
            case Kind.AshMage: Mage(target); break;
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
        // Escorpí (v1.4.1): l'escut del davant para bales I colps; cal voltar-lo i colpejar-lo per darrere o pels costats
        if (kind == Kind.Scorpion)
        {
            if (direction.sqrMagnitude > .001f && Vector2.Dot(direction.normalized, facing) < -.15f)
            { MobSounds.Play("escut_resso", transform.position, .3f); return true; }
            return false;
        }
        // Mag de Cendra: les tres orbes paren els colps (3 impactes cada una); sense orbes queda vulnerable 4 s
        if (kind == Kind.AshMage)
        {
            if (orbsLeft <= 0) return false;
            MobSounds.Play("escut_resso", transform.position, .3f);
            if (++orbHits >= 3)
            {
                orbHits = 0; orbsLeft--; Fx.Burst(transform.position, new Color(1f, .5f, .15f), 10, 3f);
                if (orbsLeft == 0) orbRegenAt = Time.time + 4f;
            }
            return true;
        }
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
        // Granota gegant: un colp mentre prepara la llengua la talla
        if (kind == Kind.GiantFrog && phase == 1)
        { phase = 0; charging = false; ai.speedMultiplier = 1f; tongueAt = Time.time + 3f; Interruptions++; return; }
        // Escarabat daurat: atordit, deixa caure una moneda amb cada colp
        if (kind == Kind.GoldenBeetle && phase == 2 && !health.IsNetProxy) { CoinDrop.Spawn(transform.position); return; }
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

    // ------------------------------------------------------------------ v1.4.1: sis enemics més

    int phase, orbsLeft = 3, orbHits; Vector2 lastDrop = new Vector2(9999f, 9999f); float chargeUntil, fireClock, orbRegenAt, nextTick, tongueAt = 3f, jumpAt = 6f, summonAt = 8f, beamStart; Vector2 jumpFrom;
    static readonly List<EnemyV140> wolves = new List<EnemyV140>();
    void OnEnable() { if (kind == Kind.SnowWolf) wolves.Add(this); }
    void OnDisable() { wolves.Remove(this); }

    // El líder de la manada és el llop viu amb l'id més baix; si el mates, ningú udola i la càrrega conjunta s'acaba
    bool IsWolfLeader
    {
        get
        {
            if (kind != Kind.SnowWolf || health == null || health.IsDead) return false;
            foreach (var w in wolves) if (w != null && w != this && w.health != null && !w.health.IsDead && w.GetInstanceID() < GetInstanceID()) return false;
            return true;
        }
    }

    // ------------------------------------------------------------------ v1.4.1: quatre microbosses

    static string MicrobossName(Kind k) =>
        k == Kind.GoldenBeetle ? "Escarabat Daurat" : k == Kind.FloatingEye ? "Ull Flotant" : k == Kind.GiantFrog ? "Granota Gegant" : "Mag de Cendra";
    static Color MicrobossColor(Kind k) =>
        k == Kind.GoldenBeetle ? new Color(.95f, .75f, .2f) : k == Kind.FloatingEye ? new Color(.75f, .35f, 1f) : k == Kind.GiantFrog ? new Color(.4f, .85f, .3f) : new Color(1f, .45f, .15f);

    void MoveTo(Vector2 p) { transform.position = p; var body2d = GetComponent<Rigidbody2D>(); if (body2d != null) body2d.position = p; }

    // Escarabat Daurat: persegueix 2 s, es fa bola i rebota per les parets 3 s (invulnerable, 2 de mal),
    // i queda atordit 2 s (vulnerable, i deixa monedes a cada colp).
    void Beetle(Transform target)
    {
        switch (phase)
        {
            case 0:
                if (timer > 0) break;
                phase = 1; timer = 3f; charging = true; ai.enabled = false; aim = ((Vector2)target.position - (Vector2)transform.position).normalized; Actions++;
                break;
            case 1:
            {
                Vector2 step = aim * 5.5f * Time.deltaTime, here = transform.position;
                if (!TileNav.CircleFree(here + new Vector2(step.x, 0f), .4f)) aim.x = -aim.x;
                if (!TileNav.CircleFree(here + new Vector2(0f, step.y), .4f)) aim.y = -aim.y;
                step = aim * 5.5f * Time.deltaTime;
                if (TileNav.CircleFree(here + step, .4f)) MoveTo(here + step);
                if (Time.time >= nextTick)
                    foreach (var person in Targets())
                        if (Vector2.Distance(person.position, transform.position) < .75f) { nextTick = Time.time + .8f; NetCoop.DamagePlayer(person, 2); }
                if (timer <= 0) { phase = 2; timer = 2f; charging = false; ai.enabled = true; ai.speedMultiplier = 0; MobSounds.Play("boss_fase2", transform.position, .25f); }
                break;
            }
            default:
                if (timer <= 0) { phase = 0; timer = 2f; ai.speedMultiplier = 1f; }
                break;
        }
    }

    // Ull Flotant: tancat és immune; obri l'ull i dispara un feix lent giratori (90° en 2,5 s), i en eixe moment és vulnerable.
    void Eye(Transform target)
    {
        if (minion != null && Time.time >= summonAt) { summonAt = Time.time + 15f; SummonMinions(2); }
        if (phase == 0)
        {
            ai.speedMultiplier = 1f;
            if (timer > 0) return;
            phase = 1; timer = 2.5f; channeling = true; ai.speedMultiplier = 0; Actions++;
            Vector2 toward = (Vector2)target.position - (Vector2)transform.position;
            beamStart = Mathf.Atan2(toward.y, toward.x) * Mathf.Rad2Deg - 45f;
            MobSounds.Play("franctirador_carrega", transform.position, .4f);
        }
        float angle = (beamStart + 90f * (1f - Mathf.Clamp01(timer / 2.5f))) * Mathf.Deg2Rad;
        Vector2 from = transform.position;
        aim = from + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 7f;
        if (Time.time >= nextTick)
        {
            nextTick = Time.time + .4f;
            foreach (var person in Targets()) if (DistanceToSegment(person.position, from, aim) < .4f) NetCoop.DamagePlayer(person, 1);
        }
        if (timer <= 0) { phase = 0; timer = 3f; channeling = false; }
    }

    void SummonMinions(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * 1.2f;
            var dungeon = DungeonGenerator.Instance;
            if (dungeon != null) { if (!dungeon.TryChildSpawn(health, pos, out pos)) continue; }
            else if (TileNav.Ground != null && TileNav.IsBlockedWorld(pos)) pos = transform.position;
            var child = Instantiate(minion, pos, Quaternion.identity);
            var childHealth = child.GetComponent<EnemyHealth>();
            if (dungeon != null) dungeon.RegisterSpawned(childHealth);
            child.GetComponent<EnemyPathfindingAI>()?.Invoke(nameof(EnemyPathfindingAI.Alert), .05f);
        }
        Fx.Burst(transform.position, new Color(.75f, .35f, 1f), 10, 3f);
    }

    // Granota Gegant: llengua que t'estira (es talla si la colpeges) i salt amb esquitxada.
    void Frog(Transform target)
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if (phase == 0)
        {
            if (Time.time >= tongueAt && distance < 6f && TileNav.LineOfSight(transform.position, target.position, 0f, true))
            { phase = 1; timer = .6f; aim = target.position; charging = true; ai.speedMultiplier = 0; }
            else if (Time.time >= jumpAt && distance < 9f)
            {
                phase = 3; timer = .5f; jumpFrom = transform.position; aim = target.position; ai.enabled = false;
                if (!TileNav.CircleFree(aim, .4f)) aim = jumpFrom;
            }
        }
        else if (phase == 1)
        {
            aim = target.position; // la llengua segueix el jugador fins al final de l'avís
            if (timer > 0) return;
            charging = false; phase = 0; tongueAt = Time.time + 5f; ai.speedMultiplier = 1f; Actions++;
            MobSounds.Play("escorpi_cua", transform.position, .4f);
            foreach (var person in Targets())
            {
                if (Vector2.Distance(person.position, transform.position) > 6.5f || !TileNav.LineOfSight(transform.position, person.position, 0f, true)) continue;
                NetCoop.DamagePlayer(person, 2);
                NetCoop.PushPlayer(person, ((Vector2)transform.position - (Vector2)person.position).normalized, Mathf.Max(0f, Vector2.Distance(person.position, transform.position) - 1.2f));
            }
        }
        else if (phase == 3)
        {
            float t = 1f - Mathf.Clamp01(timer / .5f);
            MoveTo(Vector2.Lerp(jumpFrom, aim, t));
            if (timer > 0) return;
            phase = 0; jumpAt = Time.time + 7f; ai.enabled = true; Actions++;
            Fx.Burst(transform.position, new Color(.4f, .85f, .3f), 14, 4f); Fx.Shake(.2f, .15f);
            foreach (var person in Targets()) if (Vector2.Distance(person.position, transform.position) <= 1.5f) NetCoop.DamagePlayer(person, 2);
        }
    }

    // Mag de Cendra: tres orbes de foc paren els colps; sense orbes és vulnerable 4 s i després les regenera. Ventall de 3.
    void Mage(Transform target)
    {
        if (orbsLeft <= 0 && Time.time >= orbRegenAt) { orbsLeft = 3; orbHits = 0; Fx.Burst(transform.position, new Color(1f, .5f, .15f), 12, 3f); }
        if (timer > 0) return;
        timer = 2.5f; Actions++;
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        for (int i = -1; i <= 1; i++)
        {
            Vector2 d = Quaternion.Euler(0, 0, 15f * i) * direction;
            EnemyBullet.Fire((Vector2)transform.position + d * .5f, d, 5.5f, 1, transform, 1f, new Color(1f, .5f, .15f));
        }
    }

    SpriteRenderer[] orbSprites;
    void ShowOrbs(bool show)
    {
        if (orbSprites == null)
        {
            if (!show) return;
            orbSprites = new SpriteRenderer[3];
            for (int i = 0; i < 3; i++)
            { orbSprites[i] = Fx.MakeSprite("Orba de foc", transform.position, Fx.Disc, new Color(1f, .5f, .15f), "Player", 9); orbSprites[i].transform.SetParent(transform, true); orbSprites[i].transform.localScale = Vector3.one * .35f; }
        }
        for (int i = 0; i < 3; i++)
        {
            if (orbSprites[i] == null) continue;
            orbSprites[i].enabled = show;
            float a = Time.time * 2.2f + i * Mathf.PI * 2f / 3f;
            orbSprites[i].transform.position = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * .85f;
        }
    }

    // Salamandra: colp i fugida. Corre cap a tu, s'atura, encén un cercle de foc i fuig.
    void Salamander(Transform target)
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if (phase == 0 && distance < 2f)
        { phase = 1; timer = .5f; ai.speedMultiplier = 0; charging = true; }
        else if (phase == 1 && timer <= 0)
        {
            HazardZone.Spawn(HazardZone.Style.Fire, transform.position); Actions++;
            phase = 2; timer = 2.5f; charging = false;
            ai.mode = EnemyPathfindingAI.Mode.KeepDistance; ai.preferredDistance = 9f; ai.speedMultiplier = 1.6f;
        }
        else if (phase == 2 && timer <= 0)
        { phase = 0; ai.mode = EnemyPathfindingAI.Mode.Chase; ai.speedMultiplier = 1f; }
    }

    // Llimac àcid: lent, deixa tolls d'àcid que alenteixen. En morir es parteix (EnemySplitter).
    void Slug()
    {
        if (timer > 0 || Vector2.Distance(transform.position, lastDrop) < .8f) return;
        timer = .4f; lastDrop = transform.position; Actions++;
        HazardZone.Spawn(HazardZone.Style.Acid, transform.position);
    }

    // Escorpí: escut frontal i cua en línia recta amb avís de 0,6 s.
    void Scorpion(Transform target)
    {
        if (charging)
        {
            if (timer > 0) return;
            charging = false; timer = 2.5f; ai.speedMultiplier = 1f; Actions++;
            MobSounds.Play("escorpi_cua", transform.position, .45f);
            Vector2 from = transform.position;
            foreach (var person in Targets())
                if (DistanceToSegment(person.position, from, aim) < .45f) NetCoop.DamagePlayer(person, 2);
            Fx.Burst(aim, new Color(1f, .6f, .1f), 8, 2.5f);
            return;
        }
        if (timer > 0) return;
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < 3.2f && TileNav.LineOfSight(transform.position, target.position, 0f, true))
        { charging = true; timer = .6f; ai.speedMultiplier = 0; aim = (Vector2)transform.position + facing * 3.2f; }
    }

    static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a; float t = ab.sqrMagnitude < .0001f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
    }

    // Llop de neu: el líder udola cada 6 s i els tres carreguen alhora.
    void Wolf(Transform target)
    {
        if (chargeUntil > 0f && Time.time >= chargeUntil) { chargeUntil = 0f; ai.speedMultiplier = 1f; }
        if (!IsWolfLeader || timer > 0 || chargeUntil > 0f) return;
        if (Vector2.Distance(transform.position, target.position) > 12f) return;
        timer = 6f; Actions++; waveUntil = Time.time + .45f;
        MobSounds.Play("llop_udol", transform.position, .5f);
        foreach (var w in wolves) if (w != null && w.health != null && !w.health.IsDead) w.StartCharge();
    }

    void StartCharge() { chargeUntil = Time.time + 1.2f; ai.speedMultiplier = 2.5f; ai.Alert(); }

    // Bombardier: bomba amb cercle roig 1,2 s abans; també fa mal als altres enemics.
    void Bomber(Transform target)
    {
        if (timer > 0) return;
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > 9f || !TileNav.LineOfSight(transform.position, target.position, 0f, true)) return;
        timer = 3f; Actions++;
        MobSounds.Play("bombardier_metxa", transform.position, .4f);
        HazardZone.Spawn(HazardZone.Style.Bomb, target.position);
    }

    // Torreta antiga: fixa, dispara només en línies de fila i columna i canvia d'eix cada 3 s.
    void AncientTurret(Transform target)
    {
        if (timer <= 0) { timer = 3f; phase ^= 1; MobSounds.Play("torreta_servo", transform.position, .3f); }
        fireClock -= Time.deltaTime;
        if (fireClock > 0 || Vector2.Distance(transform.position, target.position) > 12f) return;
        fireClock = .7f;
        Vector2 axis = phase == 0 ? Vector2.right : Vector2.up;
        for (int i = -1; i <= 1; i += 2)
            EnemyBullet.Fire((Vector2)transform.position + axis * i * .4f, axis * i, 5.5f, 1, transform, 1f, new Color(1f, .8f, .25f));
        Actions++;
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
        if ((kind != Kind.Mimic && !IsMicroboss) || health.IsNetProxy) return;
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        var candidates = new List<WeaponData>();
        // Mímic: una rareza més que el cofre; microbosses: sempre una arma rara (el «cofre rar» del disseny)
        ChestRarity reward = IsMicroboss ? ChestRarity.Rar : (ChestRarity)Mathf.Min(3, (int)rewardRarity + 1);
        foreach (var w in catalog.weapons) if (w != null && w.rarity == reward) candidates.Add(w);
        if (candidates.Count > 0) ItemPickup.Spawn(candidates[Random.Range(0, candidates.Count)], transform.position, transform.position + Vector3.down);
    }

    void OnDestroy()
    {
        if (kind == Kind.Mimic) TileNav.RemoveBlocker(chestPosition);
        if (beam != null && beam.material != null) Destroy(beam.material);
    }
}
