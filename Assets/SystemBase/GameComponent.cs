using Assets.Utils;
using UnityEngine;

namespace Assets.SystemBase
{
    public class GameComponent : MonoBehaviour, IGameComponent
    {
        protected void Start()
        {
            RegisterToGame();

            OnStart();
        }

        protected virtual void OnStart(){}

        public void RegisterToGame()
        {
            IoC.Resolve<Game>().RegisterComponent(this);
        }
    }
}
