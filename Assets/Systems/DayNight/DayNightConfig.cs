using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.DayNight
{
    public class DayNightConfig : GameComponent
    {
        public int DayLengthInSec = 10;
        public Vector3 RotationAxis = Vector3.forward;
    }
}