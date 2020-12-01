using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioSource SpeechSource;
    public Image SoundButtonImage;
    public Sprite SoundButtonSprite_ON;
    public Sprite SoundButtonSprite_OFF;
    public GameObject CopyrightPanel;
    public bool freeze;
    public Image BlurPanel;
    public GameObject PlayButton;
    public Transform Drum;
    public List<DIP> DrumImagePrefabs;
    public List<Animator> StarPrefabs;
    bool[] UsedDrumImages;
    int startIndex;
    public PuzzleImagePanel Puzzle;

    public Animator ChildAnimator;
    public Animator DrumAnimator;
    public Animator PuzzleAnimator;
    public Animator PartsGridAnimator;

    public PuzzleParts[] PuzzleComponents;
    public Part[] AllParts;
    public GameObject PartPrefab;
    public Transform PartsGrid;
    //public Transform PartsPool;

    public GameObject Dummy;
    public GameObject DummyDroped;
    public GameObject Cup;

    public GameObject RetryGamePanel;

    [Space]
    public AudioClip StartSpeech;
    //public AudioClip BadPoemDictorReaction_1;
    //public AudioClip BadPoemDictorReaction_2;
    //public AudioClip BadPoemDictorReaction_3;
    public AudioClip LetsTryPuzzleSpeech;
    public AudioClip WrongPuzzlePart;
    public AudioClip PuzzleIsDone;
    public AudioClip GameIsFinished;

    public AudioClip MusicTheme;
    public AudioClip MusicVictory;

    public Canvas canvas;

    void Start()
    {
        UsedDrumImages = new bool[8];
        DrumFormation(358f);

        MusicSource.clip = MusicTheme;
        MusicSource.Play();
    }

    void DrumFormation(float radius)
    {
        int rnd;
        bool[] usedRnd = new bool[8];

        for (int i = 0; i < 8; i++)
        {
            while (true)
            {
                rnd = Random.Range(0, 8);
                if (usedRnd[rnd] == false)
                {
                    usedRnd[rnd] = true;
                    break;
                }
            }

            Animator star = StarPrefabs[i];
            star.gameObject.SetActive(false);

            DIP dip = DrumImagePrefabs[i];
            dip.DrumImage = dip.GetComponent<Image>();
            dip.DrumImage.sprite = PuzzleComponents[rnd].DrumImageSprite;
            dip.PuzzleComponents = PuzzleComponents[rnd];
            dip.StarAnimator = star;

            float angle = -i * (2f * Mathf.PI / 8f);
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            dip.transform.localPosition = new Vector3(x, y, 0);

            x = Mathf.Cos(angle) * radius / 2.15f;
            y = Mathf.Sin(angle) * radius / 2.15f;

            star.transform.localPosition = new Vector3(x, y, 0);
        }
    }

    public void SoundSwitcher()
    {
        if (MusicSource.volume == 0)
        {
            SoundButtonImage.sprite = SoundButtonSprite_OFF;
            MusicSource.volume = 0.3f;
            SpeechSource.volume = 1;
        }
        else
        {
            SoundButtonImage.sprite = SoundButtonSprite_ON;
            MusicSource.volume = 0;
            SpeechSource.volume = 0;
        }
    }

    public void Copyright()
    {
        if (CopyrightPanel.activeSelf)
        {
            CopyrightPanel.SetActive(false);
        }
        else
        {
            CopyrightPanel.SetActive(true);
        }
    }

    public void GoToFullScreen()
    {
        Debug.Log(Screen.fullScreen);
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void ExitApplication()
    {
        Application.OpenURL("/gameover.php");
    }


    IEnumerator PlayStartSpeech()
    {
        SpeechSource.clip = StartSpeech;
        SpeechSource.Play();

        yield return new WaitForSecondsRealtime(StartSpeech.length + 0.5f);

        freeze = false;
    }

    public void BeginGame()
    {
        BlurPanel.enabled = false;
        PlayButton.SetActive(false);
        ChildAnimator.SetTrigger("ChildAnimate");

        freeze = true;
        StartCoroutine(PlayStartSpeech());
    }

    public void RotateDrum()
    {
        if (freeze == false)
        {
            int rnd;
            if (UsedDrumImages.All(x => x))
            {
                print("Начни игру заново");
                return;
            }

            while (true)
            {
                rnd = Random.Range(0, 8);
                if (UsedDrumImages[rnd] == false)
                {
                    break;
                }
            }

            StartCoroutine(DrumRotating(rnd));
        }
    }

    IEnumerator DrumRotating(int nextIndex)
    {
        freeze = true;
        float startRotation = Drum.rotation.eulerAngles.z;
        int step = 1;
        int sum = 0;
        int currentCircleCount = 0;

        while (true)
        {
            Drum.Rotate(0, 0, -step);
            sum += step;
            foreach (DIP dip in DrumImagePrefabs)
            {
                dip.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -Drum.rotation.eulerAngles.z));
            }
            foreach (Animator star in StarPrefabs)
            {
                star.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -Drum.rotation.eulerAngles.z));
            }

            if (sum % 360 == 0)
            {
                currentCircleCount++;
                if (currentCircleCount == 1)
                {
                    Drum.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.RoundToInt(startRotation)));
                    break;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }

        if (nextIndex - startIndex > 0) sum = -(360 - (nextIndex - startIndex) * 45);
        else sum = (nextIndex - startIndex) * 45;

        while (true)
        {
            Drum.Rotate(0, 0, -step);
            sum += step;
            foreach (DIP dip in DrumImagePrefabs)
            {
                dip.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -Drum.rotation.eulerAngles.z));
            }
            foreach (Animator star in StarPrefabs)
            {
                star.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -Drum.rotation.eulerAngles.z));
            }

            if (sum >= 0)
            {
                startIndex = nextIndex;
                UsedDrumImages[nextIndex] = true;

                Puzzle.PuzzleComponents = DrumImagePrefabs[nextIndex].PuzzleComponents;
                Puzzle.PuzzleImage.sprite = Puzzle.PuzzleComponents.PuzzleImageSprites[0];
                break;
            }
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(PuzzleShow());
    }

    IEnumerator PuzzleShow()
    {
        yield return new WaitForSeconds(0.5f);
        PuzzleAnimator.gameObject.SetActive(true);

        DrumAnimator.SetInteger("DrumScale", 0);

        yield return new WaitForSeconds(0.5f);

        PuzzleAnimator.SetInteger("PuzzleScale", 1);

        StartCoroutine(DummyDrop());
    }

    IEnumerator DummyDrop()
    {
        yield return new WaitForSeconds(2f);

        if (Dummy.activeSelf)
        {
            Dummy.SetActive(false);
            DummyDroped.SetActive(true);
        }

        StartCoroutine(BeforePuzzleDialog());
    }

    IEnumerator BeforePuzzleDialog()
    {
        yield return new WaitForSeconds(1f);
        SpeechSource.PlayOneShot(Puzzle.PuzzleComponents.BadPoem);
        yield return new WaitForSeconds(Puzzle.PuzzleComponents.BadPoem.length + 1f);
        SpeechSource.PlayOneShot(LetsTryPuzzleSpeech);

        // Добавляем в стэк части с правильными словами
        for (int i = 0; i < Puzzle.PuzzleComponents.PuzzleCorrectWords.Length; i++)
        {
            string curword = Puzzle.PuzzleComponents.PuzzleCorrectWords[i];
            for (int j = 0; j < AllParts.Length; j++)
            {
                if (AllParts[j].PartWord == curword)
                {
                    PartImagePanel part = Instantiate(PartPrefab).GetComponent<PartImagePanel>();
                    part.PartImage.sprite = AllParts[j].PartSprite;
                    part.PartWord = AllParts[j].PartWord;
                    part.transform.SetParent(PartsGrid);
                    part.transform.localScale = Vector3.one;
                    part.MainScript = this;
                    break;
                }
            }
        }

        // Добавляем в стэк части с неправильными словами
        int UncorrectWordsCount = Puzzle.PuzzleComponents.WholePartsCount - Puzzle.PuzzleComponents.PuzzleCorrectWords.Length;
        int[] usedIndexes = new int[UncorrectWordsCount];
        for (int i = 0; i < usedIndexes.Length; i++) usedIndexes[i] = -1;
        for (int i = 0; i < UncorrectWordsCount; i++)
        {
            while (true)
            {
                int rnd = Random.Range(0, AllParts.Length);
                if (!usedIndexes.Contains(rnd))
                {
                    if (!Puzzle.PuzzleComponents.PuzzleCorrectWords.Contains(AllParts[rnd].PartWord))
                    {
                        PartImagePanel part = Instantiate(PartPrefab).GetComponent<PartImagePanel>();
                        part.PartImage.sprite = AllParts[rnd].PartSprite;
                        part.PartWord = AllParts[rnd].PartWord;
                        part.transform.SetParent(PartsGrid);
                        part.transform.localScale = Vector3.one;
                        part.MainScript = this;

                        usedIndexes[i] = rnd;
                        break;
                    }
                }
            }
        }

        // Перемешиваем дочерние объекты стэка
        for (int i = 0; i < 30; i++)
        {
            int rndIndex = Random.Range(0, PartsGrid.childCount);
            PartsGrid.GetChild(rndIndex).SetAsFirstSibling();
        }

        yield return new WaitForSeconds(5f);
        // Выплывает стэк на экран
        PartsGridAnimator.SetTrigger("UP");

        yield return new WaitForSeconds(LetsTryPuzzleSpeech.length - 5f);
    }


    public IEnumerator PuzzleIsCompleted()
    {
        yield return new WaitForSeconds(1f);

        foreach (PartImagePanel part in PartsGrid.GetComponentsInChildren<PartImagePanel>())
        {
            Destroy(part.gameObject);
        }
        
        Puzzle.PuzzleIsDone = false;
        Puzzle.SpriteIndex = 0;

        DrumImagePrefabs[startIndex].DrumImage.color = Color.green;
        DrumImagePrefabs[startIndex].StarAnimator.gameObject.SetActive(true);
        DrumImagePrefabs[startIndex].StarAnimator.SetTrigger("STAR");

        bool VICTORY = true;
        foreach (Animator star in StarPrefabs)
        {
            if (!star.gameObject.activeSelf)
            {
                VICTORY = false;
                break;
            }
        }

        if (VICTORY)
        {
            MusicSource.clip = MusicTheme;
            MusicSource.Play();

            DrumAnimator.SetTrigger("VICTORY");
            SpeechSource.PlayOneShot(GameIsFinished);
            yield return new WaitForSeconds(GameIsFinished.length);
            DrumAnimator.SetInteger("DrumScale", 0);
            yield return new WaitForSeconds(1f);
            Cup.SetActive(true);
            yield return new WaitForSeconds(2f);
            RetryGamePanel.SetActive(true);
        }
        freeze = false;
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(0);
    }
}
