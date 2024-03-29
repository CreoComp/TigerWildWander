using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("G UI")]
    public GameObject GLevels;
    public GameObject GStore;
    public GameObject GSettings;

    [Header("G LEVELS")]
    public Levels[] _Levels;
    public int maxLevels = 50;
    int currentHealth;
    int maxHealthDefault = 100;
    public int maxHealth = 4400;

    [System.Serializable]
    public class Levels
    {
        public int levelID;
        public Button btn_click_level;
        public GameObject lockedLevel;
    }

    [Header("G SETTINGS")]
    public Slider slidervolume;
    public Toggle togglevolume;
    float volumefloat = 1;
    Coins _coins;

    [Header("G STORE")]

    public shop[] _shop;

    [System.Serializable]
    public class shop
    {
        public int id;
        public int price;
        public bool isBuyed;
        public TextMeshProUGUI priceText;
        public Button btnBuy;
        public GameObject btnBuyed;
    }

    [Header("SFX")]

    public AudioSource EventSource;

    private void Start()
    {
        _coins = GetComponent<Coins>();

        foreach (var item in _shop)
        {
            if (PlayerPrefs.GetInt("itemBuyed" + item.id) == 1)
                item.isBuyed = true;
        }

        if (PlayerPrefs.GetFloat("SoundFloat") == 0) { PlayerPrefs.SetFloat("SoundFloat", volumefloat); }

        if (!PlayerPrefs.HasKey("MaxHealth")) PlayerPrefs.SetInt("MaxHealth", maxHealthDefault);

        currentHealth = PlayerPrefs.GetInt("MaxHealth");

        togglevolume.isOn = PlayerPrefs.GetInt("SoundMute") == 0 ? false : true;

        volumefloat = PlayerPrefs.GetFloat("SoundFloat");

        slidervolume.value = volumefloat;

        foreach (var item in _shop)
        {
            item.btnBuy.onClick.AddListener(() => buyItem(item.id));
        }

        foreach (var item in _Levels)
        {
            item.btn_click_level.onClick.AddListener(() => Play(item.levelID));
        }

        StartCoroutine(FBLevelsOpens());
    }

    void Update()
    {
        AudioListener.volume = volumefloat;
    }

    IEnumerator FBLevelsOpens()
    {
        for (int i = 0; i < maxLevels; i++)
        {
            if (PlayerPrefs.GetInt("LevelsComplete " + i) == 1 && i < _Levels.Length)
            {
                _Levels[i].btn_click_level.interactable = true;
                _Levels[i].lockedLevel.SetActive(false);
            }
            else
            {
                if (i < _Levels.Length && _Levels[i] != null)
                {
                    _Levels[i].btn_click_level.interactable = false;
                    _Levels[i].lockedLevel.SetActive(true);
                }
            }
        }

        _Levels[0].btn_click_level.interactable = true;
        _Levels[0].lockedLevel.SetActive(false);

        yield return null;
    }

    public void OnSTORE(bool enable)
    {
        if (enable)
        {
            GStore.SetActive(true);

            foreach (var item in _shop)
            {
                item.priceText.text = item.price.ToString();

                if (item.isBuyed)
                {
                    item.btnBuy.gameObject.SetActive(false);
                    item.btnBuyed.gameObject.SetActive(true);
                }
                else
                {
                    item.btnBuyed.SetActive(false);
                    item.btnBuy.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            GStore.SetActive(false);
        }
    }

    public void buyItem(int index)
    {
        var item = _shop[index];

        if (!_coins.RemoveCoin(item.price)) return;

        if (index != 2)
        {
            item.isBuyed = true;
            item.btnBuy.gameObject.SetActive(false);
            item.btnBuyed.gameObject.SetActive(true);

            PlayerPrefs.SetInt("itemBuyed" + index, 1);
        }
        else
        {
            currentHealth = PlayerPrefs.GetInt("MaxHealth") + 50;

            PlayerPrefs.SetInt("MaxHealth", currentHealth);

            if (currentHealth == maxHealth)
            {
                item.isBuyed = true;
                item.btnBuy.gameObject.SetActive(false);
                item.btnBuyed.gameObject.SetActive(true);

                PlayerPrefs.SetInt("itemBuyed" + index, 1);
            }
        }
    }

    public void OnSettings(bool enable)
    {
        GSettings.SetActive(enable);
    }

    public void Play(int index) { SceneManager.LoadScene("Level " + index); }

    public void OnLevelsG(bool enable)
    {
        GLevels.SetActive(enable);
    }

    public void OnMute(bool mute)
    {
        AudioListener.pause = mute;
        PlayerPrefs.SetInt("SoundMute", mute == true ? 1 : 0);
    }

    public void SetVolume(float vol)
    {
        volumefloat = vol;
        PlayerPrefs.SetFloat("SoundFloat", volumefloat);
    }

    [ContextMenu("PlayerPrefs.DeleteAll")]
    void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
