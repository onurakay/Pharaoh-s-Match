using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Item")]
public sealed class Item : ScriptableObject
{
    //tile tipine özgü puan (ör: 2 puanlık palmiye tile'ı 3lük seride 2x3=6 puan verir)
    public int value;

    public Sprite sprite;
}
