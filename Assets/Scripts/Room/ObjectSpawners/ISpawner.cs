using Room.Grid;
using UnityEngine;

namespace Room.ObjectSpawners
{
    public abstract class Spawner : MonoBehaviour
    {
        public abstract void Spawn(GridManager gridManager);
    }
}