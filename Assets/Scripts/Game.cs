using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Menu;

public class Game : MonoBehaviour
{
    [Header("Level")]
    public int LevelGameID;
    public int LevelAwardsGame = 10;
    public TextMeshProUGUI _levelCountGame;
    public GameObject btnMaslo;
    public GameObject btnMeteor;

    [Header("Complete Level Game")]
    public GameObject completePrefab;
    public TextMeshProUGUI clAwardsCount;
    public bool isCompleteLevel;

    [Header("Gameplay")]
    public Transform[] spawnPoints;
    public Transform[] pointEnd;
    public Slider healthSlider;
    public int health = 100;
    public Animator damageStenaEffectAnimator;
    public Animator damageAnimator;
    public GameObject arrowPrefab1, arrowPrefab2, masloPrefab, stoneballPrefab, stonePrefab;
    public Transform pointStone;
    public Transform pointStoneBall;
    private float arrowsCooldown = 5f, masloCooldown = 30f, stonesCooldown = 45f;
    private float arrowsCooldownUpdate = 5f, masloCooldownUpdate = 30f, stonesCooldownUpdate = 45f;
    private bool isCooldownArrows, isCooldownMaslo, isCooldownStones;
    public Collider2D triggerCollider;
    public GameObject[] monstersPrefab;
    public List<Monster> monstersSpawned = new List<Monster>();
    public int maxMonsters = 5;
    public int countMonsters;
    public int currentMonsters;

    public GameObject KDArrows;
    public GameObject KDMaslo;
    public GameObject KDStones;

    public TextMeshProUGUI KDArrowsText;
    public TextMeshProUGUI KDMasloText;
    public TextMeshProUGUI KDStonesText;

    [Header("GameOver Game")]
    public GameObject GameOverPrefab;
    public bool isGameOver;

    [Header("Options")]
    public GameObject pausePrefab;
    public GameObject menuPrefab;
    public GameObject settingsPrefab;
    public Slider sliderVolumePrefab;
    public Toggle toggleVolumePrefab;
    bool isPause;

    [Header("AUDIO/SFX")]
    public AudioSource MusicSource;
    public AudioSource EventSource;
    public AudioClip completeLevel;
    public AudioClip gameOver;
    public AudioClip damageClip, arrowsClip, masloClip, stoneClip, damageMonsterClip;

    private void Start()
    {
        if (PlayerPrefs.GetInt("itemBuyed" + 0) == 1) btnMaslo.SetActive(true);
        else btnMaslo.SetActive(false);

        if (PlayerPrefs.GetInt("itemBuyed" + 1) == 1) btnMeteor.SetActive(true);
        else btnMeteor.SetActive(false);

        health = PlayerPrefs.GetInt("MaxHealth");

        _levelCountGame.text = "LEVEL " + LevelGameID;

        if (!PlayerPrefs.HasKey("SoundFloat")) { PlayerPrefs.SetFloat("SoundFloat", soundFloat); }

        toggleVolumePrefab.isOn = PlayerPrefs.GetInt("SoundMute") == 0 ? false : true;

        soundFloat = PlayerPrefs.GetFloat("SoundFloat");

        sliderVolumePrefab.value = soundFloat;

        countMonsters = maxMonsters;

        StartCoroutine(SpawnMonsters());
    }

    IEnumerator SpawnMonsters()
    {
        while (currentMonsters < maxMonsters)
        {
            GameObject monster = Instantiate(monstersPrefab[Random.Range(0, monstersPrefab.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

            monstersSpawned.Add(monster.GetComponent<Monster>());

            StartCoroutine(MoveToTarget(monster, pointEnd[Random.Range(0, pointEnd.Length)].position));

            monster.GetComponent<Monster>()._game = this;

            currentMonsters++;

            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator MoveToTarget(GameObject monster, Vector3 targetPosition)
    {
        while (monster != null && Vector3.Distance(monster.transform.position, targetPosition) > 0.1f)
        {
            monster.transform.position = Vector3.MoveTowards(monster.transform.position, targetPosition, Time.deltaTime * 0.9f);
            yield return null;
        }

        if (monster != null && Vector3.Distance(monster.transform.position, targetPosition) < 0.3f)
        {
            if (triggerCollider != null && triggerCollider.bounds.Contains(monster.transform.position))
            {
                Monster monsterComponent = monster.GetComponent<Monster>();
                if (monsterComponent != null)
                {
                    monsterComponent._damageOn = StartCoroutine(DamageAndPlayClip(monsterComponent));
                }
            }
        }
    }


    public IEnumerator DamageAndPlayClip(Monster _monster)
    {
        while (health > 0)
        {
            int damageAmount = Random.Range(1, 6);
            health -= damageAmount;
            EventSource.PlayOneShot(damageClip);
            damageAnimator.SetTrigger("Damage");
            damageStenaEffectAnimator.SetTrigger("Damage");
            _monster.OnAttack();
            yield return new WaitForSeconds(Random.Range(1, 4));
        }
    }

    public void AttackArrows()
    {
        StartCoroutine(arrows());
    }

    IEnumerator arrows()
    {
        if (!isCooldownArrows)
        {
            arrowPrefab1.SetActive(true);
            arrowPrefab2.SetActive(true);

            isCooldownArrows = true;
            StartCoroutine(ShowCooldown(true));

            EventSource.PlayOneShot(arrowsClip);

            if (monstersSpawned.Count > 0)
                foreach (var item in monstersSpawned)
                {
                    if (item != null)
                    {
                        if (Vector3.Distance(item.transform.position, triggerCollider.transform.position) <= 3f)
                        {
                            item.OnDamage(50);
                        }
                    }
                }

            yield return new WaitForSeconds(0.8f);

            arrowPrefab1.SetActive(false);
            arrowPrefab2.SetActive(false);

        }
    }

    public void AttackMaslo()
    {
        StartCoroutine(maslo());
    }

    bool isMasloActive = false;

    IEnumerator maslo()
    {
        if (!isCooldownMaslo)
        {
            masloPrefab.SetActive(true);

            EventSource.PlayOneShot(masloClip);

            isCooldownMaslo = true;
            isMasloActive = true;
            StartCoroutine(ShowCooldown(false, true));

            yield return new WaitForSeconds(5);

            masloPrefab.SetActive(false);
            isMasloActive = false;
        }
    }

    public void AttackMeteor()
    {
        StartCoroutine(stones());
    }

    IEnumerator stones()
    {
        if (!isCooldownStones)
        {
            stoneballPrefab.SetActive(true);
            stoneballPrefab.transform.position = pointStoneBall.position;

            isCooldownStones = true;
            StartCoroutine(ShowCooldown(false, false, true));

            yield return new WaitForSeconds(0);
        }
    }

    public void ThrowMeteor()
    {
        StartCoroutine(MoveToTargetMeteor());
        EventSource.PlayOneShot(stoneClip);
        stoneballPrefab.SetActive(false);
    }

    [HideInInspector] public Vector3 distanceDamageMeteor;

    IEnumerator MoveToTargetMeteor()
    {
        GameObject stone = Instantiate(stonePrefab, pointStone.position, Quaternion.identity);

        while (Vector3.Distance(stone.transform.position, stoneballPrefab.transform.position) > 0.1f)
        {
            stone.transform.position = Vector3.MoveTowards(stone.transform.position, stoneballPrefab.transform.position, Time.deltaTime * 200f);

            foreach (var item in monstersSpawned)
            {
                if (Vector3.Distance(distanceDamageMeteor, item.transform.position) <= 3f)
                {
                    item.OnDamage(100);
                }
            }

            yield return null;
        }

        if (Vector3.Distance(stone.transform.position, stoneballPrefab.transform.position) <= 0.1f)
        {
            Destroy(stone);
        }
    }

    IEnumerator ShowCooldown(bool isArrows = false, bool isMaslo = false, bool isStones = false)
    {
        if (isArrows)
        {
            KDArrows.SetActive(true);
            arrowsCooldownUpdate = arrowsCooldown;
            yield return new WaitForSeconds(arrowsCooldown); 
            KDArrows.SetActive(false);
            isCooldownArrows = false;
        }
        else if (isMaslo)
        {
            KDMaslo.SetActive(true);
            masloCooldownUpdate = masloCooldown;
            yield return new WaitForSeconds(masloCooldown); 
            KDMaslo.SetActive(false);
            isCooldownMaslo = false;
        }
        else if (isStones)
        {
            KDStones.SetActive(true);
            stonesCooldownUpdate = stonesCooldown; 
            yield return new WaitForSeconds(stonesCooldown); 
            KDStones.SetActive(false);
            isCooldownStones = false;
        }
    }

    public void Update()
    {
        if (isCooldownArrows)
        {
            arrowsCooldownUpdate -= Time.deltaTime;
            KDArrowsText.text = ((int)arrowsCooldownUpdate).ToString();
        }
        if (isCooldownMaslo)
        {
            masloCooldownUpdate -= Time.deltaTime;
            KDMasloText.text = ((int)masloCooldownUpdate).ToString();
        }
        if (isCooldownStones)
        {
            stonesCooldownUpdate -= Time.deltaTime;
            KDStonesText.text = ((int)stonesCooldownUpdate).ToString();
        }

        float normalizedHealth = (float)health / 100f;
        healthSlider.value = Mathf.Clamp01(normalizedHealth);


        if (isMasloActive)
        {
            if (monstersSpawned.Count > 0)
                foreach (var item in monstersSpawned)
                {
                    if (item != null)
                    {
                        if (Vector3.Distance(item.transform.position, triggerCollider.transform.position) <= 1f)
                        {
                            item.OnDamageDelay(5, 0.5f);
                        }
                    }
                }
        }

        if (health <= 0 && !isGameOver)
        {
            if (GameOverPrefab) GameOverPrefab.SetActive(true);

            isGameOver = true;

            MusicSource.Stop();

            EventSource.PlayOneShot(gameOver);
        }

        if (countMonsters <= 0 && !isCompleteLevel && currentMonsters >= maxMonsters)
        {
            MusicSource.Stop();

            if (completePrefab) completePrefab.SetActive(true);

            if (PlayerPrefs.HasKey("LevelsComplete " + LevelGameID)) LevelAwardsGame = LevelAwardsGame / 2;
            else PlayerPrefs.SetInt("LevelsComplete " + LevelGameID, 1);

            if (!PlayerPrefs.HasKey("LevelsComplete " + LevelGameID)) PlayerPrefs.SetInt("LevelsComplete " + LevelGameID, 1);

            isCompleteLevel = true;

            clAwardsCount.text = "x" + LevelAwardsGame.ToString();

            EventSource.PlayOneShot(completeLevel);

            GetComponent<Coins>().AddCoin(LevelAwardsGame);
        }
        
        AudioListener.volume = soundFloat;
    }

    public void OnRestart()
    {
        SceneManager.LoadScene("Level " + LevelGameID);
        Time.timeScale = 1;
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene("Level " + (LevelGameID + 1));
        Time.timeScale = 1;
    }

    public void OnMenu()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1;
    }

    public void OnPauseGame()
    {
        if (!isPause)
        {
            isPause = true;
            pausePrefab.SetActive(true);
            menuPrefab.SetActive(true);
            settingsPrefab.SetActive(false);
            Time.timeScale = 0;
        }
        else
        {
            isPause = false;
            pausePrefab.SetActive(false);
            menuPrefab.SetActive(false);
            settingsPrefab.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void OnSettings(bool enable)
    {
        settingsPrefab.SetActive(enable);
        menuPrefab.SetActive(!enable);
    }

    public void OnTutorialGame()
    {
        OnPauseGame();
        GetComponent<Tutorial>().OnTutorialGame();
    }

    public void OnMute(bool mute)
    {
        AudioListener.pause = mute;
        PlayerPrefs.SetInt("SoundMute", mute == true ? 1 : 0);
    }

    float soundFloat = 1;

    public void SetVolume(float vol)
    {
        soundFloat = vol;
        PlayerPrefs.SetFloat("SoundFloat", soundFloat);
    }
}
