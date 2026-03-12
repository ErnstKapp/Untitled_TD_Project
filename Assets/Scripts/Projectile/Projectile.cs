using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;

    private Enemy target;
    private HitInfo hitInfo;
    private float lifetimeTimer = 0f;

    /// <summary>
    /// Initialize with target and hit payload. Tower builds HitInfo from TowerData; Projectile just carries and delivers it.
    /// </summary>
    public void Initialize(Enemy targetEnemy, float projectileSpeed, HitInfo hit)
    {
        target = targetEnemy;
        speed = projectileSpeed;
        hitInfo = hit;
        lifetimeTimer = 0f;
    }

    private void Update()
    {
        lifetimeTimer += Time.deltaTime;

        if (lifetimeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < 0.2f)
            {
                HitTarget();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            target.ApplyHit(hitInfo);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null && enemy == target)
        {
            HitTarget();
        }
    }
}
