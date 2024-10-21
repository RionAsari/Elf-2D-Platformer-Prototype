using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f; // Increased speed
    public float jumpForce = 8f; // Increased jump force
    public float fallMultiplier = 2.5f; // Fall speed multiplier
    private Rigidbody2D rb;
    private bool isGrounded = true;
    private bool facingRight = true;
    private Camera mainCamera;
    private bool usingSpecialArrow = false;

    // Jump variables
    private int jumpCount = 0;
    public int maxJumpCount = 2; // Maximum jumps (1 for single jump, 2 for double jump)

    // Dash variables
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    private bool isDashing = false;
    private bool isInvincible = false;
    private float lastDashTime = -10f;

    private int dashDirection = 0; // -1 for left, 1 for right
    private float doubleTapTime = 0.2f;
    private KeyCode lastKeyPressed;
    private float lastKeyTime;

    // Reference to Animator and Bow Transform
    private Animator animator;
    public GameObject bowTransform; // Drag your bowTransform GameObject here in the inspector

    private float moveInput;

    // Player health
    public int maxHealth = 100;
    private int currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f; // Set gravity scale for faster falling
        mainCamera = Camera.main;

        // Get Animator component
        animator = GetComponent<Animator>();

        // Set player's current health to max health
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            usingSpecialArrow = !usingSpecialArrow; // Toggle between regular and special arrows
            Debug.Log("Using Special Arrow: " + usingSpecialArrow);
        }
        if (!isDashing)
        {
            Move();
            HandleJump();
            AimAtMouse();
            UpdateAnimation(); // Update animation
            CheckDash(); // Check for double tap input for dashing
        }
        else
        {
            // Stop any movement while dashing
            rb.velocity = new Vector2(dashDirection * dashSpeed, rb.velocity.y);
        }

        // Apply fall multiplier
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    // Move with A and D keys
    private void Move()
    {
        if (!isDashing) // Only allow movement if not dashing
        {
            moveInput = Input.GetAxisRaw("Horizontal"); // A = -1, D = 1

            Vector3 moveVelocity = new Vector3(moveInput * moveSpeed, rb.velocity.y, 0);
            rb.velocity = moveVelocity;

            // Flip character if moving in the opposite direction
            if ((moveInput > 0 && !facingRight) || (moveInput < 0 && facingRight))
            {
                Flip();
            }

            // Show or hide the bow based on movement
            if (!Input.GetMouseButton(0)) // Only hide when not charging
            {
                ShowBow(moveInput != 0); // Hide if not moving
            }
        }
    }

    // Function to show or hide the bow
    public void ShowBow(bool show)
    {
        bowTransform.SetActive(show);
    }

    // Function to check if the player is idle
    public bool IsIdle()
    {
        return moveInput == 0 && isGrounded; // Player is idle when not moving and is grounded
    }

    // Update the animator parameters
    private void UpdateAnimation()
    {
        // Update movement and grounded animations
        animator.SetBool("isMoving", moveInput != 0);
        animator.SetBool("isGrounded", isGrounded); // Update grounded state

        // Set jumping and falling states
        if (!isGrounded)
        {
            if (rb.velocity.y > 0)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            else
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
            }
        }
        else
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }

        // Update dash animation
        animator.SetBool("isDashing", isDashing);
    }

    // Aim at the mouse position
    private void AimAtMouse()
    {
        Vector3 mousePosition = GetMouseWorldPosition();

        // Flip character sprite to face the cursor
        if (mousePosition.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (mousePosition.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    // Get mouse position in world
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0; // Zero out Z-axis for 2D game
        return worldPosition;
    }

    // Flip the character
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Flip the character
        transform.localScale = scale;
    }

    // Handle jumping with W key
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isGrounded = false; // Set not grounded after jumping
                jumpCount = 1; // Reset jump count
                animator.SetBool("isJumping", true); // Update animator parameter
            }
            else if (jumpCount < maxJumpCount)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++;
                animator.SetBool("isJumping", true); // Update animator parameter for double jump
            }
        }
    }

    // Detect if the player is grounded
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0; // Reset jump count
            animator.SetBool("isGrounded", true); // Update animator parameter
        }

        // Handle collision with LightGrunt
        if (collision.gameObject.CompareTag("LightGrunt"))
        {
            // Take damage from LightGrunt
            TakeDamage(25);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // Update grounded state when leaving ground
            animator.SetBool("isGrounded", false);
        }
    }

    // Check for double tap input for dashing
    private void CheckDash()
    {
        if (Time.time - lastDashTime > dashCooldown)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (lastKeyPressed == KeyCode.D && Time.time - lastKeyTime < doubleTapTime)
                {
                    StartDash(1); // Dash to the right
                }
                else
                {
                    lastKeyPressed = KeyCode.D;
                    lastKeyTime = Time.time;
                }
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (lastKeyPressed == KeyCode.A && Time.time - lastKeyTime < doubleTapTime)
                {
                    StartDash(-1); // Dash to the left
                }
                else
                {
                    lastKeyPressed = KeyCode.A;
                    lastKeyTime = Time.time;
                }
            }
        }
    }

    // Start the dash movement
    private void StartDash(int direction)
    {
        dashDirection = direction;
        isDashing = true;
        isInvincible = true;
        animator.SetTrigger("StartDash"); // Trigger the dash animation
        StartCoroutine(DashCoroutine());
    }

    // Coroutine to handle dash duration and cooldown
    private IEnumerator DashCoroutine()
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            yield return null; // Wait until the dash is over
        }

        isDashing = false;
        isInvincible = false;
        lastDashTime = Time.time;
    }

    // Function to make the player invincible during the dash
    public bool IsInvincible()
    {
        return isInvincible;
    }

    // Function to take damage
    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;
            Debug.Log("Player Health: " + currentHealth);

            // Check if the player is dead
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    // Function to handle player death
    private void Die()
    {
        Debug.Log("Player has died.");
        // Implement death logic (e.g., respawn or game over)
    }
}
