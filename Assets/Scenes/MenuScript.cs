using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI counterText;
    public float counter = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        //counterText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (counterText != null)
        {
            
                counterText.text= counter.ToString();
        }
        else
        {
            Debug.Log("erreur");
        }

    }

    public void OnScriptButton () 
    {
        SceneManager.LoadScene(1);
    }
    public void OnQuitButton()
    {
        
        Application.Quit();
    }
    public void onOpenPanelButton()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

}
