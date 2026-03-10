using UnityEngine;

[CreateAssetMenu(menuName = "Game/Image Level Data")]
public class ImageLevelData : ScriptableObject,ILevelData
{
    public int levelIndex;
    public Sprite levelSprite;
    public int LevelIndex => levelIndex;
}