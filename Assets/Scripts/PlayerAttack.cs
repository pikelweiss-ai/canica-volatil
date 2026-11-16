using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Atac")]
    public int damage = 1;              // Mal que fa cada colp
    public float attackRange = 0.8f;    // Distància del centre del jugador al centre del colp
    public float attackRadius = 0.6f;   // Mida de la zona del colp
    public float attackCooldown = 0.4f; // Temps mínim entre colps (segons)
    public float knockbackForce = 6f;   // Com de lluny empenta l'enemic

    [Header("Efectes (opcionals)")]
    public CameraFollow cameraFollow;   // Per a fer un xicotet tremolor en encertar
    public GameObject slashEffectPrefab; // Si poses un prefab, apareix on pegues

    private Vector2 facingDirection = Vector2.down; // Cap a on mira el jugador
    private float nextAttackTime = 0f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Guardem l'última direcció en què s'ha mogut el jugador
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input != Vector2.zero)
        {
            facingDirection = input.normalized;
        }

        bool attackPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);

        if (attackPressed && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        Vector2 attackPoint = (Vector2)transform.position + facingDirection * attackRange;

        if (slashEffectPrefab != null)
        {
            Instantiate(slashEffectPrefab, attackPoint, Quaternion.identity);
        }

        StartCoroutine(AttackSquash());

        // Busquem tots els colliders dins del cercle del colp
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, attackRadius);
        bool hitSomething = false;

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null)
                continue;

            Vector2 knockDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
            enemy.TakeHit(damage, knockDir * knockbackForce);
            hitSomething = true;
        }

        if (hitSomething && cameraFollow != null)
        {
            cameraFollow.ShakeCamera(0.1f, 0.1f);
        }
    }

    // Xicoteta animació d'"empenta" del jugador en atacar
    IEnumerator AttackSquash()
    {
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = originalScale;
    }

    // Dibuixa la zona d'atac a la vista Scene (només a l'editor)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 dir = Application.isPlaying ? facingDirection : Vector2.down;
        Gizmos.DrawWireSphere((Vector2)transform.position + dir * attackRange, attackRadius);
    }
}
