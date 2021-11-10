using m1m1c_3DAstarPathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : SnakeSegment
{
    public SnakeSegment snakeSegment;

    public bool isActive { get; set; } 

    private Vector3Int stepDirection = new Vector3Int(1, 0, 0);

    private Node pathGoalNode;

    private List<Node> path = new List<Node>();
    private LinkedList<SnakeSegment> segments = new LinkedList<SnakeSegment>();

    private bool isWaitingForPath = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Setup(Node startNode)
    {
        currentNode = startNode;
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
        else if (targetIndex < path.Count && path.Count > 0)
        {
            targetNode = path[targetIndex];
        }
        else if (pathGoalNode == null)
        {
            //TODO get a node that we should move to
            pathGoalNode = NavVolume.NavVolumeInstance.GetRandomNode();
            return;
        }
        else if (currentNode==pathGoalNode)  
        {
            pathGoalNode = null;
            path = new List<Node>();
            targetIndex = 0;
            return;
        }

        if (targetNode == null)
        {
            targetNode = NavVolume.NavVolumeInstance.GetNodeFromIndex(currentNode.volumeCoordinate + stepDirection);
            if (targetNode == null) { return; }
        }

        if (!isMoving)
        {
            isMoving = true;
            SetStepDirection(currentNode.volumeCoordinate, targetNode.volumeCoordinate);
            StopCoroutine(MoveToTarget());
            StartCoroutine(MoveToTarget());
        }
    }

    public void OnPathFound(List<Node> newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            isWaitingForPath = false;
            //StopCoroutine(MoveToTarget());
            //StartCoroutine(MoveToTarget());
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

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i].WorldPosition, new Vector3(.2f, .2f, .2f));

                if (i == targetIndex)
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
