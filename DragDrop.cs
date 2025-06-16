using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DragDrop : MonoBehaviour
{
    [Header("Drag & Rotation")]
    public float searchRadius    = 1f;
    public float rotationStep    = 45f;

    [Header("Layer des pièces")]
    // Dans l’Inspector, cochez uniquement le layer “Pieces”
    public LayerMask piecesLayerMask;

    Vector3    spawnPosition;
    bool       isDragging = false;
    Vector3    offset;
    TangramPiece tp;

    void Start()
    {
        spawnPosition = transform.position;
        tp = GetComponent<TangramPiece>();
    }

    void Update()
    {
        HandleRightClickRotation();
        HandleLeftClickDrag();
    }

    void HandleRightClickRotation()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero, Mathf.Infinity, piecesLayerMask);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            if (tp.currentSlot == null)
            {
                float angle    = transform.eulerAngles.z;
                float newAngle = Mathf.Repeat(angle + rotationStep, 360f);
                transform.eulerAngles = new Vector3(0, 0, newAngle);
            }
        }
    }

    void HandleLeftClickDrag()
    {
        // Début du drag
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero, Mathf.Infinity, piecesLayerMask);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // Libération du slot si la pièce y était
                if (tp.currentSlot != null)
                {
                    tp.currentSlot.ReleasePiece();
                    tp.currentSlot = null;
                }

                isDragging = true;
                Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mp.z = transform.position.z;
                offset = transform.position - mp;
            }
        }

        // Pendant le drag
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mp.z = transform.position.z;
            transform.position = mp + offset;
        }

        // Fin du drag
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            TrySnapToSlot();
        }
    }

    void TrySnapToSlot()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        PlacementSlot bestSlot = null;
        float bestDist = Mathf.Infinity;

        foreach (var c in hits)
        {
            PlacementSlot slot = c.GetComponent<PlacementSlot>();
            if (slot != null
             && tp != null
             && slot.CanPlacePiece(tp.pieceType, transform.eulerAngles.z))
            {
                Vector3 target = (slot.snapPosition != null) 
                    ? slot.snapPosition.position 
                    : slot.transform.position;
                float d = Vector3.Distance(transform.position, target);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestSlot = slot;
                }
            }
        }

        if (bestSlot != null)
        {
            bestSlot.PlacePiece(gameObject);
            GameManager.Instance?.SetLastModifiedPiece(tp);
        }
        else
        {
            // Aucun slot valide → retour au spawn
            tp.currentSlot = null;
            StartCoroutine(ReturnToSpawn());
        }
    }

    IEnumerator ReturnToSpawn()
    {
        Vector3 start = transform.position;
        float t = 0f, duration = 0.3f;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(start, spawnPosition, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = spawnPosition;
        if (tp != null) 
            tp.currentSlot = null;
    }

    /// <summary>
    /// Utilisé par GameManager pour réinitialiser la pièce immédiatement.
    /// </summary>
    public void ResetToSpawn()
    {
        StopAllCoroutines();
        transform.position = spawnPosition;
        if (tp != null) 
            tp.currentSlot = null;
    }
}
