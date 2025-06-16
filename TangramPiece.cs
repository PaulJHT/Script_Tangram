using UnityEngine;

public class TangramPiece : MonoBehaviour
{
    // Définissez ici le type spécifique de la pièce (ex : TriPetitHaut, TriPetitBas, etc.)
    public TangramPieceType pieceType;

    // Référence vers le slot sur lequel la pièce est placée (si applicable)
    [HideInInspector]
    public PlacementSlot currentSlot;
}
