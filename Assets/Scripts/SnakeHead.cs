using m1m1c_3DAstarPathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SnakeHead : SnakeSegment
{
    public SnakeSegment snakeSegmentPrefab;
    public bool isActive { get; set; }

    public UnityEvent<Node> SnakeDeathEvent = new UnityEvent<Node>();

    [SerializeField] private int StartingSegments = 3;

    private Vector3Int stepDirection = new Vector3Int(1, 0, 0);

    private Node pathGoalNode;
    private Node queuedGoalNode;
    private Node startNode;

    private List<Node> path = new List<Node>();
    private List<int> savedWalkPenalties = new List<int>();

    private bool isWaitingForPath = false;

    private SnakeSegment head;
    private SnakeSegment tail;

    private int nextPathNodeIndex = 0;


    public override void Setup(Node startNode)
    {
        base.Setup(startNode);
        this.startNode = startNode;
        SetHead(this);
        SetTail(this);

        for (int i = 0; i < StartingSegments; i++)
        {
            AddSnakeSegment();
        }

        arrivalActions = new List<System.Action>();
        arrivalActions.Add(StopListeningToInbetweenNodes);
        arrivalActions.Add(IncreasePathNodeIndex);

        FruitSpawner.FruitSpawnEvent.AddListener(ChangePath);

        StartCoroutine(ActivationTimer());
    }

    private void Update()
    {
        if (!isActive) { return; }
        if (IsMoving) { return; }

        //Move to next node in path
        if (CurrentNode != pathGoalNode && path.Count > 0)
        {      

            TargetNode = path[nextPathNodeIndex];        

            var followUpIndex = nextPathNodeIndex + 1;
            if (followUpIndex < path.Count) 
            { FollowingNode = path[followUpIndex]; }
            else 
            { FollowingNode = null; }

            if (savedWalkPenalties.Count > 0)
            {
                var savedWalkPenalty = savedWalkPenalties[nextPathNodeIndex];
                if (TargetNode.WalkPenalty > savedWalkPenalty)
                {
                    UpdatePath();
                }
            }

        }
        else
        {
            if (path.Count == 0 && pathGoalNode != null && isWaitingForPath == false)
            {
                isWaitingForPath = true;
                PathRequestManager.RequestPath(transform.position, pathGoalNode.WorldPosition, OnPathFound);
            }
            else if (CurrentNode == pathGoalNode || pathGoalNode == null)
            {
                ClearPath();

                if (!CheckForQueuedGoal())
                {
                    pathGoalNode = NavVolume.NavVolumeInstance.GetRandomNode();
                }
            }
            return;
        }


        if (TargetNode == null)
        {
            TargetNode = NavVolume.NavVolumeInstance.GetNodeFromIndex(CurrentNode.volumeCoordinate + stepDirection);
            if (TargetNode == null) { return; }
        }

        StartMoving();
        SetStepDirection(CurrentNode.volumeCoordinate, TargetNode.volumeCoordinate);
    }

    public void OnPathFound(List<Node> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            savedWalkPenalties = new List<int>();

            foreach (var node in path)
            {
                node.NodeWalkableDisabled.AddListener(this.UpdatePath);
                savedWalkPenalties.Add(node.WalkPenalty);
            }

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

    public void AddSnakeSegment()
    {
        var spawnNode = tail.CurrentNode;
        spawnNode.ChangeWalkableState(false);
        var tempSegment = Instantiate(snakeSegmentPrefab, spawnNode.WorldPosition, Quaternion.identity, transform.parent);
        tempSegment.Setup(spawnNode, moveSpeed);
        tail.FollowingSegment = tempSegment;
        SetTail(tempSegment);
    }

    private bool CheckForQueuedGoal()
    {
        var retval = false;
        if (queuedGoalNode != null)
        {
            pathGoalNode = queuedGoalNode;
            retval = true;
        }
        return retval;
    }
    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(ActivateCollider());
        isActive = true;
    }

    private IEnumerator ActivateCollider()
    {
        yield return new WaitForSeconds(0.5f);
        var col=GetComponent<Collider>();
        col.enabled = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == FollowingSegment) { return; }
        if (other.CompareTag("Fruit")) { return; }

        isActive = false;
        ResetWalkPenaltyInAdjacent();
        DisableSegment();
        SnakeDeathEvent.Invoke(startNode);
        Destroy(transform.parent.gameObject);

    }

    private void QueueFruitNodeAsGoal(Node fruitNode)
    {
        queuedGoalNode = fruitNode;
    }

    private void UpdatePath()
    {      
        ChangePath(queuedGoalNode);
    }

    private void ChangePath(Node newFruitNode)
    {
        StopCoroutine(MoveToTarget(nodeOccupationActions));
        QueueFruitNodeAsGoal(newFruitNode);
        ClearPath();
        pathGoalNode = null;
        isWaitingForPath = false;
    }

    private void StopListeningToInbetweenNodes()
    {
        if (CurrentNode != null)
        {
            CurrentNode.NodeWalkableDisabled.RemoveListener(this.UpdatePath);
        }

        if (TargetNode != null)
        {
            TargetNode.NodeWalkableDisabled.RemoveListener(this.UpdatePath);
        }

    }

    private void ClearPath()
    {
        foreach (var node in path)
        {
            node.NodeWalkableDisabled.RemoveListener(this.UpdatePath);
        }
        path = new List<Node>();
    }

    private void IncreasePathNodeIndex()
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
