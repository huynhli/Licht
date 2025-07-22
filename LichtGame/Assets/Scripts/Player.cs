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
    private Rigidbody2D rb;
    [SerializeField] private float xMoveSpeed;
    [SerializeField] private float yMoveSpeed;
    private float xMovementLast = 0f;
    private float yMovementLast = 0f;
    float xMovement;
    float yMovement;

    [Header("Dashing")]
    public bool dashAbilityGet = false;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    private Vector2 dashDirection;
    bool isDashing;
    bool canDash = true;
    TrailRenderer trailRenderer;

    [Header("Attack")]
    [SerializeField] private BaseWeapon swordWeapon;
    public bool canLeftAttack = true;
    private int comboIndex = 0; 
    private Coroutine performingLeftAttackCoroutine;

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
    }

    // Movement //
    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isMoving", true);

        if (context.canceled)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("LastInputX", xMovement);
            animator.SetFloat("LastInputY", yMovement);
            xMovementLast = xMovement;
            yMovementLast = yMovement;
        }

        xMovement = context.ReadValue<Vector2>().x;
        animator.SetFloat("InputX", xMovement);

        yMovement = context.ReadValue<Vector2>().y;
        animator.SetFloat("InputY", yMovement);
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
        spriteRenderer.color = new Color(0.528f, 0.528f, 0.528f, 1f);

        // SoundManager.instance.PlaySFXClip(dashSFX, 4f);
        if (xMovement != 0 || yMovement != 0)
        {
            dashDirection = new Vector2(xMovement, yMovement).normalized;

        }
        else if (xMovementLast != 0 || yMovementLast != 0)
        {
            dashDirection = new Vector2(xMovementLast, yMovementLast).normalized;
        }
        else
        {
            dashDirection = Vector2.right;
        }
        

        rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, dashDirection.y * dashSpeed);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        trailRenderer.emitting = false;
        isInvincible = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }

    // Damage //
    public void LeftAttack(InputAction.CallbackContext context)
    {
        if (context.performed && canLeftAttack)
        {
            Debug.Log("left attacking");
            // if in combo, cancel coroutine and start another (next attack), or just start the first one
            if (performingLeftAttackCoroutine != null)
            {
                StopCoroutine(performingLeftAttackCoroutine);
            }
            performingLeftAttackCoroutine = StartCoroutine(performLeftAttack());
        }
    }

    private IEnumerator performLeftAttack()
    {
        StartCoroutine(leftAtkCooldown());
        comboIndex += 1;
        if (comboIndex == 1)
        {
            animator.SetInteger("LeftAttackCombo", comboIndex);
            swordWeapon.LeftAttack1();
        }
        else if (comboIndex == 2)
        {
            animator.SetInteger("LeftAttackCombo", comboIndex);
            swordWeapon.LeftAttack2();
            
        }

        // if haven't clicked left click before timer finishes, reset to zero state

        // TODO if you spam click, you stay in here bc it never resets after the waitfor seconds 
        yield return new WaitForSeconds(0.58f);
        comboIndex = 0;
        animator.SetInteger("LeftAttackCombo", comboIndex);
    }

    private IEnumerator leftAtkCooldown()
    {
        canLeftAttack = false;
        yield return new WaitForSeconds(0.3f);
        canLeftAttack = true;
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