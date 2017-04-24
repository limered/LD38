using System;
using Assets.SystemBase;
using Assets.Systems.PlayerMovement;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public class PathSystem : GameSystem<CanFindPathComponent>
    {
        public override int Priority { get { return 20; } }

        public override void Register(CanFindPathComponent component)
        {
            Debug.LogWarning("todo");
        }
    }
}