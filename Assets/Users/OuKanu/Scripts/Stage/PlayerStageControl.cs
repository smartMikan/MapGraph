using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStageControl : MonoBehaviour
{
    [SerializeField]
    private GameObject canEnterUI;

    RoomPointHolder currentroom;

    public StageEntryPoint entrypoint;

    // Start is called before the first frame update
    void Start()
    {
        DisplayUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (entrypoint)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                entrypoint.Activate(this.transform);
                entrypoint = entrypoint.myTargetPoint;
                currentroom = entrypoint.GetComponentInParent<RoomPointHolder>();
            }
        }

        
    }

    public void DisplayUI(bool enable)
    {
        if(canEnterUI)
            canEnterUI.SetActive(enable);
    }

}
