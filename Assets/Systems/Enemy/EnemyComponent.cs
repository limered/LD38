using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyComponent : GameComponent
    {
        public Vector3 DirectionToMove;
        public GameObject BulletSpawn;
        public float BulletTimer { get; set; }
    }
}