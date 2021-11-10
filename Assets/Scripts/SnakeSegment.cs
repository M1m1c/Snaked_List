using m1m1c_3DAstarPathfinding;
using System;
using System.Collections;
using UnityEngine;
public class SnakeSegment : MonoBehaviour
{
   

    public bool isMoving { get; protected set; }
    public Node targetNode { get; set; }

    [SerializeField] protected float moveSpeed = 1f;
    protected Node currentNode = null;

    protected int targetIndex = 0;

    public IEnumerator MoveToTarget()
    {
        while (true)
        {

            var dist = Vector3.Distance(transform.position, targetNode.WorldPosition);
            if (Mathf.Approximately(dist, 0f))
            {
                targetIndex++;
                currentNode = targetNode;
                targetNode = null;
                isMoving = false;
                yield break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetNode.WorldPosition,
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