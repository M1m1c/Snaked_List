using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m1m1c_3DAstarPathfinding
{
    public class NavVolume : MonoBehaviour
    {
        public static NavVolume NavVolumeInstance;

        public LayerMask UnwalkableMask;

        public int volumeNodeLength
        {
            get { return volumeNodes.Length; }
           
        }

        [SerializeField] private Vector3 volumeSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private float nodeSize = 1f;

        private UnityEvent ResetWalkPenaltiesEvent = new UnityEvent();

        private int xWidth;
        private int yHeight;
        private int zDepth;

        private Vector3 volumeBottomLeftCorner = new Vector3();
        private Vector3 volumeTopRightCorner = new Vector3();

        private Node[,,] volumeNodes = new Node[0, 0, 0];

        private void Awake()
        {
            NavVolumeInstance = this;

            xWidth = Mathf.RoundToInt(volumeSize.x / nodeSize);
            yHeight = Mathf.RoundToInt(volumeSize.y / nodeSize);
            zDepth = Mathf.RoundToInt(volumeSize.z / nodeSize);

            volumeBottomLeftCorner = transform.position
                - (Vector3.right * volumeSize.x / 2f)
                - (Vector3.up * volumeSize.y / 2f)
                - (Vector3.forward * volumeSize.z / 2f);

            volumeTopRightCorner = transform.position
                + (Vector3.right * volumeSize.x / 2f)
                + (Vector3.up * volumeSize.y / 2f)
                + (Vector3.forward * volumeSize.z / 2f);

            GenerateNodesInVolume();

            AddAllAjacentNodesAsListeners();
        }

       
        //Returns the node inside the NavVolume that is closest to the world position sent in.
        public Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            Node retval = null;

            var xWithinVolume = worldPosition.x >= volumeBottomLeftCorner.x && worldPosition.x <= volumeTopRightCorner.x;
            var yWithinVolume = worldPosition.y >= volumeBottomLeftCorner.y && worldPosition.y <= volumeTopRightCorner.y;
            var zWithinVolume = worldPosition.z >= volumeBottomLeftCorner.z && worldPosition.z <= volumeTopRightCorner.z;

            if (xWithinVolume && yWithinVolume && zWithinVolume)
            {
                float percentX = (worldPosition.x + volumeSize.x / 2) / volumeSize.x;
                float percentY = (worldPosition.y + volumeSize.y / 2) / volumeSize.y;
                float percentz = (worldPosition.z + volumeSize.z / 2) / volumeSize.z;
                percentX = Mathf.Clamp01(percentX);
                percentY = Mathf.Clamp01(percentY);
                percentz = Mathf.Clamp01(percentz);

                int x = Mathf.RoundToInt((xWidth - 1) * percentX);
                int y = Mathf.RoundToInt((yHeight - 1) * percentY);
                int z = Mathf.RoundToInt((zDepth - 1) * percentz);

                retval = volumeNodes[x, y, z];
            }
            return retval;
        }

        public Node GetNodeFromIndex(Vector3Int index)
        {
            Node retval = null;

            var isWithinBounds = 
                index.x < volumeNodes.GetLongLength(0) && index.x >= 0 &&
                index.y < volumeNodes.GetLongLength(1) && index.y >= 0 &&
                index.z < volumeNodes.GetLongLength(2) && index.z >= 0;

            if (isWithinBounds)
            {
                var foundNode = volumeNodes[index.x, index.y, index.z];
                if (foundNode != null)
                {
                    retval = foundNode;
                }
            }

            return retval;
        }

        //Checks a 27 node (cube) section based on current tile, returns the found nodes.
        //Depending on the MovementType,
        //returns only nodes cardinally adjacent, 2D diagonally adjacent or 3D diagonals aswell
        public List<Node> GetAdjacentNodes(Node currentNode, MovementType movementType)
        {
            var retval = new List<Node>();

            var xCurrent = currentNode.volumeCoordinate.x;
            var yCurrent = currentNode.volumeCoordinate.y;
            var zCurrent = currentNode.volumeCoordinate.z;
            var tempGridPos = new Vector3Int();


            for (int x = -1; x <= 1; x++)
            {
                tempGridPos.x = xCurrent + x;

                for (int y = -1; y <= 1; y++)
                {
                    tempGridPos.y = yCurrent + y;

                    for (int z = -1; z <= 1; z++)
                    {
                        tempGridPos.z = zCurrent + z;

                        var isCurrentTile =
                            xCurrent == tempGridPos.x &&
                            yCurrent == tempGridPos.y &&
                            zCurrent == tempGridPos.z;

                        if (isCurrentTile) { continue; }

                        var isOutOfBounds =
                            tempGridPos.x >= xWidth ||
                            tempGridPos.y >= yHeight ||
                            tempGridPos.z >= zDepth ||
                            tempGridPos.x < 0 ||
                            tempGridPos.y < 0 ||
                            tempGridPos.z < 0;

                        if (isOutOfBounds) { continue; }

                        if (movementType != MovementType.TripleAxis)
                        {
                            var matchingAxisCount = 0;
                            matchingAxisCount = matchingAxisCount + (tempGridPos.x == xCurrent ? 1 : 0);
                            matchingAxisCount = matchingAxisCount + (tempGridPos.y == yCurrent ? 1 : 0);
                            matchingAxisCount = matchingAxisCount + (tempGridPos.z == zCurrent ? 1 : 0);

                            if (movementType == MovementType.SingleAxis)
                            {
                                if (matchingAxisCount != 2) { continue; }
                            }
                            else if (movementType == MovementType.DoubleAxis)
                            {
                                if (matchingAxisCount == 0) { continue; }
                            }
                        }                       


                        var foundNode = volumeNodes[tempGridPos.x, tempGridPos.y, tempGridPos.z];
                        if (foundNode == null) { continue; }

                        retval.Add(foundNode);
                    }

                }
            }
            return retval;
        }

        public Node GetRandomNode()
        {
            var x = UnityEngine.Random.Range(0, xWidth - 1);
            var y = UnityEngine.Random.Range(0, yHeight - 1);
            var z = UnityEngine.Random.Range(0, zDepth - 1);
            return volumeNodes[x, y, z];
        }

        public void ResetWalkPenaltiesInAllNodes()
        {
            ResetWalkPenaltiesEvent.Invoke();
        }

        //Genereates the actual NavVolume by creating virtual nodes within its bounds based on set size
        private void GenerateNodesInVolume()
        {
            volumeNodes = new Node[xWidth, yHeight, zDepth];

            var halfNodeSize = nodeSize / 2f;

            var bottomLeftNodePos = volumeBottomLeftCorner
                + new Vector3(halfNodeSize, halfNodeSize, halfNodeSize);

            for (int x = 0; x < xWidth; x++)
            {
                for (int y = 0; y < yHeight; y++)
                {
                    for (int z = 0; z < zDepth; z++)
                    {

                        var nodePos = bottomLeftNodePos
                            + Vector3.right * (x * nodeSize)
                            + Vector3.up * (y * nodeSize)
                            + Vector3.forward * (z * nodeSize);

                        var walkable = !Physics.CheckSphere(nodePos, halfNodeSize, UnwalkableMask);

                        volumeNodes[x, y, z] = new Node(walkable, nodePos, new Vector3Int(x, y, z));
                        ResetWalkPenaltiesEvent.AddListener(volumeNodes[x, y, z].ResetWalkPenalty);
                    }
                }
            }
        }

        //Makes each Node add its neighbours as listeners to its change walk penalty event
        private void AddAllAjacentNodesAsListeners()
        {
            foreach (var node in volumeNodes)
            {
                var adjacent = GetAdjacentNodes(node, AStarPathFinder.StaticMovementType);
                node.AddAdjacentNodesAsListeners(adjacent.ToArray());
            }
        }      
    }
}
