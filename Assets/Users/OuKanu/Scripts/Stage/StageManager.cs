using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StageManager : MonoBehaviour
{
    public Cinemachine.CinemachineConfiner cameraconfiner;
    public FadeInCanvas fadecanvas;
    
    [HideInInspector]
    public static StageManager instance;


    
    

    private void Awake()
    {
        instance = this;
        
    }

    private void Start()
    {
        

    }

    

    public IEnumerator MoveToRoom(StageEntryPoint point,Transform player)
    {
        //diable move
        var control = player.GetComponent<UnityChan2DController>();
        control.SetPlayerControl(false);
        //start fadein
        yield return StartCoroutine(fadecanvas.DoFadeIn(1,1));
        //teleport player
        player.transform.position = point.m_inTransform.position;
        //set room camera confiner
        var confiner = point.GetComponentInParent<RoomPointHolder>().cameraBound;
        if (confiner)
            cameraconfiner.m_BoundingShape2D = confiner;

        //start fadeout
        yield return StartCoroutine(fadecanvas.DoFadeIn(1, 0));
        //enable move
        control.SetPlayerControl(true);

    }

}
