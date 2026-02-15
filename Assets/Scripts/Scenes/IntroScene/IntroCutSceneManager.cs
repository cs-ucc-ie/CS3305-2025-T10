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

    void OnDisable()
    {
        gameManager.SetActive(true);
    }

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>().gameObject;
        gameManager.SetActive(false);
        blackScreen.SetActive(true);
        middleText.text = "";
        subtitleText.text = "";
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(false);
        npcActor.SetActive(false);
        protagonistActor.SetActive(false);
        door.SetActive(false);
        foreach (GameObject actor in chasingActors)
        {
            actor.SetActive(false);
        }
    }

    public void StartFirstFightingCam()
    {
        fightingScene1Actors.SetActive(true);
    }

    public void StartSecondFightingCam()
    {
        fightingScene1Actors.SetActive(false);
        fightingScene2Actors.SetActive(true);
    }

    public void CamTurnToNPC()
    {
        npcActor.SetActive(true);
        HumanFormEnemyAnimator npcAnimator = npcActor.GetComponent<HumanFormEnemyAnimator>();
        StartCoroutine(loopAttackAndDied(npcAnimator));
    }
    IEnumerator loopAttackAndDied(HumanFormEnemyAnimator animator)
    {
        subtitleText.text = "Get to bridge, send the message! Now!";
        for (int i = 0; i < 4; i++)
        {
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttackStartUp);
            yield return new WaitForSeconds(0.5f);
            Vector3 spawnPos = transform.position + transform.forward.normalized * 0.2f + transform.right.normalized * 0.2f;
            Vector3 dir = (transform.forward + transform.forward.normalized * 10f).normalized;
            Instantiate(npcBulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttack);
            yield return new WaitUntil(() => animator.IsCurrentAnimationDone());
        }
        subtitleText.text = "Ahh!";
        animator.BeginAnimation(HumanFormEnemyAnimationState.Dead);
    }

    public void DollyZoomToPlayer()
    {
        subtitleText.text = "";
        protagonistActor.SetActive(true);
        HumanFormEnemyAnimator protagonistAnimator = protagonistActor.GetComponent<HumanFormEnemyAnimator>();
        protagonistAnimator.BeginAnimation(HumanFormEnemyAnimationState.Idle);
    }

    public void PlayerRunToBridge()
    {
        protagonistActor.SetActive(false);
        foreach (GameObject actor in chasingActors)
        {
            actor.SetActive(true);
            actor.GetComponent<HumanFormEnemyAnimator>().BeginAnimation(HumanFormEnemyAnimationState.Walk);
            actor.GetComponent<HumanFormEnemyMotor>().MoveTo(actor.transform.position + actor.transform.forward * 10f, 2f);
        }
    }

    public void CloseDoor()
    {
        door.SetActive(true);
        door.transform.Translate(Vector3.down * 2f);
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
