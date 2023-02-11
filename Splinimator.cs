using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Splinimator : MonoBehaviour
{

    [Header("Animation Details")]
        [SerializeField] Transform target;
        [SerializeField] float time;
        [SerializeField] float speed = 1;
        [SerializeField] int currentSegment = 0;
        [SerializeField] bool loop = false;
        private Vector3 originalPosition;
        private Vector3 originalRotation;
        
    private bool canUpdate = true;
        
    [System.Serializable]
    public class Segment
    {
        [Tooltip("The time in seconds that the segment takes to complete")]
        public float time = 1;
        [Tooltip("If true, the handle will be the same as the end point, making the path a straight line")]
        public bool isLinear = false;
        public Vector3 handle;
        public Vector3 endPoint;
        public bool useRotation = false;
        public Vector3 rotation;
        
        [Tooltip("The methods to call throught the segment")]
        public UnityEvent methodDuringSegment;

        [Tooltip("The methods to call at the end of the segment")]
        public UnityEvent methodAtEndOfSegment;

    }
    public List<Segment> segments = new List<Segment>();


    void OnEnable()
    {
        if (target == null)
            target = transform;

        originalPosition = transform.position;
        originalRotation = transform.eulerAngles;
    }

    void Update()
    {
        if (canUpdate)
            Animation();
    }



    void Animation()
    {
        canUpdate = false;

        // get total time of all segments
        float totalTime = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            totalTime += segments[i].time;

            if (segments[i].isLinear)
                segments[i].handle = segments[i].endPoint;
        }


        // while the time is less than or equal to the total time
        while (true)
        {
            // if loop is false and the current segment is the last segment, stay on the last segment
            if (!loop && currentSegment == segments.Count)
            {
                canUpdate = true;
                break;
            }
                
            else if (currentSegment == segments.Count) // if the current segment is greater than the number of segments, decrement the current segment
                currentSegment = 0;


            // increment time
            time += Time.deltaTime * speed;

            // if the time is greater than the total time, reset the time 
            if (time > totalTime)
            {
                time = totalTime;
                continue;
            }
            // if the time is less than 0, reset the time
            else if (time < -.5f)
            {
                time = 0;
                continue;
            }


            // call the segments methods
            segments[currentSegment].methodDuringSegment.Invoke();


            // get the start, handle, and end positions
            Vector3 p1;
            if (currentSegment == 0) // if it is the first segment
                p1 = originalPosition;
            else // if it is not the first segment
                p1 = segments[currentSegment - 1].endPoint;
            
            Vector3 h = segments[currentSegment].handle;
            Vector3 p2 = segments[currentSegment].endPoint;

            float timePercent = time / segments[currentSegment].time;

            // lerp bridges
            Vector3 p1_h = Vector3.Lerp(p1, h, timePercent);
            Vector3 h_p2 = Vector3.Lerp(h, p2, timePercent);

            // lerp target
            target.position = Vector3.Lerp(p1_h, h_p2, timePercent);

            // if the segment uses rotation, lerp the rotation
            if (segments[currentSegment].useRotation)
            {
                Quaternion startRotation;
                if (currentSegment == 0) // if it is the first segment
                    startRotation = transform.rotation;
                else // if it is not the first segment
                    startRotation = Quaternion.Euler(segments[currentSegment - 1].rotation);

                Quaternion endRotation = Quaternion.Euler(segments[currentSegment].rotation);
                
                target.rotation = Quaternion.Lerp(startRotation, endRotation, timePercent);
            }

            // continue to next segment
            if (time >= segments[currentSegment].time)
            {
                // reset time
                time = 0;

                // call the segments methods
                segments[currentSegment].methodAtEndOfSegment.Invoke();

                // increment current segment if loop is true or if the current segment is less than the number of segments
                if (loop || currentSegment < segments.Count)
                {
                    currentSegment++;
                }
                    
                
                // if the current segment is greater than the number of segments and loop is true, loop
                if (currentSegment != segments.Count || !loop) 
                {
                    canUpdate = true;
                    break;
                }
                

                continue;
            }
            // go to previous segment
            else if (time < 0 && currentSegment > 0 && loop)
            {
                time += segments[currentSegment].time;
                currentSegment--;
                continue;
            }
            // break if there is nothing else to do
            canUpdate = true;
            break;
            
        }
    }



    public void CreatePoint()
    {  
        Segment newSegment = new Segment();

        // set position of new point
        Vector3 newPosition = originalPosition;
        if (segments.Count != 0) // if there are already segments.endPoint, set the new point to the last point in the list
            newPosition = segments[segments.Count - 1].endPoint + Vector3.up;


        // add the new point to the list
        newSegment.endPoint = newPosition;

        if (segments.Count > 1) // if there are at least two segments.endPoint
        {
            // get position of the two points
            Vector3 averagePosition = (segments[segments.Count - 1].endPoint + segments[segments.Count - 2].endPoint) / 2;
                        
        }

        // add the new handle to the list
        newSegment.handle = newPosition;

        // add the new segment to the list
        segments.Add(newSegment);
    }

    public void DeletePoint()
    {
        // remove the last point from the list
        segments.RemoveAt(segments.Count - 1);
    }
    
    private void OnDrawGizmos()
    {
        Color red = Color.red;
        red.a = 0.5f;

        // draw each point in the list
        Gizmos.color = red;
        Gizmos.DrawSphere(originalPosition, 0.1f);

        for (int i = 0; i < segments.Count; i++)
        {
            Gizmos.DrawSphere(segments[i].endPoint, 0.1f);

        }

    }
    
}

