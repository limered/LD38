using Assets.Systems.ImageInput;
using Assets.Systems.RenderCamImage;
using Assets.Utils;
using System;
using System.Collections.Generic;
using Assets.Systems.Config;
using Assets.Systems.RenderFASTPoints;
using Assets.Systems.SVO;
using Assets.Systems.SVO.Wrapper;
using UnityEngine;
using UnityEngine.UI;

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
            new CppDebugger().Start();

            MapAllSystemsComponents();
        }

        public void RegisterComponent(IGameComponent component)
        {
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

            RegisterSystem(new ImageInputSystem()); //1
            RegisterSystem(new ConfigSystem()); // 2
            RegisterSystem(new SVOSystem()); //10

            RegisterSystem(new RenderCamImageSystem()); //20

            RegisterSystem(new RenderFastPointsSystem()); //30

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
