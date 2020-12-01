using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class ChildBehaviour : MonoBehaviour, IPointerClickHandler
{
    public Main main;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (main.Puzzle.PuzzleIsDone && !main.SpeechSource.isPlaying)
        {
            main.SpeechSource.clip = main.Puzzle.PuzzleComponents.BadPoem;
            main.SpeechSource.Play();
        }
    }
}
