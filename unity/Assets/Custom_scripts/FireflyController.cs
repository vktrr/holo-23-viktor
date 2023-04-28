// This script controls the behavior of a swarm of fireflies that are attracted to a target object.
// The speed of the target object is used to determine the behavior of the swarm.

using UnityEngine;
using System.Collections.Generic;

public class FireflyController : MonoBehaviour
{
// Reference to the target object that the fireflies are attracted to
public GameObject targetObject;

// Threshold below which the target object is considered to be still
public float stillnessThreshold;

// Amount of time the target object needs to be still before the fireflies are attracted to it
public float stillnessTimeThreshold;

// Maximum speed of the fireflies
public float maxSpeed = 10f;

// Timer that keeps track of how long the target object has been still
private float stillnessTimer;

// The position of the target object in the previous frame
private Vector3 lastObjectPosition;

// Reference to the ParticleSystem component that controls the fireflies
private ParticleSystem fireflies;

// Reference to the AttractionForce component that controls the attraction of the fireflies
private AttractionForce attractionForce;

// Queue that keeps track of the speed of the target object in the previous frames
private Queue<float> lastObjectSpeeds;

// Maximum size of the queue
private int maxQueueSize = 10;

// Start is called before the first frame update
void Start()
{
    // Get the ParticleSystem component attached to this object
    fireflies = GetComponent<ParticleSystem>();

    // Set the initial position of the target object
    lastObjectPosition = targetObject.transform.position;

    // Get the AttractionForce component attached to this object
    attractionForce = GetComponent<AttractionForce>();

    // Create a new queue to store the speed of the target object in the previous frames
    lastObjectSpeeds = new Queue<float>(maxQueueSize);
}

// Update is called once per frame
void FixedUpdate()
{
    // Calculate the speed of the target object in the current frame
    float objectSpeed = (targetObject.transform.position - lastObjectPosition).magnitude / Time.deltaTime;

    // Add the current speed to the queue
    lastObjectSpeeds.Enqueue(objectSpeed);

    // If the queue is full, remove the oldest value
    if (lastObjectSpeeds.Count > maxQueueSize)
    {
        lastObjectSpeeds.Dequeue();
    }

    // Calculate the smoothed speed of the target object using the average of the values in the queue
    float smoothedObjectSpeed = GetAverageSpeed(lastObjectSpeeds);

    // If the smoothed speed is below the stillness threshold, increment the stillness timer
    if (smoothedObjectSpeed < stillnessThreshold)
    {
        stillnessTimer += Time.deltaTime;

        // If the stillness timer has exceeded the stillness time threshold, enable the attraction force
        if (stillnessTimer > stillnessTimeThreshold)
        {
            attractionForce.Enable();
        }
    }
    else
    {
        // If the smoothed speed is above the stillness threshold, reset the stillness timer and calculate the mapped speed
        stillnessTimer = 0;
        float mappedSpeed = Mathf.Clamp01((float)((smoothedObjectSpeed - stillnessThreshold) / (maxSpeed - stillnessThreshold))) * 100;

        // Disable the attraction force with the mapped speed
        // Debug.Log("Object speed: " + objectSpeed);
        // Debug.Log("Mapped speed: " + mappedSpeed);
        attractionForce.Disable(mappedSpeed);
    }

    // Update the position of the target object in the previous frame
    lastObjectPosition = targetObject.transform.position;
}

// Calculates the average speed of the target object using the values in the queue
private float GetAverageSpeed(Queue<float> speeds)
{
    float sum = 0f;
    foreach (float speed in speeds)
    {
        sum += speed;
    }
    return sum / (float)speeds.Count;
}
}
