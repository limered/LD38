using System;
using Assets.SystemBase;
using Assets.Systems.PlayerMovement;
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
        }

        private void UpdateFighter(FighterComponent fighter)
        {
            AnimateBoxing(fighter);
            CheckForHit(fighter);
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

        private void CheckForHit(FighterComponent fighter)
        {
            var direction = fighter.gameObject.GetComponent<PlayerComponent>().Direction;


            if (KeyCode.Mouse0.IsPressed())
            {
                RaycastHit hit;
                if (Physics.Raycast(fighter.Model.transform.position, fighter.transform.position + direction, out hit))
                {
                    var item = hit.transform.GetComponent<VictimComponent>();
                    if (item != null)
                    {
                        Debug.DrawLine(fighter.Model.transform.position, hit.point);
                    }
                }
            }
        }
    }
}
