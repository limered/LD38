using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using UnityEngine;

namespace Assets.Systems.Enemy
{
    public class SpawnNPCComponent : GameComponent
    {
        public GameObject spawnObject;
        public float spawnTimer=5;

        public float height = 10;
    }
}