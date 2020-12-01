using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PuzzleImagePanel : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public Image PuzzleImage;
    public PuzzleParts PuzzleComponents;
    public int SpriteIndex = 0;
    public bool PuzzleIsDone = false;
    Main main;

    public void OnDrop(PointerEventData eventData)
    {
        //print("drop");

        PartImagePanel PIP = eventData.pointerDrag.GetComponent<PartImagePanel>();
        main = PIP.MainScript;

        if (PuzzleComponents.PuzzleCorrectWords.Contains(PIP.PartWord))
        {
            SpriteIndex++;
            PuzzleImage.sprite = PuzzleComponents.PuzzleImageSprites[SpriteIndex];
            Destroy(PIP.gameObject);

            if (SpriteIndex == PuzzleComponents.PuzzleCorrectWords.Length)
            {
                PuzzleIsDone = true;

                main.SpeechSource.clip = main.PuzzleIsDone;
                main.SpeechSource.Play();

                main.PuzzleAnimator.SetTrigger("DONE");

                // Стэк уплывает с экрана
                main.PartsGridAnimator.SetTrigger("DOWN");
            }
        }
        else
        {
            PIP.transform.SetParent(main.PartsGrid);
            main.SpeechSource.clip = main.WrongPuzzlePart;
            main.SpeechSource.Play();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PuzzleIsDone && !main.SpeechSource.isPlaying)
        {
            StartCoroutine(PuzzleHide());
        }
    }

    IEnumerator PuzzleHide()
    {  
        main.PuzzleAnimator.SetInteger("PuzzleScale", 0);
        yield return new WaitForSeconds(0.5f);
        main.DrumAnimator.SetInteger("DrumScale", 1);

        StartCoroutine(main.PuzzleIsCompleted());
    }
}
