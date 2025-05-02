using UnityEngine;

namespace Mobsoft.PixelStyleWaterShader
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f; // Speed of the player movement

        private Rigidbody2D rb; // Reference to the Rigidbody2D component
        private Animator animator; // Reference to the Animator component

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component on the player GameObject
            animator = GetComponent<Animator>(); // Get the Animator component on the player GameObject
        }

        private void Update()
        {
            // Read input axes
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            // Calculate movement direction based on WASD keys
            Vector2 movement = new Vector2(moveX, moveY);

            // Normalize the movement vector so diagonal movement isn't faster
            if (movement.magnitude > 1f)
            {
                movement = movement.normalized;
            }

            // Move the player
            rb.velocity = movement * moveSpeed;

            // Update IsWalking parameter in Animator
            bool isWalking = movement.magnitude > 0f; // Check if player is moving
            animator.SetBool("IsWalking", isWalking);

            // Update Horizontal and Vertical parameters in Animator based on movement direction
            animator.SetFloat("Horizontal", moveX); // Set Horizontal based on X movement (-1 to 1)
            animator.SetFloat("Vertical", moveY); // Set Vertical based on Y movement (-1 to 1)
        }
    }
}