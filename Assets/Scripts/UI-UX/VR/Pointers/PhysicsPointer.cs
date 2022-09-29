using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPointer : MonoBehaviour
{
    [SerializeField] float defaultLength = 3.0f;
    LineRenderer _lineRenderer = null;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        UpdateLength();
    }

    void UpdateLength()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, CalculateEnd());
    }

    public Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        Vector3 endPosition = DefaultEnd(defaultLength);

        if (hit.collider)
            endPosition = hit.point;

        return endPosition;
    }

    RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        Physics.Raycast(ray, out hit, defaultLength);

        return hit;
    }

    Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
}
