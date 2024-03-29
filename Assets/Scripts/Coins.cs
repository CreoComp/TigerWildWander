using System.Collections;
using TMPro;
using UnityEngine;

public class Coins : MonoBehaviour
{
    [Header("COINS")]
    public int currentCoins;
    public TextMeshProUGUI[] coinsText;

    [Header("SFX")]
    public AudioSource ESource;
    public AudioClip buyClip;
    public AudioClip errorClip;

    private void Start()
    {
        currentCoins = PlayerPrefs.GetInt("Coins");
    }

    void Update()
    {
        if (coinsText.Length > 0)
        {
            foreach (var item in coinsText) item.text = currentCoins.ToString();
        }
    }

    public void AddCoin(int index)
    {
        StartCoroutine(coinDelay(index));
    }

    public bool RemoveCoin(int index)
    {
        if (currentCoins < index)
        {
            if (errorClip) ESource.PlayOneShot(errorClip);
            return false;
        }
        else
        {
            StartCoroutine(coinDelay(-index));
            return true;
        }
    }

    IEnumerator coinDelay(int index)
    {
        currentCoins += index;

        if (buyClip) ESource.PlayOneShot(buyClip);

        PlayerPrefs.SetInt("Coins", currentCoins);

        yield return new WaitForSeconds(0.01f);
    }
}