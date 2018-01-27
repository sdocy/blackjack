using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class quit : MonoBehaviour {
    public Text quitButtonText;
    public Text YButtonText;
    public Text NButtonText;

    public void quitRequest()
    {
        enableYesNo();
    }

    public void answerYes(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void answerNo()
    {
        disableYesNo();
    }

    void Start()
    {
        disableYesNo();
    }

    void enableYesNo()
    {
        YButtonText.gameObject.SetActive(true);
        NButtonText.gameObject.SetActive(true);
    }

    void disableYesNo()
    {
        YButtonText.gameObject.SetActive(false);
        NButtonText.gameObject.SetActive(false);
    }
}
