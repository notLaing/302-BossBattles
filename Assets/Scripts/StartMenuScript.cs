using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StartMenuScript : MonoBehaviour
{
    public Button startButton, creditsButton;
    public GameObject panel;

    public void ClickStart()
    {
        SceneManager.LoadScene("Project3");
    }

    public void ClickCredits()
    {
        panel.SetActive(true);
        startButton.gameObject.SetActive(false);
        creditsButton.gameObject.SetActive(false);
    }

    public void ClickReturn()
    {
        startButton.gameObject.SetActive(true);
        creditsButton.gameObject.SetActive(true);
        panel.SetActive(false);
    }
}
