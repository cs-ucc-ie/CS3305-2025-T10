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
    [SerializeField] private GameObject fightingScene1Actor;
    [SerializeField] private GameObject fightingScene2Actor;
    [SerializeField] private GameObject dieActor;
    [SerializeField] private GameObject npcActor;
    [SerializeField] private GameObject npcBulletPrefab;
    [SerializeField] private GameObject protagonistActor1;

    void OnDisable()
    {
        gameManager.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
           gameManager = FindFirstObjectByType<GameManager>().gameObject;
        gameManager.SetActive(false);
        middleText.text = "";
        subtitleText.text = "";
        blackScreen.SetActive(true);
        dieActor.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartFirstFightingCam()
    {
        Debug.Log("StartFirstFightingCam called");
        middleText.text = "";
        blackScreen.SetActive(false);
        fightingScene1Actor.SetActive(true);
    }

    public void StartSecondFightingCam()
    {
        Debug.Log("StartSecondFightingCam called");
        middleText.text = "";
        blackScreen.SetActive(false);
        dieActor.SetActive(true);
        HumanFormEnemyAnimator dieAnimator = dieActor.GetComponent<HumanFormEnemyAnimator>();
        dieAnimator.BeginAnimation(HumanFormEnemyAnimationState.Dead);
        fightingScene2Actor.SetActive(true);
    }

    public void CamTurnToNPC()
    {
        Debug.Log("CamTurnToNPC called");
        subtitleText.text = "Run! Get to bridge, send the message! Now!";
        subtitleText.enabled = true;
        HumanFormEnemyAnimator npcAnimator = npcActor.GetComponent<HumanFormEnemyAnimator>();
        StartCoroutine(loopAttackAnimation(npcAnimator));
    }

    IEnumerator loopAttackAnimation(HumanFormEnemyAnimator animator)
    {
        while (true)
        {
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttackStartUp);
            yield return new WaitForSeconds(0.5f);
            Vector3 spawnPos = transform.position + transform.forward.normalized * 0.2f + transform.right.normalized * 0.2f;
            Vector3 dir = (transform.forward + transform.forward.normalized * 10f).normalized;
            Instantiate(npcBulletPrefab, spawnPos, Quaternion.LookRotation(dir));
            animator.BeginAnimation(HumanFormEnemyAnimationState.WeaponAttack);
            yield return new WaitUntil(() => animator.IsCurrentAnimationDone());
        }
    }

    public void DollyZoomToPlayer()
    {
        subtitleText.enabled = false;
        protagonistActor1.SetActive(true);
        Debug.Log("DollyZoomToPlayer called");
        HumanFormEnemyAnimator protagonistAnimator = protagonistActor1.GetComponent<HumanFormEnemyAnimator>();
        protagonistAnimator.BeginAnimation(HumanFormEnemyAnimationState.Hurt);
    }

    public void PlayerRunToBridge()
    {
        HumanFormEnemyMotor protagonistMotor = protagonistActor1.GetComponent<HumanFormEnemyMotor>();
        HumanFormEnemyAnimator protagonistAnimator = protagonistActor1.GetComponent<HumanFormEnemyAnimator>();
        protagonistAnimator.BeginAnimation(HumanFormEnemyAnimationState.Walk);
        protagonistMotor.RotateAndMoveTo(new Vector3(10f, 0f, 0f), 2f);
        blackScreen.SetActive(false);
        Debug.Log("PlayerRunToBridge called");

    }

    public void BlackScreenSubtitle(String text)
    {
        Debug.Log("BlackScreenSubtitle called");
        blackScreen.SetActive(true);
        middleText.text = text;
        subtitleText.text = "";
    }

    public void EndCutScene()
    {
        Debug.Log("EndCutScene called");
        gameManager.SetActive(true);
        SceneManager.LoadScene("Bridge");
    }
}
