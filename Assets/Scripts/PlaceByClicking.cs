using Assets.Systems.Inventory;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlaceByClicking : MonoBehaviour
    {
        private InventoryUi _inventoryUi;
        private InventoryComponent _inventory;

        void Start()
        {
            _inventoryUi = GetComponent<InventoryUi>();
            _inventory = GetComponent<InventoryComponent>();
        }

        void Update()
        {
            if (!Input.GetMouseButton(1))
            {
                return;
            }

            var selectedSlot = _inventoryUi.GetSelectedSlot();
            var selectedItem = _inventory.GetSlots()[selectedSlot];
            if (selectedItem == null)
            {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.DrawLine(ray.origin, hit.point);
                Instantiate(selectedItem.Prefab, hit.point + new Vector3(0, selectedItem.Prefab.transform.localScale.y/2, 0), Quaternion.identity);
                _inventory.RemoveItem(selectedSlot);
            }
        }
    }
}
