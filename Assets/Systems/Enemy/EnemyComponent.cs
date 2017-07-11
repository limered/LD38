using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using UnityEngine;

namespace Assets.Systems.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CanMoveToDirectionsComponent))]
    public class EnemyComponent : GameComponent
    {
        public GameObject BulletSpawn;
        public float BulletTimer { get; set; }
    }
}