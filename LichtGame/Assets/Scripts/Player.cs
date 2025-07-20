using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Player")]
    private Animator animator;
    public int maxHealth = 5;
    private int currentHealth;
    private Transform playerTransform;

    [Header("Movement")]
    public Rigidbody2D rb;
    public float xMoveSpeed = 15f;
    public float yMoveSpeed = 15f;
    float xMovement;
    float yMovement;
    private bool isFacingRight = true;

    [Header("Dashing")]
    public float dashSpeed = 40f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private Vector2 dashDirection;
    bool isDashing;
    bool canDash = true;
    TrailRenderer trailRenderer;

    [Header("Attack")]

    [Header("Damage Taken")]
    public float invincibilityDuration = 1.5f;
    public bool isInvincible;
    private SpriteRenderer spriteRenderer;

    [Header("SFX")]
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip playerHurtSFX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isInvincible = false;

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(xMovement * xMoveSpeed, yMovement * yMoveSpeed);

        Flip();
    }

    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isMoving", true);

        if (context.canceled)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("LastInputX", xMovement);
            animator.SetFloat("LastInputY", yMovement);
        }

        xMovement = context.ReadValue<Vector2>().x;
        animator.SetFloat("InputX", xMovement);

        yMovement = context.ReadValue<Vector2>().y;
        animator.SetFloat("InputY", yMovement);
    }

    private void Flip()
    {
        if (isFacingRight && xMovement < 0 || !isFacingRight && xMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = playerTransform.localScale;
            ls.x *= -1f;
            playerTransform.localScale = ls;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;
        isInvincible = true;
        trailRenderer.emitting = true;

        // SoundManager.instance.PlaySFXClip(dashSFX, 4f);

        dashDirection = new Vector2(xMovement, yMovement).normalized;

        rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, dashDirection.y * dashSpeed);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        trailRenderer.emitting = false;
        isInvincible = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }

    public void TakeDamage()
    {
        if (!isInvincible)
        {
            StartCoroutine(TakeDamageRoutine());
        }
    }

    public IEnumerator TakeDamageRoutine()
    {
        isInvincible = true;
        currentHealth -= 1;
        // SoundManager.instance.PlaySFXClip(playerHurtSFX, 1f);
        StartCoroutine(FlashPlayer());

        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;

        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;

            // SoundManager.instance.PlaySFXClip(deathSFX, 4f);
        }
    }

    private IEnumerator FlashPlayer()
    {
        float elapsed = 0;
        float flashInterval = 0.1f;

        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        spriteRenderer.enabled = true;
    }

    public void stopMoving()
    {
        playerTransform.position = Vector3.zero;
        isInvincible = true;
        rb.linearVelocity = Vector2.zero;
    }
}