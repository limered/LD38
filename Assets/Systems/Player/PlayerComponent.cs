using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.Player
{
    public class PlayerComponent : GameComponent
    {
        public GameObject GetPlayerGameObject()
        {
            return gameObject;
        }
    }
}
