﻿using m1m1c_3DAstarPathfinding;
using System;
using System.Collections;
using UnityEngine;
public class SnakeSegment : MonoBehaviour
{


    public bool IsMoving { get; protected set; }
    public Node TargetNode { get; set; }
    public Node CurrentNode { get; protected set; }

    public SnakeSegment FollowingSegment { get; set; }

    [SerializeField] protected float moveSpeed = 1f;

    public IEnumerator MoveToTarget(Action arrivalAction)
    {
       

        while (true)
        {

            var dist = Vector3.Distance(transform.position, TargetNode.WorldPosition);
            if (Mathf.Approximately(dist, 0f))
            {
                if (arrivalAction != null) { arrivalAction(); }

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

    //public IEnumerator MoveToNewNode(float speed, Action<SnakeSegment> callback)
    //{
    //    if (NewNode == null) { yield break; }
    //    isMoving = true;
    //    while (true)
    //    {
    //        var dist = Vector3.Distance(transform.position, NewNode.WorldPosition);
    //        if (Mathf.Approximately(dist, 0f)) 
    //        {
    //            transform.position = NewNode.WorldPosition;
    //            isMoving = false;
    //            NewNode = null;
    //            callback(this);
    //           yield break;
    //        }
    //        transform.position = Vector3.MoveTowards(transform.position, NewNode.WorldPosition, speed * Time.deltaTime);
    //        yield return null;
    //    }
    //}

}