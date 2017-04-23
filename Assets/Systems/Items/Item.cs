using System;
using UnityEngine;

namespace Assets.Systems.Items
{
    [Serializable]
    public class Item
    {
        public string Name;

        public GameObject Prefab
        {
            get
            {
                return Resources.Load(string.Format("Items/{0}/prefab", Name)) as GameObject;
            } 
        }
    }
}
