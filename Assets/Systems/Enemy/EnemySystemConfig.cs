using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.Enemy
{
    public class EnemySystemConfig : GameComponent
    {
        public float EnemySpeed;
        public float BulletSpeed;
        public float TimeBetweenBullets;
        public GameObject BulletPrefab;
    }
}