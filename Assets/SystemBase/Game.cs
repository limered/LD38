using Assets.Utils;
using System;
using System.Collections.Generic;
using Assets.Systems.Camera;
using Assets.Systems.DayNight;
using Assets.Systems.Enemy;
using Assets.Systems.Example;
using Assets.Systems.FightingSystem;
using Assets.Systems.Gravity;
using Assets.Systems.PlayerMovement;
using UnityEngine;
using UnityEngine.UI;
using Assets.Systems.Pathfinding;

namespace Assets.SystemBase
{
    public class Game : MonoBehaviour, IGameSystem
    {
        public Text DebugText;
        private readonly List<IGameSystem> _gameSystems = new List<IGameSystem>();
        private readonly Dictionary<Type, List<IGameSystem>> _systemToComponentMapper = new Dictionary<Type, List<IGameSystem>>();
        public int Priority { get { return -1; } }
        public Type[] ComponentsToRegister { get { return null; } }

        public void Init()
        {
            MapAllSystemsComponents();
        }

        public void RegisterComponent(IGameComponent component)
        {
            Debug.Log("register component: "+component.GetType());
            List<IGameSystem> systemsToRegisterTo;
            if (!_systemToComponentMapper.TryGetValue(component.GetType(), out systemsToRegisterTo)) return;

            foreach (var system in systemsToRegisterTo)
            {
                system.RegisterComponent(component);
            }
        }

        private void Awake()
        {
            IoC.RegisterSingleton(this);

            #region System Registration

            RegisterSystem(new PlayerMovementSystem());//1
            RegisterSystem(new KameraSystem());//2
            RegisterSystem(new GravitySystem());//3
            RegisterSystem(new DayNightSystem()); // 5
            RegisterSystem(new FightingSystem()); // 6

            RegisterSystem(new FunnyMovementSystem()); // 10

            RegisterSystem(new EnemySystem());//12

            RegisterSystem(new CubalPositioningSystem()); // 20

            #endregion System Registration

            Init();
        }

        private void MapAllSystemsComponents()
        {
            _gameSystems.Sort((system, gameSystem) => system.Priority - gameSystem.Priority);

            foreach (var system in _gameSystems)
            {
                foreach (var componentType in system.ComponentsToRegister)
                {
                    MapSystemToComponent(system, componentType);
                }

                system.Init();
            }
        }

        private void MapSystemToComponent(IGameSystem system, Type componentType)
        {
            if (!_systemToComponentMapper.ContainsKey(componentType))
            {
                _systemToComponentMapper.Add(componentType, new List<IGameSystem>());
            }
            _systemToComponentMapper[componentType].Add(system);
        }

        private void RegisterSystem(IGameSystem system)
        {
            _gameSystems.Add(system);
        }

        public void TellDebug(string text)
        {
            if (DebugText != null)
            {
                DebugText.text = text;
            }
        }
    }
}
