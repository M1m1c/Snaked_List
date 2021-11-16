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
        moveSpeed = speed + 1f;
    }

    public void DisableSegment()
    {
        StopCoroutine(MoveToTarget(nodeOccupationActions));

        if (CurrentNode != null) { CurrentNode.ChangeWalkableState(true); }
        if (TargetNode != null) { TargetNode.ChangeWalkableState(true); }

        if (FollowingSegment) { FollowingSegment.DisableSegment(); }
    }

    public void StartMoving()
    {
        if (IsMoving) { return; }
        IsMoving = true;

        StopCoroutine(MoveToTarget(nodeOccupationActions));

        DetemineNodeOccupationActions();

        StartCoroutine(MoveToTarget(nodeOccupationActions));

        StartMovingFollowingSegment();
    }


    private void DetemineNodeOccupationActions()
    {
        if (MySegmentType == SegmentType.None && nodeOccupationActions.Length == 0) { return; }

        if (MySegmentType.HasFlag(SegmentType.Head) && MySegmentType.HasFlag(SegmentType.Tail))
        {
            if (nodeOccupationActions.Length < 2) { nodeOccupationActions = new Action[2]; }

            nodeOccupationActions[0] = () => TargetNode.ChangeWalkableState(false);
            nodeOccupationActions[1] = () => CurrentNode.ChangeWalkableState(true);
        }
        else
        {
            if (nodeOccupationActions.Length > 1) { nodeOccupationActions = new Action[1]; }

            if (MySegmentType == SegmentType.Head)
            {
                nodeOccupationActions[0] = () => TargetNode.ChangeWalkableState(false);
            }
            else if (MySegmentType == SegmentType.Tail)
            {
                nodeOccupationActions[0] = () => CurrentNode.ChangeWalkableState(true);
            }
            else
            {
                nodeOccupationActions = new Action[0];
            }
        }
    }

    protected IEnumerator MoveToTarget(Action[] occupationActionsToCall)
    {
        if (TargetNode == null)
        {
            IsMoving = false;
            yield break;
        }

        transform.LookAt(TargetNode.WorldPosition,Vector3.up);

        while (TargetNode != null)
        {

            var dist = Vector3.Distance(transform.position, TargetNode.WorldPosition);
            if (Mathf.Approximately(dist, 0f))
            {

                //TODO update the walk penalty in adjacent nodes when a segment is next to them,
                //check when moving if the walk penalty has increased when moving toa new node, change path if it has, 
                //because that means there is a snake nearby.

                if (arrivalActions != null && arrivalActions.Count > 0)
                {
                    foreach (var action in arrivalActions)
                    {
                        if (action != null) { action(); }
                    }
                }

                if (occupationActionsToCall != null && occupationActionsToCall.Length > 0)
                {
                    foreach (var action in occupationActionsToCall)
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

    private void StartMovingFollowingSegment()
    {
        if (FollowingSegment)
        {
            if (CurrentNode != FollowingSegment.CurrentNode)
            {
                if (FollowingSegment.IsMoving == false)
                {
                    FollowingSegment.TargetNode = CurrentNode;

                    FollowingSegment.StartMoving();
                }
            }
        }
    }
}