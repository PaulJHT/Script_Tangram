using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private TangramPiece lastModifiedPiece;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Enregistre la dernière pièce placée/modifiée.
    /// </summary>
    public void SetLastModifiedPiece(TangramPiece piece)
    {
        lastModifiedPiece = piece;
    }

    /// <summary>
    /// Undo de la dernière modification :
    /// - libère le slot occupé,
    /// - replace la pièce à son spawn,
    /// - réinitialise la référence currentSlot,
    /// - désélectionne lastModifiedPiece.
    /// </summary>
    public void UndoLastModification()
    {
        if (lastModifiedPiece == null)
            return;

        // 1) Libération du slot de la pièce
        if (lastModifiedPiece.currentSlot != null)
        {
            lastModifiedPiece.currentSlot.ReleasePiece();
            lastModifiedPiece.currentSlot = null;
        }

        // 2) Remise de la pièce à son spawn
        DragDrop dd = lastModifiedPiece.GetComponent<DragDrop>();
        if (dd != null)
        {
            dd.enabled = true;
            dd.ResetToSpawn();
        }

        // 3) On annule la référence
        lastModifiedPiece = null;
    }

    /// <summary>
    /// Réinitialise tout le plateau :
    /// - replace chaque pièce au spawn,
    /// - désactive leurs slots,
    /// - libère tous les slots existants.
    /// </summary>
    public void ResetBoard()
    {
        // 1) Repositionne toutes les pièces
        DragDrop[] allPieces = FindObjectsOfType<DragDrop>();
        foreach (DragDrop dd in allPieces)
        {
            // Si jamais une pièce a encore un currentSlot : on le libère ici
            TangramPiece tp = dd.GetComponent<TangramPiece>();
            if (tp != null && tp.currentSlot != null)
            {
                tp.currentSlot.ReleasePiece();
                tp.currentSlot = null;
            }

            // Replace la pièce
            dd.enabled = true;
            dd.ResetToSpawn();
        }

        // 2) Libère tous les slots (sécurité supplémentaire)
        PlacementSlot[] allSlots = FindObjectsOfType<PlacementSlot>();
        foreach (PlacementSlot slot in allSlots)
            slot.ReleasePiece();

        // 3) On annule l’historique d’Undo
        lastModifiedPiece = null;
    }
}
