using m1m1c_3DAstarPathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SnakeHead : SnakeSegment
{
    public SnakeSegment snakeSegmentPrefab;

    public UnityEvent<int> SnakeDeathEvent = new UnityEvent<int>();
    public bool isActive { get; set; }
    public int SnakeIndex { get; private set; }
    public ScoreKeeper MyScoreKeeper { get; set; }

    [SerializeField] private int StartingSegments = 3;

    private Vector3Int stepDirection = new Vector3Int(1, 0, 0);

    private Node pathGoalNode;
    private Node queuedGoalNode;

    private List<Node> path = new List<Node>();
    private List<int> savedWalkPenalties = new List<int>();

    private bool isWaitingForPath = false;

    private SnakeSegment head;
    private SnakeSegment tail;

    private Color myColor;

    private int nextPathNodeIndex = 0;


    public virtual void Setup(Node startNode, Color matColor, int index)
    {
        Setup(startNode, matColor);
        myColor = matColor;
        SnakeIndex = index;
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

    //Handles movement, path requesting and where to move next
    private void Update()
    {
        if (!isActive) { return; }

        //Update Lifetime
        if (MyScoreKeeper != null)
        {
            MyScoreKeeper.LifeTime += Time.deltaTime;
        }


        if (IsMoving) { return; }

        //Set next node in path to target node
        if (CurrentNode != pathGoalNode && path.Count > 0 && nextPathNodeIndex < path.Count)
        {
            ProgressAlongPath();
        }
        else
        {
            GetNewPathOrNewGoal();
            return;
        }

        //if no target node, keep moving in the same direction
        if (TargetNode == null)
        {
            TargetNode = NavVolume.NavVolumeInstance.GetNodeFromIndex(CurrentNode.volumeCoordinate + stepDirection);
            if (TargetNode == null) { return; }
        }

        StartMoving();
        SetStepDirection(CurrentNode.volumeCoordinate, TargetNode.volumeCoordinate);
    }

    //Set path on callback, if no valid path was found request a new one
    //Ave the values of the alkpenalties in the path, used later for checking if another snake is adjacent to path
    public void OnPathFound(List<Node> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            savedWalkPenalties = new List<int>();

            foreach (var node in path)
            {
                node.NodeWalkableDisabledEvent.AddListener(this.UpdatePath);
                savedWalkPenalties.Add(node.WalkPenalty);
            }

            nextPathNodeIndex = 0;
            isWaitingForPath = false;
        }
        else
        {
            pathGoalNode = NavVolume.NavVolumeInstance.GetRandomNode();
            isWaitingForPath = true;
            PathRequestManager.RequestPath(
                transform.position,
                pathGoalNode.WorldPosition,
                OnPathFound,
                this.gameObject);

        }
    }

    public void AddSnakeSegment()
    {
        var spawnNode = tail.CurrentNode;
        spawnNode.ChangeWalkableState(false);
        var tempSegment = Instantiate(snakeSegmentPrefab, spawnNode.WorldPosition, Quaternion.identity, transform.parent);
        tempSegment.Setup(spawnNode, myColor, moveSpeed);
        tail.FollowingSegment = tempSegment;
        SetTail(tempSegment);
    }

    public void KillSnake()
    {
        isActive = false;
        ResetWakPenaltiesAlongBody();
        DisableSegment();
        SnakeDeathEvent.Invoke(SnakeIndex);
        Destroy(transform.parent.gameObject);
    }

    //if the snake spawned after a fruit and missed its invoke, inform it that there is a fruit
    public void InformFruitExistsInLevel(Node fruitNode)
    {
        ChangePath(fruitNode);
    }

    //sets the target node to the next node in path,
    //stores node that comes after target node in path,
    //makes call for checking if target nodes walk penalty hase changed
    private void ProgressAlongPath()
    {
        TargetNode = path[nextPathNodeIndex];

        var followUpIndex = nextPathNodeIndex + 1;
        if (followUpIndex < path.Count)
        { FollowingNode = path[followUpIndex]; }
        else
        { FollowingNode = null; }

        CheckForWalkPenaltyChange();
    }

    //Requests a path if there is none,
    //requests a new goal node if there is none or the path ahs been completed
    private void GetNewPathOrNewGoal()
    {
        //Request new path
        if (path.Count == 0 && pathGoalNode != null && isWaitingForPath == false)
        {
            isWaitingForPath = true;
            PathRequestManager.RequestPath(
                transform.position,
                pathGoalNode.WorldPosition,
                OnPathFound,
                this.gameObject);
        }
        //Set new goal node to random if no fruit node is queued
        else if (CurrentNode == pathGoalNode || pathGoalNode == null)
        {
            ClearPath();

            if (!CheckForQueuedGoal())
            {
                pathGoalNode = NavVolume.NavVolumeInstance.GetRandomNode();
            }
        }
    }



    //if there is a queued goal, then set path goal node to it
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

    //if the target node has a higher walk penalty then when we generated the path,
    //then that means there is a snake nearby and we should update our path.
    private void CheckForWalkPenaltyChange()
    {
        if (savedWalkPenalties.Count > 0)
        {
            var savedWalkPenalty = savedWalkPenalties[nextPathNodeIndex];
            if (TargetNode.WalkPenalty > savedWalkPenalty)
            {
                UpdatePath();
            }
        }
    }

    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(ActivateCollider());
        isActive = true;
    }

    //used to stager the activation of teh collider, so snake does not immediately die
    private IEnumerator ActivateCollider()
    {
        yield return new WaitForSeconds(0.5f);
        var col = GetComponent<Collider>();
        col.enabled = true;
    }

    //Handles colliding with fruit or something that will kill the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == FollowingSegment) { return; }
        if (other.CompareTag("Fruit"))
        {
            if (MyScoreKeeper != null) { MyScoreKeeper.FruitsEaten++; }
            return;
        }

        KillSnake();

    }

    private void QueueFruitNodeAsGoal(Node fruitNode)
    {
        queuedGoalNode = fruitNode;
    }

    //Used to request an updated path if a node in the path becomes unwalkable,
    //becasue that means another snake has stepped onto our path.
    private void UpdatePath()
    {
        ChangePath(queuedGoalNode);
    }

    //stops movement, clears the path so a new one can be requested in the next update cycle
    private void ChangePath(Node newFruitNode)
    {
        StopCoroutine(MoveToTarget(nodeOccupationActions));
        QueueFruitNodeAsGoal(newFruitNode);
        ClearPath();
        pathGoalNode = null;
        isWaitingForPath = false;
    }

    //Reverts back the changes to walk penalties in the nodes adjacent to each segment of the snake
    private void ResetWakPenaltiesAlongBody()
    {
        for (int i = nextPathNodeIndex - 1; i >= 0; i--)
        {
            if (i >= path.Count || i < 0) { continue; }
            var node = path[i];
            node.InvokeWalkPenaltiesEvent(-walkPenaltyNearBody, null);

            if (node == tail.CurrentNode) { break; }
        }
    }

    //stops listening for evetns  in the nodes we have passed in our path
    private void StopListeningToInbetweenNodes()
    {
        if (CurrentNode != null)
        {
            CurrentNode.NodeWalkableDisabledEvent.RemoveListener(this.UpdatePath);
        }

        if (TargetNode != null)
        {
            TargetNode.NodeWalkableDisabledEvent.RemoveListener(this.UpdatePath);
        }

    }

    private void ClearPath()
    {
        foreach (var node in path)
        {
            node.NodeWalkableDisabledEvent.RemoveListener(this.UpdatePath);
        }
        path = new List<Node>();
    }

    private void IncreasePathNodeIndex()
    {
        nextPathNodeIndex++;
    }

    //used for determining which direction to walk if we don't have a path
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
}
