using System.Collections.Generic;
using Assets.SystemBase;
using UniRx;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [RequireComponent(typeof(CanFindPathComponent))]
    public class CanMoveOnPathComponent : GameComponent
    {
        public Vector3ReactiveProperty CurrentDirection = new Vector3ReactiveProperty();
        public FloatReactiveProperty Speed = new FloatReactiveProperty(10f);
        public readonly SerialDisposable CurrentMovement = new SerialDisposable();
        public readonly BoolReactiveProperty Standing = new BoolReactiveProperty(true);
    }
}