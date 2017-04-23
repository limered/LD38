using Assets.SystemBase;
using UnityEngine;

namespace Assets.Systems.Items
{
    public class ItemComponent : GameComponent
    {
        public Item Item;

        public GameObject GameObject
        {
            get { return transform.gameObject; }
        }

        public void Take()
        {
            Destroy(gameObject);
        }

        public void Place()
        {
            Instantiate(Item.Prefab);
        }
    }
}