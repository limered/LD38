using Assets.SystemBase;
using System;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.DayNight
{
    public class DayNightSystem : IGameSystem
    {
        private DayNightConfig _config;
        private readonly Vector3ReactiveProperty _sunPosition = new Vector3ReactiveProperty();
        private readonly Vector3ReactiveProperty _moonPosition = new Vector3ReactiveProperty();

        public int Priority
        {
            get { return 5; }
        }

        public Type[] ComponentsToRegister
        {
            get { return new[] { typeof(DayNightConfig), typeof(SunComponent), typeof(MoonComponent) }; }
        }

        public void Init()
        {
        }

        public void RegisterComponent(IGameComponent component)
        {
            var config = component as DayNightConfig;
            if (config != null)
            {
                _config = config;
                _config.UpdateAsObservable().Subscribe(CalculatePositions).AddTo(_config);
            }
            var sun = component as SunComponent;
            if (sun)
            {
                _sunPosition.Value = sun.transform.position;
                _sunPosition.Subscribe(pos => UpdateCelestialBody(sun, pos, "Sunlight")).AddTo(sun);
            }
            var moon = component as MoonComponent;
            if (moon)
            {
                _moonPosition.Value = moon.transform.position;
                _moonPosition.Subscribe(pos => UpdateCelestialBody(moon, pos, "Moonlight")).AddTo(moon);
            }
        }

        private void UpdateCelestialBody(Component go, Vector3 pos, string lightName)
        {
            go.transform.position = pos;
            var light = go.transform.FindChild(lightName);
            if (light)
            {
                light.forward = -go.transform.position;
            }
        }

        private void CalculatePositions(Unit _)
        {
            //if (((int) Time.timeSinceLevelLoad) % 10 == 0)
            //{
            //    _config.RotationAxis = new Vector3().RandomVector(new Vector3(-1,-1,-1), new Vector3(1,1,1));
            //}
            if (_config.DayLengthInSec == 0) return;
            var rotAxis = _config.RotationAxis.normalized;
            var anglePerSecond = 360f / _config.DayLengthInSec * Time.deltaTime;
            var quat = Quaternion.AngleAxis(anglePerSecond, rotAxis);

            var currentSunPos = _sunPosition.Value;
            var newpos = quat * currentSunPos;
            _sunPosition.SetValueAndForceNotify(newpos);

            var currentMoonPos = _moonPosition.Value;
            var newMpos = quat * currentMoonPos;
            _moonPosition.SetValueAndForceNotify(newMpos);
        }
    }
}