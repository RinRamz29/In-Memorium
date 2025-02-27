using UnityEngine;

public class PowerUps : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float dashForce = 15f;
    public float dashDuration = 0.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool hasDoubleJump; 
    private bool hasDash; 
    private bool isDashing;
    private float dashTimeLeft;
    private bool canDoubleJump; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hasDoubleJump = false; 
        hasDash = false; 
        canDoubleJump = false; 
    }

    void Update()
    {
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        
        if (isGrounded)
        {
            canDoubleJump = true; 
        }

        
        float moveInput = Input.GetAxis("Horizontal");
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = true; 
            }
            else if (hasDoubleJump && canDoubleJump)
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = false; 
            }
        }

        
        if (Input.GetKeyDown(KeyCode.LeftShift) && hasDash && !isDashing)
        {
            if (moveInput > 0) 
            {
                StartDash(Vector2.right);
            }
            else if (moveInput < 0) 
            {
                StartDash(Vector2.left);
            }
        }

       
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
            }
        }
    }

    void StartDash(Vector2 direction)
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        rb.linearVelocity = direction * dashForce;
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DobleSalto"))
        {
            hasDoubleJump = true; 
            Destroy(collision.gameObject); 
        }
        else if (collision.CompareTag("Dash"))
        {
            hasDash = true; 
            Destroy(collision.gameObject); 
        }
    }
}
