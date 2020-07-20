using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInCanvas : MonoBehaviour
{
    public float alpha = 0f;
    public bool m_IsFading = false;

    private CanvasGroup mCanvasGroup;


    public IEnumerator DoFadeIn(float time,float targetAlpha)
    {

        m_IsFading = true;
        float fadeSpeed = Mathf.Abs(mCanvasGroup.alpha - targetAlpha) / time;
        while (!Mathf.Approximately(mCanvasGroup.alpha, targetAlpha))
        {
            mCanvasGroup.alpha = Mathf.MoveTowards(mCanvasGroup.alpha, targetAlpha,
                fadeSpeed * Time.deltaTime);
            yield return null;
        }
        mCanvasGroup.alpha = 0;
        m_IsFading = false;
        
    }

    void Start()
    {
        mCanvasGroup = GetComponent<CanvasGroup>();
    }

   
}
