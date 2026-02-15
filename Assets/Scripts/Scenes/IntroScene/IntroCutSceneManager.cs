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
    [SerializeField] private GameObject fightingScene2Actors;
    [SerializeField] private GameObject npcActor;
    [SerializeField] private GameObject npcBulletPrefab;
    [SerializeField] private GameObject protagonistActor;
    [SerializeField] private GameObject[] chasingActors;
    [SerializeField] private GameObject door;
    private HumanFormEnemyAnimator protagonistAnimator;

    void OnDisable()
    {
        gameManager.SetActive(true);
    }

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include).gameObject;
        gameManager.SetActive(false);
        blackScreen.SetActive(true);
        middleText.text = "";
        subtitleText.text = "";
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(false);
        npcActor.SetActive(false);
        protagonistAnimator = protagonistActor.GetComponent<HumanFormEnemyAnimator>();
        protagonistAnimator.enabled = false;
        protagonistActor.SetActive(false);
        door.SetActive(false);
        foreach (GameObject actor in chasingActors)
        {
            actor.SetActive(false);
        }
    }

    public void StartFirstFightingCam()
    {
        blackScreen.SetActive(false);
        middleText.text = "";
        fightingScene1Actors.SetActive(true);
    }

    public void StartSecondFightingCam()
    {
        blackScreen.SetActive(false);
        middleText.text = "";
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(true);
    }

    public void CamTurnToNPC()
    {
        npcActor.SetActive(true);
        protagonistActor.SetActive(true);
        HumanFormEnemyAnimator npcAnimator = npcActor.GetComponent<HumanFormEnemyAnimator>();
        StartCoroutine(loopAttackAndDied(npcAnimator));
    }
    IEnumerator loopAttackAndDied(HumanFormEnemyAnimator animator)
    {
        subtitleText.text = "Get to bridge, send the message!";
        for (int i = 0; i < 3; i++)
        {
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttackStartUp);
            yield return new WaitForSeconds(0.5f);
            // 发射火球，火球生成在敌人前方偏右一点
            Vector3 spawnPos = npcActor.transform.position + npcActor.transform.forward.normalized * 0.2f + npcActor.transform.right.normalized * 0.1f;
            Vector3 dir = npcActor.transform.forward.normalized - npcActor.transform.right.normalized * 0.1f;
            var s = Instantiate(npcBulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            Debug.Log("Bullet Spawned at " + spawnPos + " with direction " + dir);
            Debug.Log(s);
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttack);
            yield return new WaitUntil(() => animator.IsCurrentAnimationDone());
        }
        subtitleText.text = "Ahh!";
        animator.BeginAnimation(HumanFormEnemyAnimationState.Dead);
    }

    public void ProtagonistShockAtNPCDeath()
    {
        subtitleText.text = "";
        protagonistActor.SetActive(true);
        protagonistAnimator.enabled = true;
        protagonistAnimator.BeginAnimation(HumanFormEnemyAnimationState.Idle);
    }

    public void ProtagonistRunToBridge()
    {
        foreach (GameObject actor in chasingActors)
        {
            actor.SetActive(true);
            actor.GetComponent<HumanFormEnemyAnimator>().BeginAnimation(HumanFormEnemyAnimationState.Walk);
            actor.GetComponent<HumanFormEnemyMotor>().MoveTo(actor.transform.position + actor.transform.forward * 40f, 3.5f);
        }
        protagonistActor.SetActive(false);
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
        float duration = 0.5f;
        Vector3 startPos = door.transform.position;
        Vector3 endPos = startPos + Vector3.down * 2f;

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
        blackScreen.SetActive(true);
        middleText.text = text;
        subtitleText.text = "";
    }

    public void EndCutScene()
    {
        SceneManager.LoadScene("Bridge");
    }
}
