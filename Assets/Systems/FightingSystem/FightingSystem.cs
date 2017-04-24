using System;
using System.Diagnostics;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.Systems.FightingSystem
{
    public class FightingSystem : IGameSystem
    {
        public int Priority { get { return 6; } }

        public Type[] ComponentsToRegister
        {
            get { return new[] {typeof(FighterComponent), typeof(VictimComponent)}; }
        }

        public void Init()
        {
        }

        public void RegisterComponent(IGameComponent component)
        {
            RegisterComponent(component as FighterComponent);
        }

        private void RegisterComponent(FighterComponent comp)
        {
            if (comp == null)
            {
                return;
            }

            comp.UpdateAsObservable().Subscribe(_ => UpdateFighter(comp)).AddTo(comp);
        }

        private void UpdateFighter(FighterComponent fighter)
        {
            AnimateBoxing(fighter);
        }

        private static void AnimateBoxing(FighterComponent fighter)
        {
            if (KeyCode.Mouse0.IsPressed())
            {
                fighter.Model.GetComponent<StateFrameAnimation>().ActivateState("Boxing");
            }
            else
            {
                fighter.Model.GetComponent<StateFrameAnimation>().ActivateState("IdleTorso");
            }
        }
    }
}
