using UnityEngine;
using Room.Grid;

[CreateAssetMenu(fileName = "DecorationAsset", menuName = "Decoration Asset/Create New Decoration Asset")]
public class DecorationAsset : ScriptableObject
{
    public GameObject prefab;
    public Vector2 area;            // Size of the decoration in cells (width, height)
    public CellTag zone;            // Zone where this decoration can be placed

    [Range(0, 1)]
    public float chances;           // Chance of this decoration being placed

    public bool hasAssociatedDecorations;           // Does this decoration have associated decorations?
    public DecorationType associatedDecorationType; // Type of associated decoration

    public DecorationType decorationType; // The type of this decoration
}