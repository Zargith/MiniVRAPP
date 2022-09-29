using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class CanvasPointer : MonoBehaviour
{
    [SerializeField] float defaultLength = 3.0f;
    [SerializeField] EventSystem eventSystem = null;
    [SerializeField] XRUIInputModule inputModule = null;
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
        _lineRenderer.SetPosition(1, GetEnd());
    }

    Vector3 GetEnd()
    {
        float distance = GetCanvasDistance();
        Vector3 endPosition = CalculateEnd(defaultLength);

        if (distance != 0)
            endPosition = CalculateEnd(distance);

        return endPosition;
    }

    float GetCanvasDistance()
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = inputModule.inputOverride.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(eventData, results);

        return Mathf.Clamp(FindFirstRaycast(results).distance, 0, defaultLength);
    }

    RaycastResult FindFirstRaycast(List<RaycastResult> raycastResults)
    {
        foreach (RaycastResult result in raycastResults) {
            if (!result.gameObject)
                continue;
            return result;
        }

        return new RaycastResult();
    }

    Vector3 CalculateEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }

    Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
}
