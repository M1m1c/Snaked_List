using System;
using System.Collections.Generic;
using UnityEngine;

namespace m1m1c_3DAstarPathfinding
{
    public class PathRequestManager : MonoBehaviour
    {
        Queue<PathRequest> PathRequestQueue = new Queue<PathRequest>();
        PathRequest currentRequest;

        static PathRequestManager PathRequesterInstance;
        AStarPathFinder PathFinder;

        bool isProceesingPath = false;

        private void Awake()
        {
            PathRequesterInstance = this;
            PathFinder = GetComponent<AStarPathFinder>();
        }

        //Creates a request based on paramters and stores it in a queue, starts processing next request
        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<List<Node>, bool> callBack, GameObject requester)
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callBack, requester);
            PathRequesterInstance.PathRequestQueue.Enqueue(newRequest);
            PathRequesterInstance.TryProcessNextRequest();
        }

        //Starts the process of genererating a path based on the first request in the queue using the AStarPathFinder class
        private void TryProcessNextRequest()
        {
            if (!isProceesingPath && PathRequestQueue.Count > 0)
            {
                currentRequest = PathRequestQueue.Dequeue();

                var requester = currentRequest.Requester;
                if (requester == null) { return; }

                isProceesingPath = true;
                PathFinder.StartFindPath(currentRequest.pathStart, currentRequest.pathEnd);
            }
        }

        //Is called when AStarPathFInder has finished finding a path.
        //Calls the callback method, then resumes processing of next request in queue
        public void FinishedProcessingPath(List<Node> path, bool wasSucessfull)
        {
            if (currentRequest.Requester)
            {
                currentRequest.callBack(path, wasSucessfull);
            }

            isProceesingPath = false;
            PathRequesterInstance.TryProcessNextRequest();
        }

        struct PathRequest
        {
            public Vector3 pathStart;
            public Vector3 pathEnd;
            public Action<List<Node>, bool> callBack;
            public GameObject Requester;

            public PathRequest(Vector3 start, Vector3 end, Action<List<Node>, bool> call, GameObject requester)
            {
                pathStart = start;
                pathEnd = end;
                callBack = call;
                Requester = requester;
            }
        }
    }
}
