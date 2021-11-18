using UnityEngine;
using UnityEngine.Events;

namespace m1m1c_3DAstarPathfinding
{
    public class Node : IHeapItem<Node>
    {
        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }

        public int HeapIndex { get; set; }
        public int Key { get; set; }

        public Node parent;

        public bool Walkable { get; private set; }

        public int WalkPenalty { get; set; }

        public Vector3 WorldPosition { get; private set; }

        public Vector3Int volumeCoordinate { get; private set; }

        public UnityEvent NodeWalkableDisabledEvent = new UnityEvent();
        private UnityEvent<int, Node> AdjacentWalkPenaltyEvent = new UnityEvent<int, Node>();

        private int maxWalkPenalty = 50;

        public Node(bool walkable, Vector3 worldPos, Vector3Int coordinate)
        {
            Walkable = walkable;
            WorldPosition = worldPos;
            volumeCoordinate = coordinate;
        }

        //Used to compare tile costs for pathfinding purposes
        public int CompareTo(Node nodeToCompare)
        {
            var compare = fCost.CompareTo(nodeToCompare.fCost);

            if (compare == 0) { compare = hCost.CompareTo(nodeToCompare.hCost); }

            return -compare;
        }

        public void AddAdjacentNodesAsListeners(Node[] nodes)
        {
            foreach (var node in nodes)
            {
                AdjacentWalkPenaltyEvent.AddListener(node.ChangeWalkPenaltyExluding);
            }
        }

        public void ChangeWalkableState(bool isWalkable)
        {
            Walkable = isWalkable;
            if (Walkable == false) { NodeWalkableDisabledEvent.Invoke(); }
        }

        //changes thsis ndoes walk penalty unless it is the node sent in at a parameter
        public void ChangeWalkPenaltyExluding(int value, Node followingNode)
        {

            if (followingNode == this) { return; }

            this.WalkPenalty = Mathf.Clamp(WalkPenalty + value, 0, maxWalkPenalty);
        }

        //calls the change of walk penalties in adjacent tiles excluding the parameter node
        public void InvokeWalkPenaltiesEvent(int value, Node followingNode)
        {
            AdjacentWalkPenaltyEvent.Invoke(value, followingNode);
        }

        public void ResetWalkPenalty()
        {
            WalkPenalty = 0;
        }
    }
}
