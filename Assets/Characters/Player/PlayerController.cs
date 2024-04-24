using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 1f;
    //public float collisionOffset = 0.01f;

    public Boolean win = false;
    public Boolean gameOver = false;

    private AudioManager audioManager;
    private int keys = 0;

    private Animator animator;

    private SpriteRenderer spriteRenderer;

    private ContactFilter2D contactFilter;
    Rigidbody2D rb;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    private Vector2 moveInput;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public GameObject winScreen;
    public GameObject gameOverScreen;


    private void FixedUpdate()
    {

        if (gameOver && !win && gameObject.active)
        {
            gameOverScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        if (win && !gameOver && gameObject.active)
        {
            winScreen.SetActive(true);
             gameObject.SetActive(false);
        }

        // if there isn't movement don't do anything
        if (moveInput == Vector2.zero)
        {
            animator.SetBool("isMoving", false);
            return;
        }

        animator.SetBool("isMoving", true);

        // try moving in all directions.
        if (!TryMove(moveInput))
        {
            if (!TryMove(new Vector2(moveInput.x, 0)))
            {
                TryMove(new Vector2(0, moveInput.y));
            }
        }

        animator.SetFloat("moveX", moveInput.x);
        animator.SetFloat("moveY", moveInput.y);
    }

    public void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    private bool TryMove(Vector2 position)
    {
        int count = rb.Cast(
                    moveInput,
                 contactFilter,
                 castCollisions,
                 moveSpeed * Time.fixedDeltaTime);

        if (count != 0) return false;

        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * moveInput);
        return true;
    }
    void OnMove(InputValue moveInput)
    {
        this.moveInput = moveInput.Get<Vector2>();
    }

    private Collider2D attackCollider;
    public void SwordAttack(AnimationEvent animationEvent)
    {

        string direction = animationEvent.animatorClipInfo.clip.name;

        switch (direction)
        {
            case "player_attack_top":
                attackCollider = GameObject.FindGameObjectWithTag("SwordAttackTop").GetComponent<Collider2D>();
                print("top");
                break;
            case "player_attack_bottom":
                attackCollider = GameObject.FindGameObjectWithTag("SwordAttackBottom").GetComponent<Collider2D>();
                print("bottom");
                break;

            case "player_attack_right":
                print("right");
                attackCollider = GameObject.FindGameObjectWithTag("SwordAttackRight").GetComponent<Collider2D>();
                break;
            case "player_attack_left":
                print("left");
                attackCollider = GameObject.FindGameObjectWithTag("SwordAttackLeft").GetComponent<Collider2D>();
                break;
        }

        if (attackCollider)
        {
            audioManager.PlaySFX(audioManager.sword);
            attackCollider.enabled = true;
        }

    }

    public void StopAttack()
    {
        print("stop");
        attackCollider.enabled = false;
    }

    void OnFire()
    {
        animator.SetTrigger("swordAttack");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key") && this.keys < 3)
        {
            ++this.keys;
            audioManager.PlaySFX(audioManager.key);
            Destroy(other.gameObject);


            if (this.keys == 3)
            {
                win = true;
            }
        }


    }

    public void ReplyGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
