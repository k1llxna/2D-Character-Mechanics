using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Player : MonoBehaviour {

    private int lives = 3;
    public int score = 0;
    public int hp = 6;

    public float atkDmg;
    public float speed;
    public float jumpForce;
    public float groundCheckRadius;

    private float moveValue;
    private float floatValue;
    private int dmgTaken;

    public bool dead = false;
    public bool grounded;

    private bool facingRight;
    private bool canMove = true;
    private bool canCrouch = true;
    private bool sucking = false;
    private bool suckTransform = false;

    private State currentState;

    public Rigidbody2D rb;
    public Transform groundCheck;
    public LayerMask isGroundLayer;

    Animator anim;

	void Start () {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = 1.0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        facingRight = true;

        anim = GetComponent<Animator>();

        currentState = State.Normal;
        form = AttackForm.None;

        CheckList();
    }

	void Update () {

        if (groundCheck)
        {
            grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, isGroundLayer);
        }

        if (canMove)
        {
            moveValue = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(moveValue * speed, rb.velocity.y); // Move
            Flip(moveValue);

            if (grounded) // Any action when on ground
            {   
                // Jump
                if (Input.GetButtonDown("Jump")) // Spacebar // Input.GetKeyDown(KeyCode.Space)
                {
                    StartCoroutine(Jump());
                }
                anim.SetBool("Float",false);
            }

            if (Input.GetButtonDown("Up"))
            {
                floatValue = Input.GetAxisRaw("Up");
                rb.AddForce(Vector2.up * jumpForce / 1.8f, ForceMode2D.Impulse);
                anim.SetBool("Float", true);
            }

            //if (Input.GetKeyDown(KeyCode.C))
            //    floatValue = 0.0f;
            //    anim.SetFloat("Float", Mathf.Abs(floatValue));

        } // Move Bracket    
        StartCoroutine(SuckCheck());
        CrouchCheck();

        if (anim)
        {
            switch (currentState)
            {
                case State.Normal:
                    anim.SetFloat("Movement", Mathf.Abs(moveValue));
                    break;

                case State.Full:
                    anim.SetFloat("Full Movement", Mathf.Abs(moveValue));
                    break;
            }
        }
    }

   
     IEnumerator Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        anim.SetBool("Jump", true);
        yield return new WaitForSeconds(1.1f);
        anim.SetBool("Jump", false);
    }

    IEnumerator SuckCheck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            canMove = false;
            canCrouch = false;
            sucking = true;
            anim.SetBool("Suck", true);
            yield return new WaitForSeconds(1);
            suckHitBox.SetActive(true);
            suckRadius.SetActive(true);
            anim.speed = 0.0f;
        }
        if (Input.GetMouseButtonUp(1))
        {
            suckHitBox.SetActive(false);
            suckRadius.SetActive(false);
            anim.speed = 1f;
            canMove = true;
            canCrouch = true;
            sucking = false;
            anim.SetBool("Suck", false);
        }
    }

    void CrouchCheck()
    {
        if (canCrouch)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
              //  rb.velocity = new Vector2(0, rb.velocity.y);  
             //   moveValue = 0.0f;
                canMove = false;
                anim.SetBool("Crouching", true);
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                anim.SetBool("Crouching", false);
              
                canMove = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D enemy)
    {
        if (enemy.gameObject.tag == "Enemy")
        {
            BaseEnemy mob = enemy.gameObject.GetComponent<BaseEnemy>();

            if (sucking == true)
            {
                mob.Die();
                currentState = State.Full;
                suckTransform = true;
                anim.SetBool("Suck", false);
                anim.SetBool("Full", true);
            }
            else
            {
                dmgTaken = mob.atkDmg;
                TakeDamage(dmgTaken);
                mob.Die();
            }
        }
    }

    public void TakeDamage(int dmgTaken)
    {
        anim.SetBool("Hit", true);
        hp -= dmgTaken;
        canMove = false;     

        if (hp > 0)
        {
            StartCoroutine(Hit());
        }
        else
        {
            lives -= 1;
            StartCoroutine(Die());       
        }   
    }

    IEnumerator Hit()
    {
        yield return new WaitForSeconds(2);
        anim.SetBool("Hit", false);
        currentState = State.Normal;
        canMove = true;
    }

    IEnumerator Die()
    {
        anim.SetBool("Dead", true);
        yield return new WaitForSeconds(3);
        transform.position = spawnPoint.transform.position;
        anim.SetBool("Hit", false);
        anim.SetBool("Dead", false);
        hp = 6;
        canMove = true;    
    }

    private void Flip(float moveValue) 
    {
        if (moveValue > 0 && !facingRight || moveValue < 0 && facingRight)
        {
            facingRight = !facingRight;
            Vector3 playerScale = transform.localScale;
            playerScale.x *= -1;
            transform.localScale = playerScale;
        }
    }

    public enum State
    {
        Normal,
        Full
    }

    public void CheckList()
    {
        if (speed < 0 || speed > 5.0f)
        {
            speed = 5.0f;
            Debug.LogWarning("Speed not set on " + name + ". Defaulting to " + speed);
        }

        if (jumpForce <= 0 || jumpForce > 10.0f)
        {
            jumpForce = 10.0f;
            Debug.LogWarning("JumpForce not set on " + name + ". Defaulting to " + jumpForce);
        }

        if (!groundCheck)
        {
            groundCheck = GameObject.Find("GroundCheck").GetComponent<Transform>(); // In hierarchy

        }
    }
}
