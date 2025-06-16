using UnityEngine;

public enum TangramPieceType 
{ 
    TriPetit,  
    GrandTri,  
    MoyenTri, 
    Square, 
    Parallelogram 
}

[System.Serializable]
public class AllowedSlotEntry
{
    [Header("Type et Rotation")]
    // Le type de pièce autorisé pour cette entrée.
    public TangramPieceType pieceType;
    
    // Active la vérification de la rotation pour cette entrée.
    public bool restrictRotation = false;
    
    // Si allowMultipleRotations est false, ce champ est utilisé pour la rotation requise.
    public float requiredRotation = 0f;
    
    // Si true, permet de renseigner plusieurs angles autorisés dans le tableau ci-dessous.
    public bool allowMultipleRotations = false;
    
    // Tableaux des rotations autorisées (en degrés) si allowMultipleRotations est activé.
    public float[] allowedRotations;
    
    // Tolérance en degrés (par exemple 5°). La pièce sera acceptée si sa rotation diffère d'au plus cette valeur.
    public float rotationTolerance = 5f;
}

public class PlacementSlot : MonoBehaviour
{
    [Header("Configuration du Slot")]
    // Vous pouvez ajouter plusieurs entrées, permettant ainsi à ce slot d'accepter, par exemple,
    // des pieces Square tournées à différents angles.
    public AllowedSlotEntry[] allowedPieces;
    
    // Position de snapping optionnelle : si assignée, cette position sera utilisée pour snapper la pièce.
    public Transform snapPosition;
    
    // Indique si ce slot est déjà occupé.
    [HideInInspector]
    public bool isOccupied = false;
    
    [Header("Blocage par Emplacement")]
    // Vous pouvez définir des slots qui bloquent ce slot.
    // Si l'un d'eux est occupé, alors ce slot ne pourra pas être rempli.
    public PlacementSlot[] blockingSlots;

    /// <summary>
    /// Vérifie si le slot accepte la pièce en fonction du type et de la rotation fournie.
    /// Cela inclut la vérification de la disponibilité et des slots bloquants.
    /// </summary>
    public bool CanPlacePiece(TangramPieceType pieceType, float pieceRotation)
    {
        // Vérifications de base – slot occupé ou blocages.
        if (isOccupied)
            return false;
        if (blockingSlots != null)
        {
            foreach (PlacementSlot bs in blockingSlots)
            {
                if (bs != null && bs.isOccupied)
                    return false;
            }
        }
        // Si aucune entrée n'est définie, on refuse.
        if (allowedPieces == null || allowedPieces.Length == 0)
            return false;
        
        // Parcourt toutes les entrées autorisées.
        foreach (AllowedSlotEntry entry in allowedPieces)
        {
            if (entry.pieceType == pieceType)
            {
                if (entry.restrictRotation)
                {
                    // Si l'option "multiple rotations" est activée, on parcourt chaque angle autorisé.
                    if (entry.allowMultipleRotations && entry.allowedRotations != null && entry.allowedRotations.Length > 0)
                    {
                        foreach (float allowedAngle in entry.allowedRotations)
                        {
                            float delta = Mathf.Abs(Mathf.DeltaAngle(pieceRotation, allowedAngle));
                            if (delta <= entry.rotationTolerance)
                                return true;
                        }
                    }
                    else
                    {
                        // Sinon, on utilise uniquement requiredRotation.
                        float delta = Mathf.Abs(Mathf.DeltaAngle(pieceRotation, entry.requiredRotation));
                        if (delta <= entry.rotationTolerance)
                            return true;
                    }
                }
                else
                {
                    // Pas de vérification de rotation : on accepte.
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Version simplifiée qui ignore la rotation (si besoin de l'appeler sans vérifier la rotation).
    /// </summary>
    public bool CanPlacePiece(TangramPieceType pieceType)
    {
        return CanPlacePiece(pieceType, 0f);
    }

    /// <summary>
    /// Place la pièce dans le slot (en snapant sur snapPosition ou la position du slot)
    /// et marque ce slot comme occupé.
    /// </summary>
    public void PlacePiece(GameObject piece)
    {
        Vector3 targetPos = (snapPosition != null) ? snapPosition.position : transform.position;
        piece.transform.position = targetPos;
        isOccupied = true;
        TangramPiece tp = piece.GetComponent<TangramPiece>();
        if (tp != null)
            tp.currentSlot = this;
    }

    /// <summary>
    /// Libère le slot.
    /// </summary>
    public void ReleasePiece()
    {
        isOccupied = false;
    }
}
