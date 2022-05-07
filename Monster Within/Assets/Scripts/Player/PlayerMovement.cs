using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpCooldown;
    private float horizontalInput;


    [Header("SFX")]
    [SerializeField] private AudioClip jumpSound;

    // These variables go at the top with the others
    /* ---------------------------- */
    // how fast you want to dash
    public float dashSpeed;
    // how long you want to dash for
    public float dashTime;
    // cooldown for when dash ability is ready.
    private float dashCooldown;
    // resets the cooldown to specified time. higher numbers = longer CD
    public float resetDashCooldown;
    /* ------------------------------- */

    private void Awake()
    {
        //Grab references for rigidbody and animator from object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        //Flip player when moving left-right
        if (horizontalInput > 0.01f)
            transform.localScale = new Vector3(5, 5, 5); 
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-5, 5, 5);

        //Set animator parameters
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", isGrounded());

        //Wall jump logic
        if (wallJumpCooldown > 0.2f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            if (onWall() && !isGrounded())
            {
                body.gravityScale = 0;
                body.velocity = Vector2.zero;
            }
            else
                body.gravityScale = 4;

            if (Input.GetKey(KeyCode.Space))
                Jump();
        }
        else
            wallJumpCooldown += Time.deltaTime;


        /* ------------------------------- */
        // starts the dash cooldown timer
        dashCooldown -= Time.deltaTime;
        // stops the timer once the cooldown is ready
        if (dashCooldown < 0)
        {
            dashCooldown = -1;
        }
        else
        {
            dashCooldown -= Time.deltaTime;
        }

        //You can change KeyCode to whatever you like. currently on the NUMPAD 0 "zero" button
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (dashCooldown <= 0)
            {
                StartCoroutine(Dash());
            }
        }
        /* -------------------------------- */


    }

    /* -------------------------------- */
    IEnumerator Dash()
    {

        float startTime = Time.time;
        float localScaleX = transform.localScale.x;

        while (Time.time < startTime + dashTime)
        {

            float movementSpeed = dashSpeed * Time.deltaTime;

            if (Mathf.Sign(localScaleX) == 1)
            {
                transform.Translate(movementSpeed, 0, 0);
            }
            else
            {
                transform.Translate(-movementSpeed, 0, 0);
            }

            dashCooldown = resetDashCooldown;

            yield return null;
        }

    }
    /* --------------------------------- */

    private void Jump()
    {
        if (isGrounded())
        {
            SoundManager.instance.PlaySound(jumpSound);
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            anim.SetTrigger("jump");
        }
        else if (onWall() && !isGrounded())
        {
            if (horizontalInput == 0)
            {
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
                body.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 6);

            wallJumpCooldown = 0;
        }
    }


    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }
}