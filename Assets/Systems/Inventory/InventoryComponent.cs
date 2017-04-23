using Assets.SystemBase;
using Assets.Systems.Items;

namespace Assets.Systems.Inventory
{
    public class InventoryComponent : GameComponent
    {
        private readonly Item[] _slots = new Item[7];

        public void Add(Item item)
        {
            for (var slot = 0; slot < _slots.Length; slot++)
            {
                if (_slots[slot] == null)
                {
                    _slots[slot] = item;
                    return;
                }
            }
            //Inventory full :(
        }

        public Item[] GetSlots()
        {
            return _slots;
        }

        public void RemoveItem(int selectedSlot)
        {
            _slots[selectedSlot] = null;
        }
    }
}
