using System.Collections.Generic;
using Assets.SystemBase;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [RequireComponent(typeof(Rigidbody))]
    public class CanMoveOnPathComponent : GameComponent
    {
        public readonly SerialDisposable MovingSubscription = new SerialDisposable();
        public ReactiveProperty<Position> Destination = new ReactiveProperty<Position>();
        public ReactiveProperty<List<Position>> CurrentPath = new ReactiveProperty<List<Position>>();
        public Vector3ReactiveProperty CurrentDirection = new Vector3ReactiveProperty();
        public FloatReactiveProperty Speed = new FloatReactiveProperty(10f);
        public readonly SerialDisposable CurrentMovement = new SerialDisposable();
        public readonly BoolReactiveProperty Standing = new BoolReactiveProperty(true);
    }
}