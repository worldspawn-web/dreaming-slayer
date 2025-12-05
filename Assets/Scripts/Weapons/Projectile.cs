using UnityEngine;
using DreamingSlayer.Enemies;

namespace DreamingSlayer.Weapons
{
    /// <summary>
    /// Basic projectile that moves in a direction and damages enemies
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private int pierceCount = 0; // 0 = no pierce, destroys on first hit

        [Header("Visual")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem hitEffect;

        private Vector2 direction;
        private float speed;
        private float damage;
        private int currentPierceCount;
        private bool initialized;

        public void Initialize(Vector2 dir, float spd, float dmg)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;
            currentPierceCount = pierceCount;
            initialized = true;

            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!initialized) return;
            
            // Move projectile
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if we hit an enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                // Deal damage
                enemy.TakeDamage(damage);

                // Spawn hit effect
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, transform.position, Quaternion.identity);
                }

                // Handle piercing
                if (destroyOnHit)
                {
                    if (currentPierceCount <= 0)
                    {
                        DestroyProjectile();
                    }
                    else
                    {
                        currentPierceCount--;
                    }
                }
            }
        }

        private void DestroyProjectile()
        {
            // Detach trail so it fades out nicely
            if (trail != null)
            {
                trail.transform.SetParent(null);
                Destroy(trail.gameObject, trail.time);
            }

            Destroy(gameObject);
        }
    }
}

