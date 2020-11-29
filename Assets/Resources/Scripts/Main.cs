using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Main : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioSource SpeechSource;
    public Image SoundButtonImage;
    public Sprite SoundButtonSprite_ON;
    public Sprite SoundButtonSprite_OFF;
    public GameObject CopyrightPanel;
    bool freeze = false;
    public Image BlurPanel;
    public GameObject PlayButton;
    public Transform Drum;
    public GameObject DrumImagePrefab;
    public List<DIP> DrumImagePrefabs;
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
    public Transform PartsPool;

    public GameObject Dummy;
    public GameObject DummyDroped;

    [Space]
    public AudioClip StartSpeech;
    public AudioClip BadPoemDictorReaction_1;
    public AudioClip BadPoemDictorReaction_2;
    public AudioClip BadPoemDictorReaction_3;
    public AudioClip LetsTryPuzzleSpeech;

    void Start()
    {
        UsedDrumImages = new bool[8];
        DrumFormation(358f);

        foreach (Part p in AllParts)
        {
            PartUI part = Instantiate(PartPrefab).GetComponent<PartUI>();
            part.PartImage.sprite = p.PartSprite;
            part.PartWord = p.PartWord;
            part.transform.SetParent(PartsPool);
        }
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

            DIP dip = DrumImagePrefabs[i];
            dip.DrumImage = dip.GetComponent<Image>();
            dip.DrumImage.sprite = PuzzleComponents[rnd].DrumImageSprite;
            dip.PuzzleComponents = PuzzleComponents[rnd];

            float angle = -i * (2f * Mathf.PI / 8f);
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            dip.transform.localPosition = new Vector3(x, y, 0);
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
        Debug.Log("Exit");
        Application.OpenURL("/gameover.php");
        Application.Quit();
    }


    IEnumerator SpeechCreate(AudioClip clip)
    {
        SpeechSource.clip = clip;
        SpeechSource.Play();

        freeze = true;
        yield return new WaitForSecondsRealtime(clip.length);
        freeze = false;
    }

    public void BeginGame()
    {
        BlurPanel.gameObject.SetActive(false);
        PlayButton.SetActive(false);
        ChildAnimator.SetTrigger("ChildAnimate");

        StartCoroutine(SpeechCreate(StartSpeech));
    }

    public void RotateDrum()
    {
        if (!freeze)
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

            if (sum >= 0)
            {
                DrumImagePrefabs[nextIndex].DrumImage.color = Color.green;
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

        Dummy.SetActive(false);
        DummyDroped.SetActive(true);

        StartCoroutine(BeforePuzzleDialog());
    }

    IEnumerator BeforePuzzleDialog()
    {
        yield return new WaitForSeconds(1f);
        SpeechSource.PlayOneShot(Puzzle.PuzzleComponents.BadPoem);
        yield return new WaitForSecondsRealtime(Puzzle.PuzzleComponents.BadPoem.length);
        //yield return new WaitForSeconds(1f);
        SpeechSource.PlayOneShot(LetsTryPuzzleSpeech);
        yield return new WaitForSecondsRealtime(LetsTryPuzzleSpeech.length);
        //yield return new WaitForSeconds(1f);

        for (int i = 0; i < Puzzle.PuzzleComponents.PuzzleCorrectWords.Length; i++)
        {
            string curword = Puzzle.PuzzleComponents.PuzzleCorrectWords[i];
            for (int j = 0; j < AllParts.Length; j++)
            {
                if (AllParts[j].PartWord == curword)
                {
                    PartUI part = Instantiate(PartPrefab).GetComponent<PartUI>();
                    part.PartImage.sprite = AllParts[j].PartSprite;
                    part.PartWord = AllParts[j].PartWord;
                    part.transform.SetParent(PartsGrid);
                    break;
                }
            }
        }
        //int index = 0;
        //string word = "";
        for (int i = 0; i < Puzzle.PuzzleComponents.WholePartsCount - Puzzle.PuzzleComponents.PuzzleCorrectWords.Length; i++)
        {
            while (true)
            {
                int rnd = Random.Range(0, AllParts.Length);

                if (!Puzzle.PuzzleComponents.PuzzleCorrectWords.Contains(AllParts[rnd].PartWord))
                {
                    PartUI part = Instantiate(PartPrefab).GetComponent<PartUI>();
                    part.PartImage.sprite = AllParts[rnd].PartSprite;
                    part.PartWord = AllParts[rnd].PartWord;
                    part.transform.SetParent(PartsGrid);
                    break;
                }
            }
        }
        PartsGridAnimator.SetTrigger("UP");
    }



    // Update is called once per frame
    void Update()
    {

    }
}
