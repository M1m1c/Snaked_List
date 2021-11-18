using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m1m1c_3DAstarPathfinding
{
    public class AStarPathFinder : MonoBehaviour
    {
        public static MovementType StaticMovementType;

        [SerializeField] private MovementType movementType = MovementType.SingleAxis;
        [SerializeField] private bool shouldSimplifyPath = false;

        [SerializeField] private int stepCostCardinal = 10;
        [SerializeField] private int stepCostDiagonal2D = 14;
        [SerializeField] private int stepCostDiagonal3D = 17;

        private NavVolume navVolumeComp;

        private PathRequestManager PathRequester;

        private Heap<Node> openSet;

        void Awake()
        {
            navVolumeComp = GetComponent<NavVolume>();
            PathRequester = GetComponent<PathRequestManager>();
            openSet = new Heap<Node>(navVolumeComp.volumeNodeLength);
            StaticMovementType = movementType;
        }

        public void StartFindPath(Vector3 pathStart, Vector3 pathEnd)
        {
            StartCoroutine(FindPath(pathStart, pathEnd));
        }

        /*
         FindPath is the major A* portion of the class.

         Itterates over nodes' neighbours between startpos and goalpos, 
         by adding them to closed set and adding/removing them from open set.

        Takes the first node in openset (currentNode), which based on its class "Heap" is the highest priority item.
        nodes added to open set get automatically sorted,
        with high priority nodes ending up closer to the start of the collection.

        Compares the adjacent nodes to the picked out current node, 
        and assigns the adjacent nodes new costs based on current nodes distance from start and goal.
        Sets the current node as parent of the adjacent tile.
        Adds adjacent nodes to open set if they are not already there.

        Itterates until the currentNode is the goal node.
        */
        private IEnumerator FindPath(Vector3 startPos, Vector3 goalPos)
        {

            var path = new List<Node>();
            var succeeded = false;

            var startNode = navVolumeComp.GetNodeFromWorldPosition(startPos);
            var goalNode = navVolumeComp.GetNodeFromWorldPosition(goalPos);

            var isGoalReachable = goalNode.Walkable == true;
            if (isGoalReachable)
            {
                var closedTiles = new HashSet<Node>();
                openSet.Add(startNode);

                while (openSet.Count > 0)
                {

                    var currentNode = openSet.RemoveFirst();
                    closedTiles.Add(currentNode);

                    if (currentNode == goalNode)
                    {
                        succeeded = true;
                        break;
                    }

                    var adjacentNodes = navVolumeComp.GetAdjacentNodes(currentNode, movementType);
                    foreach (var adjacent in adjacentNodes)
                    {

                        if (!adjacent.Walkable || closedTiles.Contains(adjacent)) { continue; }


                        var newGCost = currentNode.gCost + GetNodeDistanceCost(currentNode, adjacent);
                        newGCost += adjacent.WalkPenalty;

                        if (newGCost < adjacent.gCost || !openSet.Contains(adjacent))
                        {
                            adjacent.gCost = newGCost;
                            adjacent.hCost = GetNodeDistanceCost(adjacent, goalNode);
                            adjacent.parent = currentNode;

                            if (!openSet.Contains(adjacent))
                            { openSet.Add(adjacent); }
                            else
                            { openSet.UpdateItem(adjacent); }
                        }
                    }
                }

            }



            yield return null;

            if (succeeded)
            {
                path = RetracePath(startNode, goalNode);
                openSet.Clear();
            }

            PathRequester.FinishedProcessingPath(path, succeeded);
        }

        //Itterates through all parents that are connected to the end Node until the start Node is reached,
        //Then reverses the list that is generated and returns it
        private List<Node> RetracePath(Node startNode, Node endNode)
        {
            var path = new List<Node>();
            var currentTile = endNode;

            while (currentTile != startNode)
            {

                path.Add(currentTile);
                currentTile = currentTile.parent;
            }

            if (shouldSimplifyPath)
            {
                path = SimplifyPath(path);
            }

            path.Reverse();

            return path;
        }

        //Simplifies the path by removing nodes that require movment in the same direction,
        //only using nodes where the movement direciton changes.
        List<Node> SimplifyPath(List<Node> path)
        {
            var waypoints = new List<Node>();
            var directionOld = Vector3.zero;

            for (int i = 1; i < path.Count; i++)
            {
                var directionNew = new Vector3(
                    path[i - 1].volumeCoordinate.x - path[i].volumeCoordinate.x,
                    path[i - 1].volumeCoordinate.y - path[i].volumeCoordinate.y,
                    path[i - 1].volumeCoordinate.z - path[i].volumeCoordinate.z
                    );

                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i]);
                }
                directionOld = directionNew;
            }
            return waypoints;
        }

        //Gets a distance cost based on a step cost,
        //which is modified by the coordinate distance between the parameter nodes.
        //Uses different costs depending on if diagonals are allowed or not.
        private int GetNodeDistanceCost(Node nodeA, Node nodeB)
        {
            var retval = 0;
            var distX = Mathf.Abs(nodeA.volumeCoordinate.x - nodeB.volumeCoordinate.x);
            var distY = Mathf.Abs(nodeA.volumeCoordinate.y - nodeB.volumeCoordinate.y);
            var distZ = Mathf.Abs(nodeA.volumeCoordinate.z - nodeB.volumeCoordinate.z);

            var minDist = Mathf.Min(distX, distY, distZ);
            var maxDist = Mathf.Max(distX, distY, distZ);

            var tripleAxis = minDist;
            var doubleAxis = Mathf.Max(distX + distY + distZ - maxDist - (2 * minDist), 0);
            var singleAxis = maxDist - doubleAxis - tripleAxis;

            if (movementType == MovementType.SingleAxis)
            {
                retval = singleAxis * stepCostCardinal;
            }
            else if (movementType == MovementType.DoubleAxis)
            {
                retval = singleAxis * stepCostCardinal + doubleAxis * stepCostDiagonal2D;
            }
            else
            {
                retval = singleAxis * stepCostCardinal
                    + doubleAxis * stepCostDiagonal2D
                    + tripleAxis * stepCostDiagonal3D;
            }

            return retval;
        }
    }
}


