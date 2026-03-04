using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
    private GameObject gameManager;
    [SerializeField] private TextMeshProUGUI middleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private GameObject fightingScene1Actors;
    [SerializeField] private GameObject[] fightingScene1MovingActors;
    [SerializeField] private GameObject fightingScene2Actors;
    [SerializeField] private GameObject ocelot;
    [SerializeField] private AudioSource ocelotAudioSource;
    [SerializeField] private AudioClip ocelotFire;
    [SerializeField] private GameObject Vcam2OrbitCenter;
    private float rotateTimer;
    [SerializeField] private GameObject npcActor;
    [SerializeField] private GameObject npcBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private GameObject protagonistActor;
    [SerializeField] private GameObject[] chasingActors;
    [SerializeField] private GameObject chasingProtagonistActor;
    [SerializeField] private GameObject door;
    [SerializeField] private AudioClip NPCFire;
    [SerializeField] private AudioSource audioSourceProtagonist;
    [SerializeField] private AudioSource audioSourceDoor;
    [SerializeField] private GameObject hitNPCFireball;
    private HumanFormEnemyAnimator protagonistAnimator;

    void OnDisable()
    {
        if (gameManager != null) gameManager.SetActive(true);
    }

    void Start()
    {
        blackScreen.SetActive(true);
        middleText.text = "";
        subtitleText.text = "";
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(false);
        npcActor.SetActive(false);
        protagonistActor.SetActive(false);
        door.SetActive(false);
        hitNPCFireball.SetActive(false);
        foreach (GameObject actor in chasingActors)
        {
            actor.SetActive(false);
        }
        chasingProtagonistActor.SetActive(false);
        var gameManagerObject = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (gameManagerObject != null)
        {
            gameManager = gameManagerObject.gameObject;
            gameManager.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndCutScene();
        }
    }
    public void StartFirstFightingCam()
    {
        blackScreen.SetActive(false);
        middleText.text = "";
        fightingScene1Actors.SetActive(true);
        StartCoroutine(StartScene1Walk());
    }

    private IEnumerator StartScene1Walk()
    {
        foreach (GameObject actor in fightingScene1MovingActors)
        {
            var motor = actor.GetComponent<HumanFormEnemyMotor>();
            motor.MoveTo(actor.transform.position + actor.transform.forward * 10f, 2f);
            var animator = actor.GetComponent<HumanFormEnemyAnimator>();
            yield return new WaitForSeconds(0.1f);
            animator.BeginAnimation(HumanFormEnemyAnimationState.Walk);
        }
    }

    public void StartSecondFightingCam()
    {
        blackScreen.SetActive(false);
        middleText.text = "";
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(true);
        StartCoroutine(StartScene2OcelotAttack());
        StartCoroutine(StartVcam2Rotate());
    }
    private IEnumerator StartVcam2Rotate()
    {
        float totalTime = 3f;
        float interval = 0.02f;
        float elapsedTime = 0f;
        float rotateAngle = 150f;
        while (elapsedTime < totalTime){
            Vcam2OrbitCenter.transform.Rotate(Vector3.up, rotateAngle / totalTime * interval);
            elapsedTime += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator StartScene2OcelotAttack()
    {
        var animator = ocelot.GetComponent<HumanFormEnemyAnimator>();
        //var motor = ocelot.GetComponent<HumanFormEnemyMotor>();
        for (int i = 0; i < 5; i++)
        {
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttack);

            // 发射火球，火球生成在敌人前方偏右一点
            ocelotAudioSource.PlayOneShot(ocelotFire);
            Vector3 spawnPos = ocelot.transform.position + ocelot.transform.forward.normalized * 0.2f + ocelot.transform.right.normalized * 0.1f;
            Vector3 dir = ocelot.transform.forward.normalized - ocelot.transform.right.normalized * 0.1f;
            Instantiate(enemyBulletPrefab, spawnPos, Quaternion.LookRotation(dir));

            yield return new WaitUntil(() => animator.IsCurrentAnimationDone());

            float currentY = ocelot.transform.eulerAngles.y;
            float angle = 80f;
            float targetY = currentY + angle;
            ocelot.transform.rotation = Quaternion.Euler(0f, targetY, 0f);
            var spriteRendererObject = ocelot.GetComponentInChildren<SpriteRenderer>().gameObject;
            spriteRendererObject.transform.rotation = Quaternion.Euler(0f, spriteRendererObject.transform.eulerAngles.y - angle, 0f);
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttackStartUp);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void CamTurnToNPC()
    {
        npcActor.SetActive(true);
        protagonistActor.SetActive(true);
        protagonistAnimator = protagonistActor.GetComponent<HumanFormEnemyAnimator>();
        protagonistAnimator.StayInSpecificFrame(0, HumanFormEnemyAnimationState.Walk);
        HumanFormEnemyAnimator npcAnimator = npcActor.GetComponent<HumanFormEnemyAnimator>();
        StartCoroutine(loopAttackAndDied(npcAnimator));
    }
    IEnumerator loopAttackAndDied(HumanFormEnemyAnimator animator)
    {
        subtitleText.text = "kept you waiting, huh?";
        for (int i = 0; i < 3; i++)
        {
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttackStartUp);
            yield return new WaitForSeconds(0.5f);
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttack);
            yield return new WaitUntil(() => animator.IsCurrentAnimationDone());
            // 发射火球，火球生成在敌人前方偏右一点
            audioSourceProtagonist.PlayOneShot(NPCFire);
            Vector3 spawnPos = npcActor.transform.position + npcActor.transform.forward.normalized * 0.2f + npcActor.transform.right.normalized * 0.1f;
            Vector3 dir = npcActor.transform.forward.normalized - npcActor.transform.right.normalized * 0.1f;
            Instantiate(npcBulletPrefab, spawnPos, Quaternion.LookRotation(dir));
        }
        audioSourceProtagonist.PlayOneShot(ocelotFire);
        animator.BeginAnimation(HumanFormEnemyAnimationState.Dead);
        yield return new WaitForSeconds(0.2f);
        hitNPCFireball.SetActive(true);
        subtitleText.text = "Ahh!";
    }

    public void ProtagonistShockAtNPCDeath()
    {
        fightingScene2Actors.SetActive(false);
        subtitleText.text = "";
        protagonistAnimator.StayInSpecificFrame(0, HumanFormEnemyAnimationState.Idle);
    }

    public void ProtagonistRunToBridge()
    {
        foreach (GameObject actor in chasingActors)
        {
            actor.SetActive(true);
            actor.GetComponent<HumanFormEnemyAnimator>().BeginAnimation(HumanFormEnemyAnimationState.Walk);
            actor.GetComponent<HumanFormEnemyMotor>().MoveTo(actor.transform.position + actor.transform.forward * 40f, 4f);
        }
        protagonistActor.SetActive(false);
        chasingProtagonistActor.SetActive(true);
        chasingProtagonistActor.GetComponent<HumanFormEnemyAnimator>().BeginAnimation(HumanFormEnemyAnimationState.Walk);
        chasingProtagonistActor.GetComponent<HumanFormEnemyMotor>().MoveTo(chasingProtagonistActor.transform.position + chasingProtagonistActor.transform.forward * 40f, 3.5f);
        blackScreen.SetActive(false);
        middleText.text = "";

    }

    public void CloseDoor()
    {
        door.SetActive(true);
        StartCoroutine(CloseDoorCoroutine());
    }

    private IEnumerator CloseDoorCoroutine()
    {
        float elapsedTime = 0f;
        float duration = 0.3f;
        Vector3 startPos = door.transform.position;
        Vector3 endPos = startPos + Vector3.down * 2f;

        audioSourceDoor.volume = 0.8f;
        while (elapsedTime < duration)
        {
            door.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        door.transform.position = endPos;
    }

    public void BlackScreenSubtitle(String text)
    {
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(false);
        blackScreen.SetActive(true);
        middleText.text = text;
        subtitleText.text = "";
    }

    public void EnableScene1Actors()
    {
        fightingScene1Actors.SetActive(true);
    }

    public void EnableScene2Actors()
    {
        fightingScene2Actors.SetActive(true);
    }


    public void EndCutScene()
    {
        SceneManager.LoadScene("Bridge");
    }
}
