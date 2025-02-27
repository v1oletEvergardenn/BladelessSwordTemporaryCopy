using UnityEngine;

namespace Mobsoft.PixelStyleWaterShader
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        private Rigidbody2D rb;
        private Animator animator;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(moveX, moveY).normalized;
            rb.velocity = movement * moveSpeed;

            // Update animator parameters based on movement
            bool isMoving = movement.magnitude > 0.1f;
            animator.SetBool("IsWalking", isMoving);

            // Flip the player sprite based on movement direction
            if (movement.x != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(movement.x), 1, 1);
            }
        }
    }
}