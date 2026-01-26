using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;

    private bool isWalking;

    // Update is called once per frame
    void Update()
    {
        Vector2 playerMovement = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(playerMovement.x, 0f, playerMovement.y);



        transform.position += moveDir * Time.deltaTime * moveSpeed;

        //if (moveDir != Vector3.zero)
        //{
        //    transform.forward = moveDir;
        //}
        isWalking = moveDir != Vector3.zero;
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}
