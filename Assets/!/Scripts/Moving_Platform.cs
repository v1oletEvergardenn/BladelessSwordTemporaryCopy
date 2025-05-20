using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


public enum PlatformType
{
    normalGround,
    hardwall_horizontal,
    hardwall_vertical,
    softwall
}

public class Moving_Platform : MonoBehaviour
{

    public PlatformType platformType;
    public Sprite normalGround_sprite;
    public Sprite hardwall_sprite;
    public Sprite softwall_sprite;
    private SpriteRenderer sprite;
    private BoxCollider2D col;
    public float speed = 30f;
    public Transform followingPoint;
    public List<Transform> points = new List<Transform>();
    private int destPoint = 0;
    private Transform currentTarget;
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        points  = followingPoint.GetComponentsInChildren<Transform>().Skip(1).ToList();
        switch (platformType)
        {
            case PlatformType.normalGround:
                sprite.sprite = normalGround_sprite;
                gameObject.layer = 7;
                gameObject.tag = "Untagged";
                break;
            case PlatformType.hardwall_horizontal:
                sprite.sprite = hardwall_sprite;
                gameObject.layer = 11;
                gameObject.tag = "hard_wall_horizontal";
                break;
            case PlatformType.hardwall_vertical:
                sprite.sprite = hardwall_sprite;
                gameObject.layer = 11;
                gameObject.tag = "hard_wall_vertical";
                break;
            case PlatformType.softwall:
                sprite.sprite = softwall_sprite;
                gameObject.layer = 10;
                gameObject.tag = "Untagged";
                break;
        }
        col.size = sprite.size;

        GotoNextPoint();
        
    }

    public void MoveToTargetPoint()
    {
        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
    }

    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Count == 0)
            return;

        // Set the agent to go to the currently selected destination.
        currentTarget = points[destPoint];

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Count;
    }

    // Update is called once per frame
    void Update()
    {
        MoveToTargetPoint();
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f)
            GotoNextPoint();
    }
}
