using TMPro;
using UnityEngine;

// v1.4.1: barra de vida flotant sobre els microbosses (24 px d'ample) amb el nom xicotet.
// Es crea a tots els equips; llig la vida de l'EnemyHealth (als clients la sincronitza l'amfitrió).
public class MicrobossBar : MonoBehaviour
{
    static Sprite square;
    static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
                tex.SetPixel(0, 0, Color.white); tex.Apply();
                square = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, .5f), 1f);
            }
            return square;
        }
    }

    EnemyHealth health;
    Transform root, fill;
    SpriteRenderer fillRenderer;
    float width = 1.5f, lastHealth;
    float flashUntil;
    Color barColor = new Color(.95f, .55f, .15f);

    public static void Attach(GameObject enemy, string displayName, Color color)
    {
        if (enemy.GetComponent<MicrobossBar>() != null) return;
        var bar = enemy.AddComponent<MicrobossBar>();
        bar.barColor = color; bar.Build(displayName);
    }

    void Build(string displayName)
    {
        health = GetComponent<EnemyHealth>();
        root = new GameObject("Barra microboss").transform;
        root.SetParent(transform, false);
        // Fora de l'escala de l'enemic i damunt del cap
        root.localPosition = new Vector3(0f, .95f, 0f);
        MakeBar("Fons", new Color(0f, 0f, 0f, .75f), width + .1f, .22f, -.05f, 20);
        fill = MakeBar("Vida", barColor, width, .14f, 0f, 21);
        fillRenderer = fill.GetComponent<SpriteRenderer>();
        MakeBar("Marca 50%", new Color(1f, 1f, 1f, .8f), .04f, .2f, width * .5f, 22);

        var label = new GameObject("Nom", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
        label.transform.SetParent(root, false); label.transform.localPosition = new Vector3(width * .5f, .24f, 0f);
        label.text = displayName; label.fontSize = 1.6f; label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white; label.rectTransform.sizeDelta = new Vector2(4f, .5f);
        label.GetComponent<MeshRenderer>().sortingLayerName = "PlayerName"; label.GetComponent<MeshRenderer>().sortingOrder = 23;
        lastHealth = health != null ? health.currentHealth : 0;
    }

    Transform MakeBar(string name, Color color, float barWidth, float height, float x, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer)); go.transform.SetParent(root, false);
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = Square; sr.color = color;
        sr.sortingLayerName = "PlayerName"; sr.sortingOrder = order;
        go.transform.localPosition = new Vector3(x, 0f, 0f);
        go.transform.localScale = new Vector3(barWidth, height, 1f);
        return go.transform;
    }

    void LateUpdate()
    {
        if (health == null || root == null) return;
        // La barra no gira ni s'escala amb l'enemic (el líder dels llops, per exemple)
        root.rotation = Quaternion.identity;
        var s = transform.lossyScale; root.localScale = new Vector3(1f / Mathf.Max(.01f, Mathf.Abs(s.x)), 1f / Mathf.Max(.01f, Mathf.Abs(s.y)), 1f);
        root.position = transform.position + Vector3.up * .95f - new Vector3(width * .5f, 0f, 0f);

        float fraction = health.maxHealth > 0 ? Mathf.Clamp01(health.currentHealth / (float)health.maxHealth) : 0f;
        fill.localScale = new Vector3(Mathf.Max(.001f, width * fraction), fill.localScale.y, 1f);
        if (health.currentHealth < lastHealth) flashUntil = Time.time + .12f; // blanca en rebre mal
        lastHealth = health.currentHealth;
        fillRenderer.color = Time.time < flashUntil ? Color.white : barColor;
        if (health.IsDead) Destroy(root.gameObject);
    }
}
