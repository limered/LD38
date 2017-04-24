using System;
using Assets.SystemBase;
using Assets.Systems.PlayerMovement;
using Assets.Utils;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public class PathSystem : GameSystem<CanFindPathComponent>
    {
        public override int Priority { get { return 20; } }

        public override void Register(CanFindPathComponent component)
        {
            Debug.LogWarning("todo");

            var nav = IoC.Resolve<NavigationGrid>();
            // var path = nav.FindPath(new Position(0, 0, CubeFace.Up), new Position(0, 2, CubeFace.Up));
            // if(path != null) Debug.Log("found path: "+path.Count);
            // else Debug.Log("CANNOT found path");
        }
    }
}