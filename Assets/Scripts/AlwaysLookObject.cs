using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysLookObject : MonoBehaviour
{
    [SerializeField] Transform target;

    void Update()
    {
        if (target != null)
            this.transform.rotation = Quaternion.LookRotation(this.transform.position - target.position);
    }
}
