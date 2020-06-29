using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class ShowPanels : MonoBehaviour
{
    GameObject[] panels;
    long count = 0;
    int index = 0;
    public void Start()
    {
        panels = GameObject.FindGameObjectsWithTag("Panel");
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (count % 700 == 0) 
        {
           // Debug.Log("panels.Length=" + panels.Length);

            if (panels.Length == 0) return;

            panels[index].SetActive(false);
           // Debug.Log("panels[index].name" + panels[index].name);
            index++;
            index = index % panels.Length;
            panels[index].SetActive(true);

        }
        count++;
    }



}
