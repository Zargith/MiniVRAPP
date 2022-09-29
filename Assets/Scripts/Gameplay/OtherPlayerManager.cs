using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OtherPlayerManager : InterfacePlayerManager
{
    public override void Die(string reason = "")
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Animator>().enabled = false;
        GetComponentInChildren<TextMeshPro>().color = Color.red;
        StartCoroutine(dieCoroutine());
    }

    IEnumerator dieCoroutine()
    {
        yield return new WaitForSeconds(3);
        this.gameObject.SetActive(false);
    }
}
