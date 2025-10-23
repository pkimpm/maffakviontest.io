using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FinalBattleManager : MonoBehaviour
{
    [SerializeField] private ItemReceivedPanel itemPanel;
    [SerializeField] private GameObject weaponModel;
    [SerializeField] private VillainController villain;

    [Header("Puzzle Input для битвы")]
    [SerializeField] private PuzzleInput battleInput;

    [Header("Objects to toggle after battle")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Анимация исцеления статуи")]
    [SerializeField] private Animator[] animatorsToPlay;
    [SerializeField] private string animationName = "Healing";
    [SerializeField] private int animationLayer = 0;
    [SerializeField] private bool playReverse = true;
    [SerializeField] private float delayBeforeAnimation = 0.5f;

    [Header("Активация злодейки")]
    [SerializeField] private GameObject villainObject;
    [SerializeField] private float villainActivationDelay = 0.5f;

    public event Action OnBattleFinished;

    private void Awake()
    {
        if (villain != null)
            villain.OnDefeated += HandleBattleFinished;
        
        if (villainObject != null)
            villainObject.SetActive(false);
    }

    public void ActivateVillain()
    {
        StartCoroutine(ActivateVillainCoroutine());
    }

    private IEnumerator ActivateVillainCoroutine()
    {
        yield return new WaitForSeconds(villainActivationDelay);
        
        if (villainObject != null)
        {
            villainObject.SetActive(true);
        }
    }

    public void TriggerItemPhase()
    {
        if (itemPanel != null)
        {
            itemPanel.OnContinue += OnItemConfirmed;
            itemPanel.Show("Получено оружие!");
        }
        else
        {
            OnItemConfirmed();
        }
    }

    private void OnItemConfirmed()
    {
        if (itemPanel != null)
            itemPanel.OnContinue -= OnItemConfirmed;

        if (weaponModel != null) weaponModel.SetActive(true);
        if (battleInput != null) battleInput.enabled = true;
        if (villain != null) villain.EnableTargeting();
    }

    private void HandleBattleFinished()
    {
        if (battleInput != null)
            battleInput.enabled = false;

        StartCoroutine(PlayStatueHealingAnimation());
    }

    private IEnumerator PlayStatueHealingAnimation()
    {
        yield return new WaitForSeconds(delayBeforeAnimation);

        foreach (var go in objectsToEnable)
            if (go != null) go.SetActive(true);

        foreach (var go in objectsToDisable)
            if (go != null) go.SetActive(false);

        if (animatorsToPlay != null && animatorsToPlay.Length > 0)
        {
            float maxDuration = 0f;

            foreach (Animator animator in animatorsToPlay)
            {
                if (animator == null || animator.runtimeAnimatorController == null) continue;

                animator.speed = playReverse ? -1f : 1f;

                if (playReverse)
                {
                    animator.Play(animationName, animationLayer, 1f);
                }
                else
                {
                    animator.Play(animationName, animationLayer, 0f);
                }

                AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(animationLayer);
                if (clipInfos.Length > 0)
                {
                    float clipLength = clipInfos[0].clip.length;
                    if (clipLength > maxDuration)
                        maxDuration = clipLength;
                }
                else
                {
                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    {
                        if (clip.name == animationName)
                        {
                            if (clip.length > maxDuration)
                                maxDuration = clip.length;
                            break;
                        }
                    }
                }
            }

            if (maxDuration > 0)
            {
                yield return new WaitForSeconds(maxDuration);

                foreach (Animator animator in animatorsToPlay)
                {
                    if (animator != null)
                    {
                        animator.speed = 0f;
                    }
                }
            }
        }

        OnBattleFinished?.Invoke();
    }
    
    public PuzzleInput GetBattleInput()
    {
        return battleInput;
    }
}