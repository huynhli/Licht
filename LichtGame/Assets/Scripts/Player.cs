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
    bool isDashing = false;
    bool canDash = false;
    TrailRenderer trailRenderer;

    [Header("Attack")]
    bool isAttacking = false;
    [SerializeField] private BaseWeapon swordWeapon;
    public bool canLeftAttack = true;
    private int comboIndex = 0;
    private Coroutine performingLeftAttackCoroutine;
    [SerializeField] private Camera worldCamera;
    private Vector2 lookDirection;

    [Header("Damage Taken")]
    public float invincibilityDuration = 1.5f;
    public bool isInvincible;
    private SpriteRenderer spriteRenderer;

    [Header("SFX")]
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip playerHurtSFX;

    [Header("Light Attack")]
    public float mana = 0f;
    public bool manaActive = false;

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
        if (isDashing || isAttacking) return;
        rb.linearVelocity = new Vector2(xMovement * xMoveSpeed, yMovement * yMoveSpeed);
    }

    // Look //
    public void Look(InputAction.CallbackContext context)
    {
        Vector2 mouseScreenPos = context.ReadValue<Vector2>();
        Vector2 worldMousePos = worldCamera.ScreenToWorldPoint(mouseScreenPos);
        lookDirection = worldMousePos - rb.position;
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
        animator.SetBool("isDashing", true);
        trailRenderer.emitting = true;
        spriteRenderer.color = new Color(0.528f, 0.528f, 0.528f, 1f);

        // SoundManager.instance.PlaySFXClip(dashSFX, 4f);

        // TODO change to vector instead of saving each individually
        if (xMovement != 0 || yMovement != 0)
        {
            dashDirection = new Vector2(xMovement, yMovement).normalized;
            animator.SetFloat("DashX", xMovement);
            animator.SetFloat("DashY", yMovement);
        }
        else if (xMovementLast != 0 || yMovementLast != 0)
        {
            dashDirection = new Vector2(xMovementLast, yMovementLast).normalized;
            animator.SetFloat("DashX", xMovementLast);
            animator.SetFloat("DashY", yMovementLast);
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
        animator.SetFloat("DashX", 0f);
        animator.SetFloat("DashY", 0f);
        animator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }

    // Damage //
    public void LeftAttack(InputAction.CallbackContext context)
    {
        if (comboIndex == 3) return;
        if (context.performed && canLeftAttack)
        {
            mana += 50f;
            if (mana >= 100f)
            {
                StartCoroutine(manaBarDeplete());
            }

            animator.SetFloat("LookX", lookDirection.x);
            animator.SetFloat("LookY", lookDirection.y);
            animator.SetFloat("LastInputX", lookDirection.x);
            animator.SetFloat("LastInputY", lookDirection.y);


            isAttacking = true;
            rb.linearVelocity = Vector2.zero;
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
        animator.SetInteger("LeftAttackCombo", comboIndex);
        switch (comboIndex)
        {
            case 1:
                swordWeapon.LeftAttack1();
                break;
            case 2:
                // TODO fix ending of anim it cuts abruptly
                swordWeapon.LeftAttack2();
                break;
            case 3:
                // TODO fix ending of anim it looks laggy
                swordWeapon.LeftAttack3();
                break;
        }

        // if haven't clicked left click before timer finishes, reset to zero state
        yield return new WaitForSeconds(0.58f);
        if (comboIndex == 3)
        {
            yield return new WaitForSeconds(0.7f);
        }
        comboIndex = 0;
        animator.SetInteger("LeftAttackCombo", comboIndex);
        isAttacking = false;
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

    public IEnumerator manaBarDeplete()
    {
        canDash = true;
        while (mana > 0f)
        {
            mana--;
            yield return new WaitForSeconds(0.1f);
        }

        mana = 0f;
        canDash = false;
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
            stopMoving();
            StartCoroutine(deathAnim());

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

    private IEnumerator deathAnim()
    {
        yield return new WaitForSeconds(1f);
    }
}