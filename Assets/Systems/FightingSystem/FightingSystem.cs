using System;
using Assets.SystemBase;
using Assets.Utils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
            comp.OnCollisionStayAsObservable().Subscribe(collision => CollisionEnter(comp, collision)).AddTo(comp);
        }

        private void CollisionEnter(FighterComponent comp, Collision collision)
        {
            if (KeyCode.Mouse0.IsPressed())
            {
                var victimComponent = collision.gameObject.GetComponent<VictimComponent>();
                if (victimComponent == null)
                {
                    return;
                }

                victimComponent.KnockBack(collision.transform.position - comp.transform.position);
            }
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
