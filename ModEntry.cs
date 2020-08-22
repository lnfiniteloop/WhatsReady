using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;


namespace WhatsReady
{
    class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (e.Button.Equals(SButton.F3))
            {
                // print button presses to the console window
                this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

                IEnumerable<GameLocation> locations = Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );

                IDictionary<string, int> holdy = new Dictionary<string, int>();
                IDictionary<string, int> last_holdy = new Dictionary<string, int>();
                IDictionary<string, SObject> ready_items = new Dictionary<string, SObject>();
                SObject ready_item;

                foreach (GameLocation location in locations)
                    if (location.IsFarm)
                    {
                        foreach (SObject obj in location.objects.Values)
                        {
                            if (obj is Chest || !this.IsSpawnedWorldItem(obj))
                            {
                                if (obj.readyForHarvest)
                                {
                                    //obj.heldObject.Value.displayName - crafting machine name
                                    //this.Monitor.Log($"{obj.Name} = {obj.Type} = {obj.readyForHarvest} = {obj.heldObject.Value.displayName}");
                                    ready_item = obj.heldObject.Value;//obj.heldObject.Value.DisplayName;
                                    //save object = obj.heldObject.Value

                                    if (!holdy.ContainsKey(ready_item.name))
                                    {
                                        holdy.Add(ready_item.name, 0);
                                        ready_items.Add(ready_item.name, ready_item);
                                    }

                                    holdy[ready_item.name]++;
                                }
                            }

                        }
                        IEnumerable<KeyValuePair<Vector2, TerrainFeature>> featurePairs = location.terrainFeatures.Pairs;
                        foreach (KeyValuePair<Vector2, TerrainFeature> keyValuePair in featurePairs)
                        {
                            if (keyValuePair.Value is HoeDirt)
                            {
                                var hoeDirt = (HoeDirt)keyValuePair.Value;
                                if (hoeDirt.readyForHarvest())
                                {
                                    //showMessage("Crops readayyy");
                                    //break;

                                    SObject crop = GetItemByIndex(hoeDirt.crop.netSeedIndex);

                                    if (!holdy.ContainsKey(crop.name))
                                    {
                                        holdy.Add(crop.name, 0);
                                        ready_items.Add(crop.name, crop);
                                    }

                                    holdy[crop.name]++;
                                }
                            }
                        }
                    }

                if (holdy.Count > 0)
                {
                    this.Monitor.Log("--- Ready items ---");

                    foreach (KeyValuePair<string, SObject> item in ready_items)
                    {
                        SObject o = item.Value;
                        this.Monitor.Log($"{o.name}: {holdy[o.name]}");
                        SObject obj = new SObject(o.ParentSheetIndex, o.Stack, false, o.Price, o.quality);
                        Game1.addHUDMessage(new HUDMessage($"{o.Name} ready", holdy[o.name], true, Color.Green, obj));
                    }
                }
            }
        }

        public static void showMessage(string msg)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true);
            hudmsg.whatType = 2;
            Game1.addHUDMessage(hudmsg);
        }


        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            this.Monitor.VerboseLog($"Object list changed in {e.Location.Name}, reloading its machines.");
        }

        private bool IsSpawnedWorldItem(Item item)
        {
            return
                item is SObject obj
                && (
                    obj.IsSpawnedObject
                    || obj.isForage(null) // location argument is only used to check if it's on the beach, in which case everything is forage
                    || (!(obj is Chest) && (obj.Name == "Weeds" || obj.Name == "Stone" || obj.Name == "Twig"))
                );
        }

        private SObject GetItemByIndex(int index)
        {
            return new SObject(index, 1);
        }
    }
}
