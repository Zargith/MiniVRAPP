using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OtherPlayerStartAnimation : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        animator.Play(0, -1, Random.value);
    }
}
