using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool guiEnable = false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "FPSController")
        {
            guiEnable = true;
        }
    }
    
    void OnGUI(){
        GUIStyle style = new GUIStyle();
        style.fontSize = 30; 
        
        if(guiEnable)
        {
            GUI.Label (new Rect(Screen.width / 3, Screen.height / 2, 300, 50), "WIN! YOU'VE REACHED THE GOAL.", style);
            Application.Quit();
        }
    }
}
