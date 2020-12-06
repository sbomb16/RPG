using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{

    void Update()
    {
        // does the player exist?
        if (Player_Controller.me != null && !Player_Controller.me.dead)
        {
            Vector3 targetPos = Player_Controller.me.transform.position;
            targetPos.z = -10;
            transform.position = targetPos;
        }
    }
}
