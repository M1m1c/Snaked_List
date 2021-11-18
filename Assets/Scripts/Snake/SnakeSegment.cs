using m1m1c_3DAstarPathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegment : MonoBehaviour
{
    public SegmentType MySegmentType { get; set; }
    public SnakeSegment FollowingSegment { get; set; }
    public Node TargetNode { get; set; }
    public Node FollowingNode { get; set; }
    public Node CurrentNode { get; protected set; }
    public bool IsMoving { get; protected set; }

    protected Action[] nodeOccupationActions = new Action[2];

    protected List<Action> arrivalActions;

    protected Renderer myRenderer;

    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected int walkPenaltyNearBody = 10;

    public enum SegmentType
    {
        None,
        Head,
        Tail
    }

    public virtual void Setup(Node startNode, Color matColor)
    {
        CurrentNode = startNode;
        myRenderer = GetComponent<Renderer>();
        myRenderer.material.color = matColor;
    }

    //Used by non head segments to make sure they can keep up with the head
    public virtual void Setup(Node startNode, Color matColor, float speed)
    {
        CurrentNode = startNode;
        moveSpeed = speed + 1f;
        myRenderer = GetComponent<Renderer>();
        myRenderer.material.color = matColor;
    }

    public void DisableSegment()
    {
        StopCoroutine(MoveToTarget(nodeOccupationActions));

        if (CurrentNode != null) { CurrentNode.ChangeWalkableState(true); }

        if (TargetNode != null) { TargetNode.ChangeWalkableState(true); }

        if (FollowingSegment) { FollowingSegment.DisableSegment(); }
    }

    //Initiates the transition between two nodes and makes sure the following segment also moves
    public void StartMoving()
    {
        if (IsMoving) { return; }
        IsMoving = true;

        StopCoroutine(MoveToTarget(nodeOccupationActions));

        DetemineNodeOccupationActions();

        StartCoroutine(MoveToTarget(nodeOccupationActions));

        StartMovingFollowingSegment();
    }

    // used for setting up if there are any specific actions
    // that this segment needs to call in the nodes it traverses.
    // Mainly used by head and tail segments to make nodes walkable,
    // unwalkable or affect their walk penalty.
    private void DetemineNodeOccupationActions()
    {
        if (MySegmentType == SegmentType.None && nodeOccupationActions.Length == 0) { return; }

        //edge case for if the snake only has a head and no other segments
        if (MySegmentType.HasFlag(SegmentType.Head) && MySegmentType.HasFlag(SegmentType.Tail))
        {
            if (nodeOccupationActions.Length < 4) { nodeOccupationActions = new Action[4]; }

            nodeOccupationActions[0] = () => TargetNode.ChangeWalkableState(false);
            nodeOccupationActions[1] = () => CurrentNode.ChangeWalkableState(true);

            nodeOccupationActions[2] = () =>
            TargetNode.InvokeWalkPenaltiesEvent(walkPenaltyNearBody, FollowingNode);
            nodeOccupationActions[3] = () =>
            CurrentNode.InvokeWalkPenaltiesEvent(-walkPenaltyNearBody, null);
        }
        else
        {
            if (nodeOccupationActions.Length > 2) { nodeOccupationActions = new Action[2]; }

            if (MySegmentType == SegmentType.Head)
            {
                nodeOccupationActions[0] = () => TargetNode.ChangeWalkableState(false);
                nodeOccupationActions[1] = () =>
                TargetNode.InvokeWalkPenaltiesEvent(walkPenaltyNearBody, FollowingNode);
            }
            else if (MySegmentType == SegmentType.Tail)
            {
                nodeOccupationActions[0] = () => CurrentNode.ChangeWalkableState(true);
                nodeOccupationActions[1] = () =>
                CurrentNode.InvokeWalkPenaltiesEvent(-walkPenaltyNearBody, null);
            }
            else
            {
                nodeOccupationActions = new Action[0];
            }
        }
    }

    //Moves this segment between two positions(nodes) over time.
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

    //Initiates movment in the next segment connected to this, 
    //if it is not occupying the same node as this.
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