﻿using System;
using Assets.SystemBase;
using Assets.Systems.Pathfinding;
using Assets.Systems.PlayerMovement;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Systems.Enemy
{
    public class EnemySystem : GameSystem<EnemySystemConfig, PlayerComponent, EnemyComponent>
    {
        private GameObject _player;
        private EnemySystemConfig _config;

        public override int Priority
        {
            get { return 12; }
        }

        public override void Register(PlayerComponent component)
        {
            _player = component.gameObject;
        }

        public override void Register(EnemyComponent component)
        {
            var mover = component.GetComponent<CanMoveOnPathComponent>();
            _player
            .GetComponent<TrackPositionComponent>()
            .CurrentPosition
            .Subscribe(pos => {
                mover.Destination.Value = pos;
            })
            .AddTo(component);

            component.OnTriggerStayAsObservable()
                .Subscribe(coll=>ShootForPlayer(coll, component))
                .AddTo(component);
        }

        private void ShootForPlayer(Collider coll, EnemyComponent enemy)
        {
            if (enemy.BulletTimer >= _config.TimeBetweenBullets)
            {
                PlayerComponent player;
                if (coll.gameObject.TryGetComponent(out player))
                {
                    var playerDir = player.transform.position - enemy.transform.position;
                    playerDir.Normalize();
                    SpawnBullet(playerDir, enemy.BulletSpawn.transform.position);
                }
                enemy.BulletTimer = 0;
            }
            enemy.BulletTimer += Time.deltaTime;
        }

        private void SpawnBullet(Vector3 direction, Vector3 position)
        {
            var bullet = Object.Instantiate(_config.BulletPrefab, position, Quaternion.identity);
            bullet.GetComponent<Rigidbody>().AddForce(direction * _config.BulletSpeed);
        }

        public override void Register(EnemySystemConfig component)
        {
            _config = component;
        }
    }
}
