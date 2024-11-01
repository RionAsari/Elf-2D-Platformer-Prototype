using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackedLightGrunt : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    public HealthbarBehaviour healthbar; 
    public float followDistance = 3f; 
    public float followSpeed = 3f; 
    public float attackRange = 1.5f; 
    public float enemyDetectRange = 5f; 
    public float attackCooldown = 2f; 
    public float damage = 10f; 

    private Animator animator;
    private Transform player; 
    private Transform currentTarget; 
    private float lastAttackTime; 
    private Vector3 originalScale; // Tambahkan variabel untuk menyimpan skala asli

    private Collider2D playerCollider; 
    private Collider2D enemyCollider; 

    private void Start()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform; 

        playerCollider = player.GetComponent<Collider2D>();
        enemyCollider = GetComponent<Collider2D>();

        if (playerCollider != null && enemyCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, enemyCollider);
        }

        if (healthbar != null)
        {
            healthbar.SetHealth(health, maxHealth);
        }

        lastAttackTime = Time.time;

        // Simpan skala asli sprite
        originalScale = transform.localScale;
    }

    private void Update()
    {
        UpdateAnimation();
        DetectEnemyOrFollowPlayer(); 
        UpdateHealthBar();
    }

    private void UpdateAnimation()
    {
        animator.SetBool("isWalking", false); 
    }

    private void UpdateHealthBar()
    {
        if (healthbar != null)
        {
            healthbar.UpdatePosition(transform.position);
        }
    }

    private void DetectEnemyOrFollowPlayer()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] middleBots = GameObject.FindGameObjectsWithTag("MiddleBot");
        
        float nearestDistance = Mathf.Infinity;
        currentTarget = null;

        foreach (GameObject target in targets)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget < nearestDistance && distanceToTarget <= enemyDetectRange)
            {
                nearestDistance = distanceToTarget;
                currentTarget = target.transform;
            }
        }

        foreach (GameObject middleBot in middleBots)
        {
            float distanceToMiddleBot = Vector3.Distance(transform.position, middleBot.transform.position);
            if (distanceToMiddleBot < nearestDistance && distanceToMiddleBot <= enemyDetectRange)
            {
                nearestDistance = distanceToMiddleBot;
                currentTarget = middleBot.transform;
            }
        }

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance <= attackRange)
            {
                AttackTarget();
            }
            else
            {
                ChaseTarget();
            }
        }
        else
        {
            FollowPlayer();
        }
    }

    private void FollowPlayer()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > followDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;

                // Flip sprite berdasarkan arah gerakan tanpa mengubah skala
                if (direction.x > 0) // Jika player ada di kanan
                {
                    transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Balik ke kanan
                }
                else if (direction.x < 0) // Jika player ada di kiri
                {
                    transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Tetap menghadap kiri
                }

                transform.position += direction * followSpeed * Time.deltaTime;

                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
    }

    private void ChaseTarget()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;

            // Flip sprite berdasarkan arah gerakan tanpa mengubah skala
            if (direction.x > 0) // Jika target ada di kanan
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Balik ke kanan
            }
            else if (direction.x < 0) // Jika target ada di kiri
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Tetap menghadap kiri
            }

            transform.position += direction * followSpeed * Time.deltaTime;

            animator.SetBool("isWalking", true);
        }
    }

    private void AttackTarget()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("isAttacking");

            // Flip sprite berdasarkan posisi target tanpa mengubah skala
            if (currentTarget != null)
            {
                if (currentTarget.position.x > transform.position.x)
                {
                    transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Balik ke kanan
                }
                else if (currentTarget.position.x < transform.position.x)
                {
                    transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);  // Tetap menghadap kiri
                }
            }

            // Hanya menyerang jika target bertag "Enemy" atau "MiddleBot"
            if (currentTarget != null)
            {
                if (currentTarget.CompareTag("Enemy"))
                {
                    LightGrunt enemyScript = currentTarget.GetComponent<LightGrunt>();
                    if (enemyScript != null)
                    {
                        enemyScript.TakeDamage(damage); 
                    }
                }
                else if (currentTarget.CompareTag("MiddleBot"))
                {
                    MiddleBot middleBotScript = currentTarget.GetComponent<MiddleBot>();
                    if (middleBotScript != null)
                    {
                        middleBotScript.TakeDamage(damage);
                    }
                }
            }

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health < 0) health = 0;

        if (healthbar != null)
        {
            healthbar.SetHealth(health, maxHealth);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetTrigger("isDead");
        Destroy(gameObject, 0.2f); 
    }

    public void ReceiveEnemyAttack(float damage)
    {
        TakeDamage(damage);
    }
}