using Assets.SystemBase;
using UniRx;

namespace Assets.Systems.WorldRotation
{
    public class WorldRotationConfigComponent : GameComponent
    {
        public FloatReactiveProperty RotationSpeed = new FloatReactiveProperty(10);
    }
}
