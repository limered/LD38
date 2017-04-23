using Assets.Systems.Inventory;
using Assets.Systems.Items;
using UnityEngine;

namespace Assets.Scripts
{
    public class CollectByClicking : MonoBehaviour
    {
        private InventoryComponent _inventory;

        void Start ()
        {
            _inventory = GetComponent<InventoryComponent>();

        }
	
        void FixedUpdate () {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var item = hit.transform.GetComponent<ItemComponent>();
                    if (item != null)
                    {
                        Debug.DrawLine(ray.origin, hit.point);
                        item.Take();
                        _inventory.Add(item.Item);
                    }
                }
            }
        }
    }
}
