using System.Collections;
using UnityEngine;

// v1.4.0 fase 3: cada enemic nou fa el que diu el disseny (Mimic, Sanador, Escut-guerrer, Talp, Franctirador, Banshee).
public partial class NetBot
{
    IEnumerator Enemies140(Vector2 origin, Transform player, Rigidbody2D body, NetContentCatalog catalog)
    {
        GameObject Prefab(string id)
        {
            foreach (var e in catalog.enemies) if (e != null && e.name == id) return e;
            return null;
        }
        void Heal() { var hp = player.GetComponent<PlayerHealth>(); if (hp != null) hp.currentHealth = hp.maxHealth; }
        void Place(Vector2 at) { player.position = at; if (body != null) body.position = at; }

        foreach (string id in new[] { "Mimic", "Sanador", "EscutGuerrer", "Talp", "FranctiradorCristall", "Banshee" })
            if (Prefab(id) == null) { Fail("falta " + id + " al catàleg d'enemics"); yield break; }

        // --- Mimic: no fa res fins que t'acostes; llavors es desperta i mossega
        Place(origin + new Vector2(-1.5f, 0f));
        var mimicGo = Instantiate(Prefab("Mimic"), origin + new Vector2(4f, 0f), Quaternion.identity);
        yield return new WaitForSeconds(1.2f);
        var mimic = mimicGo.GetComponent<EnemyV140>();
        if (mimic.Actions != 0) { Fail("el Mimic ha atacat sense que t'acostes"); yield break; }
        Place(origin + new Vector2(3.4f, 0f));
        yield return WaitFor(() => mimic.Actions > 0, 3f, "el Mimic es desperta i mossega");
        if (failed) yield break;
        Heal(); Destroy(mimicGo); Ok("Mimic: cofre fals que mossega en acostar-t'hi");

        // --- Escut-guerrer: l'escut davanter para les bales; per darrere sí que li fas mal
        Place(origin + new Vector2(-1.5f, 0f));
        var shieldGo = Instantiate(Prefab("EscutGuerrer"), origin + new Vector2(3f, 0f), Quaternion.identity);
        var shieldHealth = shieldGo.GetComponent<EnemyHealth>();
        yield return new WaitForSeconds(.6f);
        Heal();
        int before = shieldHealth.currentHealth;
        shieldHealth.TakeProjectileHit(2, Vector2.right * 2f, ProjectileEffect.None); // la bala va cap a ell des del jugador: de front
        if (shieldHealth.currentHealth != before) { Fail("l'Escut-guerrer ha rebut una bala de front"); yield break; }
        shieldHealth.TakeProjectileHit(2, Vector2.left * 2f, ProjectileEffect.None); // per darrere
        if (shieldHealth.currentHealth >= before) { Fail("l'Escut-guerrer no rep bales per darrere"); yield break; }
        Destroy(shieldGo); Heal(); Ok("Escut-guerrer: para les bales de front i no per darrere");

        // --- Sanador: cura un aliat ferit que té a la vista
        Place(origin + new Vector2(-2f, 0f));
        var healerGo = Instantiate(Prefab("Sanador"), origin + new Vector2(3f, 0f), Quaternion.identity);
        var ally = new GameObject("Aliat ferit", typeof(BoxCollider2D), typeof(EnemyHealth));
        ally.transform.position = origin + new Vector2(3f, 1.5f);
        var allyHealth = ally.GetComponent<EnemyHealth>(); allyHealth.maxHealth = 10; allyHealth.currentHealth = 3;
        yield return WaitFor(() => allyHealth.currentHealth > 3, 5f, "el Sanador cura un aliat");
        if (failed) yield break;
        Destroy(healerGo); Destroy(ally); Heal(); Ok("Sanador: cura els aliats ferits");

        // --- Talp: s'enterra (invulnerable) i després ix
        Place(origin + new Vector2(-1.5f, 0f));
        var moleGo = Instantiate(Prefab("Talp"), origin + new Vector2(4f, 0f), Quaternion.identity);
        var mole = moleGo.GetComponent<EnemyV140>();
        bool wasHidden = false;
        for (float t = 0; t < 7f && mole.Actions == 0; t += .1f)
        {
            Heal();
            if (mole.Invulnerable) wasHidden = true;
            yield return new WaitForSeconds(.1f);
        }
        if (!wasHidden) { Fail("el Talp no s'ha enterrat"); yield break; }
        if (mole.Actions == 0) { Fail("el Talp no ha eixit"); yield break; }
        Destroy(moleGo); Heal(); Ok("Talp: s'enterra, és invulnerable i ix per sota");

        // --- Franctirador: carrega un tret i el mal el pot interrompre
        Place(origin + new Vector2(-1.5f, 0f)); // dins de la sala inicial (11x11), no a la paret
        var sniperGo = Instantiate(Prefab("FranctiradorCristall"), origin + new Vector2(4f, 0f), Quaternion.identity);
        var sniper = sniperGo.GetComponent<EnemyV140>();
        for (float t = 0; t < 5f && !sniper.Charging; t += .1f) { Heal(); yield return new WaitForSeconds(.1f); }
        if (!sniper.Charging) { Fail("el Franctirador no carrega cap tret"); yield break; }
        sniperGo.GetComponent<EnemyHealth>().TakeHit(1, Vector2.zero);
        if (sniper.Interruptions < 1) { Fail("el mal no interromp el Franctirador"); yield break; }
        Destroy(sniperGo); Heal(); Ok("Franctirador: carrega i el mal l'interromp");

        // --- Banshee: crida i fa mal
        Place(origin + new Vector2(-1.5f, 0f));
        var bansheeGo = Instantiate(Prefab("Banshee"), origin + new Vector2(1.5f, 0f), Quaternion.identity);
        var banshee = bansheeGo.GetComponent<EnemyV140>();
        yield return WaitFor(() => banshee.Actions > 0, 6f, "la Banshee crida");
        if (failed) yield break;
        Destroy(bansheeGo); Heal(); Ok("Banshee: crida i empeny");
    }
}
