using UnityEngine;

// Parts are saved as sub-assets of this asset; original PNGs remain intact.
public sealed class DogRunSpriteSet : ScriptableObject
{
    public Sprite original;
    public Sprite body;
    public Sprite[] legs = new Sprite[4];
}
