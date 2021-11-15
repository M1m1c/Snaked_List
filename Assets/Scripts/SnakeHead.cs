using m1m1c_3DAstarPathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : SnakeSegment
{
    public SnakeSegment snakeSegmentPrefab;
    public bool isActive { get; set; }

    private Vector3Int stepDirection = new Vector3Int(1, 0, 0);

    private Node pathGoalNode;
    private Node queuedGoalNode;

    private List<Node> path = new List<Node>();

    private bool isWaitingForPath = false;

    private SnakeSegment head;
    private SnakeSegment tail;

    private int nextPathNodeIndex = 0;


    public override void Setup(Node startNode)
    {
        base.Setup(startNode);
        SetHead(this);
        SetTail(this);

        for (int i = 0; i < 30; i++)
        {
            AddSnakeSegment();
        }

        arrivalActions = new List<System.Action>();
        arrivalActions.Add(IncreaseTargetIndex);

        StartCoroutine(ActivationTimer());
    }



    // Update is called once per frame
    void Update()
    {
        if (!isActive) { return; }

        if (path.Count == 0 && pathGoalNode != null && isWaitingForPath == false)
        {
            isWaitingForPath = true;
            PathRequestManager.RequestPath(transform.position, pathGoalNode.WorldPosition, OnPathFound);
            return;
        }
        else if (CurrentNode != pathGoalNode && path.Count > 0)
        {
            TargetNode = path[nextPathNodeIndex];
        }
        else if (pathGoalNode == null)
        {
            //TODO get a node that we should move to
            pathGoalNode = NavVolume.NavVolumeInstance.GetRandomNode();
            return;
        }
        else if (CurrentNode == pathGoalNode)
        {
            pathGoalNode = null;
            path = new List<Node>();
            //nextPathNodeIndex = 0;
            return;
        }

        if (TargetNode == null)
        {
            TargetNode = NavVolume.NavVolumeInstance.GetNodeFromIndex(CurrentNode.volumeCoordinate + stepDirection);
            if (TargetNode == null) { return; }
        }

        if (!IsMoving)
        {
            StartMoving();
            SetStepDirection(CurrentNode.volumeCoordinate, TargetNode.volumeCoordinate);

        }
    }

    public void OnPathFound(List<Node> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            nextPathNodeIndex = 0;
            isWaitingForPath = false;
        }
        else
        {
            pathGoalNode = NavVolume.NavVolumeInstance.GetRandomNode();
            isWaitingForPath = true;
            PathRequestManager.RequestPath(transform.position, pathGoalNode.WorldPosition, OnPathFound);

        }
    }

    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(1f);

        isActive = true;
    }

    private void AddSnakeSegment()
    {
        var tempSegment = Instantiate(snakeSegmentPrefab, CurrentNode.WorldPosition, Quaternion.identity);
        tempSegment.Setup(CurrentNode, moveSpeed);
        tail.FollowingSegment = tempSegment;
        SetTail(tempSegment);
    }

    private void IncreaseTargetIndex()
    {
        nextPathNodeIndex++;
    }

    private void SetStepDirection(Vector3Int currentCoord, Vector3Int newCoord)
    {
        stepDirection = new Vector3Int(
            GetStepValue(currentCoord.x, newCoord.x),
            GetStepValue(currentCoord.y, newCoord.y),
            GetStepValue(currentCoord.z, newCoord.z));
    }

    private int GetStepValue(int myAxisPos, int otherAxisPos)
    {
        var stepSize = 0;
        if (myAxisPos > otherAxisPos)
        {
            stepSize = -1;
        }
        else if (myAxisPos < otherAxisPos)
        {
            stepSize = 1;
        }
        return stepSize;
    }

    private void SetHead(SnakeSegment segmentToUse)
    {
        if (head) { head.MySegmentType &= ~SegmentType.Head; }
        segmentToUse.MySegmentType |= SegmentType.Head;
        head = segmentToUse;
    }

    private void SetTail(SnakeSegment segmentToUse)
    {
        if (tail) { tail.MySegmentType &= ~SegmentType.Tail; }
        segmentToUse.MySegmentType |= SegmentType.Tail;
        tail = segmentToUse;
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = nextPathNodeIndex; i < path.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i].WorldPosition, new Vector3(.2f, .2f, .2f));

                if (i == nextPathNodeIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i].WorldPosition);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1].WorldPosition, path[i].WorldPosition);
                }
            }
        }
    }

}
