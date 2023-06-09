using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    [SerializeField] float time_to_move = 0.0001f;
    Rigidbody2D rigid_body;

    private bool is_moving;
    private Vector3 orig_pos, target_pos;

    void Start()
    {
        rigid_body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W) && !is_moving) {
            StartCoroutine(MovePlayer(Vector3.up/2));
        }
        if (Input.GetKey(KeyCode.S) && !is_moving) {
            StartCoroutine(MovePlayer(Vector3.down/2));
        }
        if (Input.GetKey(KeyCode.A) && !is_moving) {
            StartCoroutine(MovePlayer(Vector3.left/2));
        }
        if (Input.GetKey(KeyCode.D) && !is_moving) {
            StartCoroutine(MovePlayer(Vector3.right/2));
        }
    }

    private IEnumerator MovePlayer(Vector3 direction) {
        is_moving = true;
        float elapsed_time = 0;
        orig_pos = transform.position;
        target_pos = orig_pos + direction;

        while (elapsed_time < time_to_move) {
            transform.position = Vector3.Lerp(orig_pos, target_pos, elapsed_time/time_to_move);
            elapsed_time += Time.deltaTime;
            yield return null;
        }
        transform.position = target_pos;

        is_moving = false;
    }
    
}
