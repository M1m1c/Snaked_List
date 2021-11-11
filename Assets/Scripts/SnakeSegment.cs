using m1m1c_3DAstarPathfinding;
using System;
using System.Collections;
using UnityEngine;
public class SnakeSegment : MonoBehaviour
{


    public bool IsMoving { get; protected set; }
    public Node TargetNode { get; set; }
    public Node CurrentNode { get; protected set; }

    public SnakeSegment FollowingSegment { get; set; }

    [SerializeField] protected float moveSpeed = 1f;


    public virtual void Setup(Node startNode)
    {
        CurrentNode = startNode;
    }

    public virtual void Setup(Node startNode, float speed)
    {
        CurrentNode = startNode;
        moveSpeed = speed+1f;
    }


    public void StartMoving(Action  arrivalAction)
    {
        if (IsMoving) { return; }
        IsMoving = true;
        StopCoroutine(MoveToTarget(arrivalAction));
        StartCoroutine(MoveToTarget(arrivalAction));

        if (FollowingSegment)
        {
            if (CurrentNode != FollowingSegment.CurrentNode)
            {
                if (FollowingSegment.IsMoving == false)
                {
                    FollowingSegment.TargetNode = CurrentNode;
                    FollowingSegment.StartMoving(null);
                }
            }
        }
    }

    protected IEnumerator MoveToTarget(Action arrivalAction)
    {
        if (TargetNode == null)
        {
            IsMoving = false;
            yield break;
        }

        while (TargetNode != null)
        {

            var dist = Vector3.Distance(transform.position, TargetNode.WorldPosition);
            if (Mathf.Approximately(dist, 0f))
            {
                if (arrivalAction != null) { arrivalAction(); }

                CurrentNode = TargetNode;
                TargetNode = null;
                IsMoving = false;
                yield break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                TargetNode.WorldPosition,
                moveSpeed * Time.deltaTime);

            yield return null;
        }        
    }
}