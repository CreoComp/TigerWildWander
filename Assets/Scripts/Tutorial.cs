using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Game _game;
    public GameObject tutorialPrefab;
    public int id = 0;

    [System.Serializable]
    public class TutorText
    {
        public int id;
        public GameObject tutorialGameID;
    }
    public TutorText[] _pressetsTutorial;

    void Start()
    {
        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            PlayerPrefs.SetInt("Tutorial", 1);
            _game.OnPauseGame();
            OnTutorialGame();
        }
    }

    public void OnTutorialGame()
    {
        id = 0;
        pokazatTutorial();
    }

    public void PrevTutorialGame()
    {
        id = Mathf.Max(0, id - 1);
        pokazatTutorial();
    }

    public void NextTutorialGame(bool skip)
    {
        if (skip || id >= _pressetsTutorial.Length)
        {
            exitTutorial();
        }
        else
        {
            id++;
            pokazatTutorial();
        }
    }

    private void pokazatTutorial()
    {
        tutorialPrefab.SetActive(true);
        Time.timeScale = 0;

        foreach (var item in _pressetsTutorial)
        {
            item.tutorialGameID.SetActive(false);
        }

        if (id < _pressetsTutorial.Length)
        {
            _pressetsTutorial[id].tutorialGameID.SetActive(true);
        }
    }

    private void exitTutorial()
    {
        _game.OnPauseGame();
        tutorialPrefab.SetActive(false);
        Time.timeScale = 1;
    }
}
