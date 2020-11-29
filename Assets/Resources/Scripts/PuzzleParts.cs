using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Puzzle", menuName = "Puzzles")]
public class PuzzleParts : ScriptableObject
{
    public Sprite DrumImageSprite;
    public Sprite[] PuzzleImageSprites;
    public string[] PuzzleCorrectWords;
    public int WholePartsCount;
    public AudioClip BadPoem;
}
