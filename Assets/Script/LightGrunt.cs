using System.Collections;
using UnityEngine;

public class LightGrunt : MonoBehaviour
{
    public float health = 100f; // Current health
    public float maxHealth = 100f; // Maximum health
    public float moveSpeed = 2f;
    public float attackRange = 5f; 
    public float damageAmount = 10f; 
    public float detectionRange = 10f; 
    public float allyDuration = 10f; 
    public Transform playerTransform; 
    public LayerMask enemyLayer; 
    public float allyAttackRange = 7f; 
    public float hackDistance = 2f; 

    public float knockbackForce = 5f; // Added variable for knockback force

    public bool isDisabled = false; 
    public bool isHackable = true; 
    public bool isAlly = false; 
    public bool isHacked = false; 

    public HealthbarBehaviour healthbar; 

    private Rigidbody2D rb; 
    private Animator animator; 
    private bool alreadyAttacked = false; 
    private SpriteRenderer spriteRenderer; 
    private Coroutine patrolCoroutine; 

    private void Start()
    {
        health = maxHealth; // Initialize health to maxHealth

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Check if required components are found
        if (rb == null) Debug.LogError("Rigidbody2D is missing!", this);
        if (animator == null) Debug.LogError("Animator is missing!", this);
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer is missing!", this);

        if (healthbar != null)
        {
            healthbar.SetHealth(health, maxHealth); 
        }

        patrolCoroutine = StartCoroutine(Patrol());
    }

    private void Update()
    {
        if (health <= 0)
        {
            return; 
        }

        if (healthbar != null)
        {
            healthbar.UpdatePosition(transform.position);
        }

        // Control behavior based on states
        if (isHacked)
        {
            FollowPlayer();
            DetectAndAttackEnemies(); // Hacked enemies attack other enemies
        }
        else if (isAlly)
        {
            DetectAndAttackEnemies(); // Allies can also attack enemies
        }
        else if (!isDisabled)
        {
            DetectAndAttackPlayer();
        }

        // Check for manual hacking input
        if (Input.GetKeyDown(KeyCode.F) && isHackable && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= hackDistance)
        {
            HackEnemy();
        }

        // Handle Idle and Walking animation
        animator.SetBool("isWalking", rb.velocity.magnitude > 0);
    }

    private IEnumerator Patrol()
    {
        while (!isAlly && !isDisabled && !isHacked)
        {
            // Move to the right
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            spriteRenderer.flipX = true; // Face left (the opposite of movement)

            yield return new WaitForSeconds(2f);

            // Move to the left
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            spriteRenderer.flipX = false; // Face right (the opposite of movement)

            yield return new WaitForSeconds(2f);
        }
    }

    private void DetectAndAttackPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < detectionRange && distanceToPlayer > attackRange)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
            spriteRenderer.flipX = direction.x > 0; // Face right if moving right
        }
        else if (distanceToPlayer <= attackRange && !alreadyAttacked)
        {
            StartCoroutine(AttackPlayer());
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop when not detecting player
        }
    }

    private void DetectAndAttackEnemies()
    {
        // Only attack enemies if isHacked or isAlly
        if (!isHacked && !isAlly) return;

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, allyAttackRange, enemyLayer);

        if (enemiesInRange.Length > 0)
        {
            Transform target = null;
            float closestDistance = float.MaxValue;

            foreach (var enemyCollider in enemiesInRange)
            {
                // Target only non-hacked enemies
                LightGrunt enemyScript = enemyCollider.GetComponent<LightGrunt>();
                if (enemyScript != null && enemyScript.health > 0 && !enemyScript.isHacked)
                {
                    float distanceToEnemy = Vector2.Distance(transform.position, enemyCollider.transform.position);
                    if (distanceToEnemy < closestDistance)
                    {
                        closestDistance = distanceToEnemy;
                        target = enemyCollider.transform;
                    }
                }
            }

            if (target != null)
            {
                Vector2 directionToTarget = (target.position - transform.position).normalized;
                rb.velocity = new Vector2(directionToTarget.x * moveSpeed, rb.velocity.y);
                spriteRenderer.flipX = directionToTarget.x > 0; // Face right if moving right

                if (closestDistance <= attackRange && !alreadyAttacked)
                {
                    StartCoroutine(AttackEnemy(target));
                }
            }
        }
    }

    private IEnumerator AttackPlayer()
    {
        alreadyAttacked = true;
        animator.SetTrigger("isAttacking");
        if (playerTransform != null)
        {
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(25); // Deal 25 damage to the player

                // Apply knockback effect to the player
                Vector2 knockbackDirection = (playerTransform.position - transform.position).normalized; // Direction from enemy to player
                Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse); // Apply knockback force
                }
            }
        }
        yield return new WaitForSeconds(1f);
        alreadyAttacked = false;
    }

    private IEnumerator AttackEnemy(Transform enemy)
    {
        alreadyAttacked = true;
        animator.SetTrigger("isAttacking");

        LightGrunt enemyScript = enemy.GetComponent<LightGrunt>();
        if (enemyScript != null && !enemyScript.isAlly) 
        {
            enemyScript.TakeDamage(damageAmount);
        }

        yield return new WaitForSeconds(1f);
        alreadyAttacked = false;
    }

    private void FollowPlayer()
    {
        if (playerTransform == null) return; // Ensure playerTransform is assigned

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Check if the player is within a certain distance to follow
        if (distanceToPlayer > 1f)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
            spriteRenderer.flipX = direction.x > 0; // Face right if moving right
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop moving if the player is out of range
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
        rb.velocity = Vector2.zero;
        Destroy(gameObject, 2f); // Destroy enemy after 2 seconds
    }

    public void HackEnemy()
    {
        isHackable = false; 
        isAlly = true; 
        isHacked = true; 
        health = maxHealth; // Reset health when hacked
        rb.velocity = Vector2.zero; 
        gameObject.tag = "Ally"; 

        animator.SetTrigger("isHacked");
        StartCoroutine(DestroyHackedEnemy());
    }

    private IEnumerator DestroyHackedEnemy()
    {
        yield return new WaitForSeconds(allyDuration);
        Die(); 
    }

    // Method to disable the enemy when hit by a special arrow
    public IEnumerator DisableEnemy(float duration)
    {
        isDisabled = true; 
        rb.velocity = Vector2.zero; // Stop movement
        rb.isKinematic = true; // Prevent physics interactions
        animator.SetTrigger("isDisabled"); 

        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine); // Stop patrolling if the enemy is disabled
        }

        yield return new WaitForSeconds(duration);
        isDisabled = false; 
        rb.isKinematic = false; // Resume physics interactions
        animator.SetTrigger("isActive"); 
        patrolCoroutine = StartCoroutine(Patrol()); // Resume patrolling
    }
}
