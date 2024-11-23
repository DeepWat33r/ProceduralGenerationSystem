using UnityEngine;
using Room.Grid;

[CreateAssetMenu(fileName = "DecorationAsset", menuName = "Decoration Asset/Create New Decoration Asset")]
public class DecorationAsset : ScriptableObject
{
    public GameObject prefab;
    public Vector2 area;          
    public CellTag zone;           
    [Range(0, 1)]
    public float chances;          

    public bool hasAssociatedDecorations;          
    public DecorationType associatedDecorationType; 

    public DecorationType decorationType; 
}