using Room.Grid;
using UnityEngine;

[CreateAssetMenu(fileName = "DecorationAsset", menuName = "Decoration Asset/Create New Decoration Asset")]
public class DecorationAsset : ScriptableObject
{
    public GameObject prefab;
    public Vector2 area;           // Size of the decoration in cells (width, height)
    public CellTag zone;           // Zone where this decoration can be placed (Inner or Outer)

    [Range(0, 1)]
    public float chances;          // Chance of this decoration being placed
}