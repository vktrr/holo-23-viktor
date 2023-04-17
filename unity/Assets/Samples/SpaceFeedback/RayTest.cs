using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTest : MonoBehaviour
{
    public bool isHit;
    public RaycastHit hitInfo;

    void FixedUpdate()
    {
        var ray = new Ray(transform.position, transform.forward);
        isHit = Physics.Raycast(ray, out var hit);
        Debug.DrawRay(ray.origin,ray.direction*(isHit?hit.distance:100),Color.yellow);
        hitInfo = hit;
    }
}
