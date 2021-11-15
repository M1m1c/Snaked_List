using m1m1c_3DAstarPathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SnakeSegment : MonoBehaviour
{

    public bool IsMoving { get; protected set; }
    public Node TargetNode { get; set; }
    public Node CurrentNode { get; protected set; }
    public SegmentType MySegmentType { get; set; }
    public SnakeSegment FollowingSegment { get; set; }

    protected Action[] nodeOccupationActions = new Action[2];

    protected List<Action> arrivalActions;

    [SerializeField] protected float moveSpeed = 1f;

    public enum SegmentType
    {
        None,
        Head,
        Tail
    }

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
                if (actionsToCall != null && actionsToCall.Length > 0)
                {
                    foreach (var action in actionsToCall)
                    {
                        if (action != null) { action(); }
                    }
                }

                if(arrivalActions!=null && arrivalActions.Count > 0)
                {
                    foreach (var action in arrivalActions)
                    {
                        if (action != null) { action(); }
                    }
                }

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