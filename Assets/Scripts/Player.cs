using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        transform.position += moveDir * Time.deltaTime * moveSpeed;

        if (moveDir != Vector3.zero)
        {
            transform.forward = moveDir;
        }
    }
}
