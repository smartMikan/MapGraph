using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallEventTrigger : MonoBehaviour
{
    public GameObject dropArea;
    public Collider2D targetCameraBound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            StartCoroutine(Drop());
    }

    private IEnumerator Drop()
    {
        dropArea.SetActive(false);
        StageManager.instance.cameraconfiner.m_BoundingShape2D = null;
        yield return new WaitForSeconds(1f);
        StageManager.instance.cameraconfiner.m_BoundingShape2D = targetCameraBound;
    }
}
