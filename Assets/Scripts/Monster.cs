using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [HideInInspector] public Game _game;
    [HideInInspector] public Coroutine _damageOn;

    public Animator _walking;
    public Animator _attackEffect;

    public SpriteRenderer monsterSprite;
    public AudioClip deadMonsterClips;
    public GameObject[] monsterDeadBloods;

    public int health = 100;
    bool isDead;

    public float minRotation = -20f;
    public float maxRotation = 20f;
    public float rotationSpeed = 2f;

    private float targetRotation;

    private void Start()
    {
        monsterSprite = GetComponent<SpriteRenderer>();

        targetRotation = maxRotation;
    }

    public void Update()
    {
        if (health <= 0 && !isDead)
        {
            OnDead();
            isDead = true;
        }
    }

    public void OnDamage(int damage)
    {
        health -= damage;
        _game.EventSource.PlayOneShot(_game.damageMonsterClip);
    }

    Coroutine _coroutineDamageDelay;

    public void OnDamageDelay(int damage, float delay)
    {
        _coroutineDamageDelay = StartCoroutine(OnDamage(damage, delay));
    }

    bool isDamage;

    IEnumerator OnDamage(int damage, float delay)
    {
        if (isDamage) yield return null;

        isDamage = true;
        health -= damage;

        yield return new WaitForSeconds(delay);
        isDamage = false;
    }

    public void OnAttack()
    {
        _walking.gameObject.SetActive(false);

        _attackEffect.SetTrigger("Attack");

        float currentRotation = transform.rotation.eulerAngles.z;
        float smoothRotation = Mathf.MoveTowardsAngle(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, smoothRotation);

        if (Mathf.Approximately(transform.rotation.eulerAngles.z, targetRotation))
        {
            targetRotation = targetRotation == maxRotation ? minRotation : maxRotation;
        }
    }

    public void OnDead()
    {
        _game.countMonsters--;
        _game.monstersSpawned.Remove(this);

        _walking.gameObject.SetActive(false);
        _attackEffect.gameObject.SetActive(false);

        if (_coroutineDamageDelay != null) StopCoroutine(_coroutineDamageDelay);
        if (_damageOn != null) StopCoroutine(_damageOn);

        _game.EventSource.PlayOneShot(deadMonsterClips);

        monsterSprite.enabled = false;
        GameObject obj = Instantiate(monsterDeadBloods[Random.Range(0, 1)], transform.position, Quaternion.identity);
        obj.transform.SetParent(monsterSprite.transform);
        obj.transform.localScale = new Vector3(0.04965432f, 0.04965432f, 0.04965432f);

        Destroy(gameObject, 3f);
    }
}
