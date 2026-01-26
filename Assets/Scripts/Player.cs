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

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        //bool canMove = !Physics.Raycast(transform.position, moveDir, playerSize);
        float playerHeight = 2f;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            // 尝试只在x轴上移动
            Vector3 moveDirX = new Vector3(playerMovement.x, 0f, 0f);
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else // 尝试只在z轴上移动
            {
                Vector3 moveDirZ = new Vector3(0f, 0f, playerMovement.y);
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDir * moveDistance;
        }

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
