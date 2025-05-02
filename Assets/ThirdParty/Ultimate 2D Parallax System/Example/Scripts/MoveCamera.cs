using System;
using UnityEngine;

namespace KabreetGames.ParallaxSystem
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] private float speed = 1;

        [SerializeField] private MoveDirection direction = MoveDirection.Both;
        [SerializeField] private bool restrictNegativeY = true;

        private void FixedUpdate()
        {
            var mouseDir = Input.mousePosition - new Vector3(Screen.width, Screen.height, 0) / 2f;
            
            switch (direction)
            {
                case MoveDirection.Both:
                    mouseDir = new Vector3(mouseDir.x, mouseDir.y, 0);
                    break;
                case MoveDirection.Horizontal:
                    mouseDir = new Vector3(mouseDir.x, 0, 0);
                    break;
                case MoveDirection.Vertical:
                    mouseDir = new Vector3(0, mouseDir.y, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var speedM = Mathf.Lerp(0, speed, 2f * mouseDir.magnitude / Screen.width);
            transform.position += mouseDir.normalized * (Time.deltaTime * speedM);
            if (transform.position.y < 0 && restrictNegativeY)
            {
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            }
        }
        private enum MoveDirection
        {
            Both,
            Horizontal,
            Vertical,
        }
    }
    
    
}