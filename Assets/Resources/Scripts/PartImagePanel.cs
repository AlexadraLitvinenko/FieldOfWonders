using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

public class PartImagePanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image PartImage;
    public string PartWord;

    public Main MainScript;
    public RectTransform rect;

    public CanvasGroup canvasgroup;

    public void OnDrag(PointerEventData eventData)
    {
        if (MainScript.Puzzle.PuzzleIsDone) return;
        //transform.position = eventData.pointerCurrentRaycast.screenPosition;
        rect.anchoredPosition += eventData.delta / MainScript.canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (MainScript.Puzzle.PuzzleIsDone) return;
        transform.SetParent(MainScript.PartsGridAnimator.transform);

        canvasgroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (MainScript.Puzzle.PuzzleIsDone) return;

        transform.SetParent(MainScript.PartsGrid);

        canvasgroup.blocksRaycasts = true;
    }
}
