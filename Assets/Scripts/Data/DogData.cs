using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu đại diện cho 1 chú chó đua
/// </summary>
[CreateAssetMenu(fileName = "NewDogData", menuName = "Game Data/Dog Data")]
public class DogData : ScriptableObject
{
    public int dogId = 1;
    public string breedName = "Shiba Inu";
    public Sprite dogSprite;
    public Color bibColor = Color.red;
}
