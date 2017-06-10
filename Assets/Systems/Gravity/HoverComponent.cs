using Assets.SystemBase;
using Assets.Utils;
using UnityEngine;
using System;

namespace Assets.Systems.Gravity
{
    [Serializable]
    public class HoverComponent : GameComponent
    {
        public RangeValue BounceForce = new RangeValue{Min = 1, Max = 10};
        public float Height = 5f;
        public bool onlyPushUp = true;


        [Header(DebugUtils.DefaultDebugHeader)]
        public float currentHeight;
        public bool pushing;
    }
}