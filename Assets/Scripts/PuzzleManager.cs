using UnityEngine;
using System;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [Header("Все куски пазла")]
    [SerializeField] private PuzzlePiece[] pieces;

    [Header("Опциональная 7‑я подсказка")]
    [SerializeField] private HintData seventhHint;

    [Header("Цель паззла")]
    [SerializeField] private GoalData goalData;
    [SerializeField] private GoalPanel goalPanel;

    [Header("Объекты при завершении пазла")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField, Tooltip("Задержка после закрытия последней подсказки перед активацией объектов")]
    private float toggleDelay = 0f;

    public int RoomIndex { get; set; }
    public bool IsInteractable { get; private set; }
    public int PlacedCount { get; private set; }
    public int TotalPieces => pieces?.Length ?? 0;

    private bool puzzleCompleted;
    public event Action OnPuzzleCompleted;

    private void Awake()
    {
        foreach (var piece in pieces)
            if (piece != null) piece.Manager = this;
    }

    public IEnumerator PlayPuzzle()
    {
        if (goalPanel != null && goalData != null)
        {
            goalPanel.Show(goalData);
            yield return goalPanel.WaitUntilClosed();
        }

        PlacedCount = 0;
        IsInteractable = true;
        puzzleCompleted = false;

        yield return new WaitUntil(() => puzzleCompleted);
    }

    public virtual void OnPiecePlaced(PuzzlePiece piece)
    {
        if (!IsInteractable || piece == null) return;

        PlacedCount++;

        if (PlacedCount < TotalPieces)
        {
            if (piece.PieceHint != null)
                HintManager.Instance.ShowHint(piece.PieceHint, HintManager.HintButtonType.Close);
            return;
        }

        if (seventhHint == null)
        {
            StartCoroutine(FinalSixthNextAndComplete(piece.PieceHint));
        }
        else
        {
            StartCoroutine(FinalSixthCloseThenSeventhNextAndComplete(piece.PieceHint, seventhHint));
        }
    }

    private IEnumerator FinalSixthNextAndComplete(HintData sixthAsNext)
    {
        if (sixthAsNext != null)
        {
            HintManager.Instance.ShowHint(sixthAsNext, HintManager.HintButtonType.Next);
            yield return HintManager.Instance.WaitUntilClosed();
        }
        
        yield return ActivateObjectsAndComplete();
    }

    private IEnumerator FinalSixthCloseThenSeventhNextAndComplete(HintData sixthClose, HintData seventhNext)
    {
        if (sixthClose != null)
        {
            HintManager.Instance.ShowHint(sixthClose, HintManager.HintButtonType.Close);
            yield return HintManager.Instance.WaitUntilClosed();
        }

        if (seventhNext != null)
        {
            HintManager.Instance.ShowHint(seventhNext, HintManager.HintButtonType.Next);
            yield return HintManager.Instance.WaitUntilClosed();
        }

        yield return ActivateObjectsAndComplete();
    }

    private IEnumerator ActivateObjectsAndComplete()
    {
        if (toggleDelay > 0)
        {
            yield return new WaitForSeconds(toggleDelay);
        }

        ToggleObjects();

        puzzleCompleted = true;
        IsInteractable = false;
        OnPuzzleCompleted?.Invoke();
    }

    private void ToggleObjects()
    {
        if (objectsToDisable != null)
        {
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (objectsToEnable != null)
        {
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}