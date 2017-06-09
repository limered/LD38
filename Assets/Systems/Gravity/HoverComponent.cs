using Assets.SystemBase;
using Assets.Utils;
using UnityEngine;
using System;

namespace Assets.Systems.Gravity
{
    [Serializable]
    public class HoverComponent : GameComponent
    {
        public float Height = 5f;
        public float currentHeight;
        public bool pushing;

        public RangeValue BounceForce = new RangeValue{Min = 1, Max = 10};
    }
}