using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapIndicater : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (player == null) { player = PlayerAttack.instance.gameObject; }
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 4, transform.position.z);
    }
}