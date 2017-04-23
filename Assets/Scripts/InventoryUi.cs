using Assets.Systems.Inventory;
using UnityEngine;

namespace Assets.Scripts
{
    public class InventoryUi : MonoBehaviour
    {
        private InventoryComponent _inventory;
        private int _totalInventoryBarWidth;
        private int _selectedSlot = 0;

        private const int SlotWidth = 70;
        private const int SlotSpace = 20;
        private const int SelectedSlotBorderSize = 5;

        private float _lastScrollTime = 0f;

        void Awake()
        {
            _inventory = GetComponent<InventoryComponent>();
            var totalSlots = _inventory.GetSlots().Length;
            _totalInventoryBarWidth = totalSlots * SlotWidth + (totalSlots - 1) * SlotSpace;
        }

        void OnGUI ()
        {
            CheckSelectedSlotChanges();
            DisplayInventory();
        }

        private void CheckSelectedSlotChanges()
        {
            var d = Input.GetAxis("Mouse ScrollWheel");
            if (_lastScrollTime + .03 > Time.time)
            {
                return;
            }

            _lastScrollTime = Time.time;

            if (d > 0f)
            {
                // scroll up
                _selectedSlot--;
                if (_selectedSlot == -1)
                {
                    _selectedSlot = _inventory.GetSlots().Length - 1;
                }
            }
            else if (d < 0f)
            {
                // scroll down
                _selectedSlot++;
                if (_selectedSlot == _inventory.GetSlots().Length)
                {
                    _selectedSlot = 0;
                }
            }
        }

        private void DisplayInventory()
        {
            var slotsInInventory = _inventory.GetSlots();

            var slotPositionX = Screen.width / 2f - _totalInventoryBarWidth / 2f;
            var slotPositionY = Screen.height - SlotWidth - SlotSpace;

            for (var slotIndex = 0; slotIndex < slotsInInventory.Length; slotIndex++)
            {
                if (slotIndex == _selectedSlot)
                {
                    var selectedTexture = new Texture2D(1, 1);
                    selectedTexture.SetPixel(0, 0, Color.yellow);
                    selectedTexture.Apply();
                    GUI.skin.box.normal.background = selectedTexture;
                    const int slotLength = SlotWidth + SelectedSlotBorderSize * 2;
                    GUI.Box(
                        new Rect(slotPositionX - SelectedSlotBorderSize, slotPositionY - SelectedSlotBorderSize,
                            slotLength, slotLength), GUIContent.none);
                }

                var slot = slotsInInventory[slotIndex];
                Texture2D texture;
                if (slot != null)
                {
                    texture = Resources.Load(string.Format("Items/{0}/icon", slot.Name)) as Texture2D;
                }
                else
                {
                    texture = GetEmptySlotTexture();
                }

                GUI.skin.box.normal.background = texture;
                GUI.Box(new Rect(slotPositionX, slotPositionY, SlotWidth, SlotWidth), GUIContent.none);

                slotPositionX += SlotWidth + SlotSpace;
            }
        }

        public int GetSelectedSlot()
        {
            return _selectedSlot;
        }

        private static Texture2D GetEmptySlotTexture()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.black);
            texture.Apply();
            return texture;
        }
    }
}
