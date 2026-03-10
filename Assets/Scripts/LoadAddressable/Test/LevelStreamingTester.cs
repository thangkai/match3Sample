using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class LevelStreamingTester : MonoBehaviour
{
    public Image targetImage;

    async void Start()
    {
        Debug.Log("Init streaming manager");

        await LevelStreamingManager<ImageLevelData>.Instance.Initialize(1);

        ShowCurrentLevel();
    }

    void ShowCurrentLevel()
    {
        var level = LevelStreamingManager<ImageLevelData>.Instance.CurrentLevelData;

        targetImage.sprite = level.levelSprite;

        Debug.Log("Show Level: " + level.LevelIndex);
    }

    public async void NextLevel()
    {
        await LevelStreamingManager<ImageLevelData>.Instance.NextLevel();

        ShowCurrentLevel();
    }
}