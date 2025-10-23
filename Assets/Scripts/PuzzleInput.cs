using UnityEngine;

public class PuzzleInput : MonoBehaviour
{
    [Header("Слой для пазлов")]
    [SerializeField] private string puzzleLayerName = "PuzzlePieceLayer";

    [Header("Слой для битвы")]
    [SerializeField] private string battleLayerName = "BattleLayer";

    private int puzzleLayerMask;
    private int battleLayerMask;
    private PuzzlePiece activePiece;

    private void Awake()
    {
        puzzleLayerMask = LayerMask.GetMask(puzzleLayerName);
        battleLayerMask = LayerMask.GetMask(battleLayerName);
    }

    private void Update()
    {
        if (activePiece != null && activePiece.IsBusy) return;

        if (Input.GetMouseButtonDown(0)) HandleMouseDown();
        if (Input.GetMouseButtonUp(0)) HandleMouseUp();

        if (activePiece != null && Input.GetMouseButton(0))
        {
            activePiece.DragFollowMouse();
        }
    }

    private void HandleMouseDown()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitPuzzle, 100f, puzzleLayerMask))
        {
            PuzzlePiece piece = hitPuzzle.collider.GetComponent<PuzzlePiece>();
            if (piece != null && !piece.IsBusy)
            {
                activePiece = piece;
                activePiece.OnGrab();
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit hitBattle, 100f, battleLayerMask))
        {
            if (hitBattle.collider.TryGetComponent<VillainController>(out var villain))
            {
                villain.TakeHit(hitBattle.point);
                return;
            }

            if (hitBattle.collider.TryGetComponent<WeaponPickup>(out var weapon))
            {
                weapon.Pickup();
                return;
            }
        }
    }

    private void HandleMouseUp()
    {
        if (activePiece == null) return;
        activePiece.OnRelease();
        activePiece = null;
    }
}