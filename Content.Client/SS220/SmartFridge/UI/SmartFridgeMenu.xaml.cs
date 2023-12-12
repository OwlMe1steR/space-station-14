// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using System.Numerics;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Content.Shared.VendingMachines;

namespace Content.Client.SS220.SmartFridge.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class SmartFridgeMenu : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public event Action<ItemList.ItemListSelectedEventArgs>? OnItemSelected;
        public event Action<string>? OnSearchChanged;

        public SmartFridgeMenu()
        {
            MinSize = new Vector2(250, 150); // Corvax-Resize
			SetSize = new Vector2(450, 150); // Corvax-Resize
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            SearchBar.OnTextChanged += _ =>
            {
                OnSearchChanged?.Invoke(SearchBar.Text);
            };

            SmartFridgeContents.OnItemSelected += args =>
            {
                OnItemSelected?.Invoke(args);
            };
        }

        /// <summary>
        /// Populates the list of available items on the vending machine interface
        /// and sets icons based on their prototypes
        /// </summary>
        public void Populate(List<VendingMachineInventoryEntry> inventory, out List<int> filteredInventory,  string? filter = null)
        {

            filteredInventory = new();

            if (inventory.Count == 0)
            {
                SmartFridgeContents.Clear();
                var outOfStockText = Loc.GetString("vending-machine-component-try-eject-out-of-stock");
                SmartFridgeContents.AddItem(outOfStockText);
                SetSizeAfterUpdate(outOfStockText.Length, SmartFridgeContents.Count);
                return;
            }

            while (inventory.Count != SmartFridgeContents.Count)
            {
                if (inventory.Count > SmartFridgeContents.Count)
                    SmartFridgeContents.AddItem(string.Empty);
                else
                    SmartFridgeContents.RemoveAt(SmartFridgeContents.Count - 1);
            }

            var longestEntry = string.Empty;
            var spriteSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpriteSystem>();

            var filterCount = 0;
            for (var i = 0; i < inventory.Count; i++)
            {
                var entry = inventory[i];
                var smartFridgeItem = SmartFridgeContents[i - filterCount];
                smartFridgeItem.Text = string.Empty;
                smartFridgeItem.Icon = null;

                var itemName = entry.ID;
                Texture? icon = null;
                if (_prototypeManager.TryIndex<EntityPrototype>(entry.ID, out var prototype))
                {
                    itemName = prototype.Name;
                    icon = spriteSystem.GetPrototypeIcon(prototype).Default;
                }

                // search filter
                if (!string.IsNullOrEmpty(filter) &&
                    !itemName.ToLowerInvariant().Contains(filter.Trim().ToLowerInvariant()))
                {
                    SmartFridgeContents.Remove(smartFridgeItem);
                    filterCount++;
                    continue;
                }

                if (itemName.Length > longestEntry.Length)
                    longestEntry = itemName;

                smartFridgeItem.Text = $"{itemName} [{entry.Amount}]";
                smartFridgeItem.Icon = icon;
                filteredInventory.Add(i);
            }

            SetSizeAfterUpdate(longestEntry.Length, inventory.Count);
        }

        private void SetSizeAfterUpdate(int longestEntryLength, int contentCount)
        {
            SetSize = new Vector2(Math.Clamp((longestEntryLength + 2) * 12, 250, 300),
                Math.Clamp(contentCount * 50, 150, 350));
        }
    }
}