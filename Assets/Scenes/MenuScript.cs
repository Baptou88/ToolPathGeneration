using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI counterText;
    public float counter = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (counterText != null)
        {

            counterText.text = counter.ToString();
        }

    }

    public void OnScriptButton()
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
