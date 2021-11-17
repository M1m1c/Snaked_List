using m1m1c_3DAstarPathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SnakeSegment : MonoBehaviour
{

    public bool IsMoving { get; protected set; }
    public Node TargetNode { get; set; }
    public Node FollowingNode { get; set; }
    public Node CurrentNode { get; protected set; }
    public SegmentType MySegmentType { get; set; }
    public SnakeSegment FollowingSegment { get; set; }

    protected Action[] nodeOccupationActions = new Action[2];

    protected List<Action> arrivalActions;

    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected int walkPenaltyNearBody = 10;

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

    public void ResetWalkPenaltyInAdjacent()
    {
        if (CurrentNode != null)
        {
            CurrentNode.WalkPenalty = 0;
        }

        if (TargetNode != null)
        {
            TargetNode.WalkPenalty = 0;
        }

        if (FollowingSegment) { FollowingSegment.ResetWalkPenaltyInAdjacent(); }
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
            if (nodeOccupationActions.Length < 4) { nodeOccupationActions = new Action[4]; }

            nodeOccupationActions[0] = () => TargetNode.ChangeWalkableState(false);
            nodeOccupationActions[1] = () => CurrentNode.ChangeWalkableState(true);

            nodeOccupationActions[2] = () =>
            TargetNode.ChangeAdjacentWalkPenaltyExludingFollowing(walkPenaltyNearBody, FollowingNode);
            nodeOccupationActions[3] = () =>
            CurrentNode.ChangeAdjacentWalkPenaltyExludingFollowing(-walkPenaltyNearBody, null);
        }
        else
        {
            if (nodeOccupationActions.Length > 2) { nodeOccupationActions = new Action[2]; }

            if (MySegmentType == SegmentType.Head)
            {
                nodeOccupationActions[0] = () => TargetNode.ChangeWalkableState(false);
                nodeOccupationActions[1] = () =>
                TargetNode.ChangeAdjacentWalkPenaltyExludingFollowing(walkPenaltyNearBody, FollowingNode);
            }
            else if (MySegmentType == SegmentType.Tail)
            {
                nodeOccupationActions[0] = () => CurrentNode.ChangeWalkableState(true);
                nodeOccupationActions[1] = () =>
                CurrentNode.ChangeAdjacentWalkPenaltyExludingFollowing(-walkPenaltyNearBody, null);
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

        transform.LookAt(TargetNode.WorldPosition, Vector3.up);

        while (TargetNode != null)
        {

            var dist = Vector3.Distance(transform.position, TargetNode.WorldPosition);
            if (Mathf.Approximately(dist, 0f))
            {

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