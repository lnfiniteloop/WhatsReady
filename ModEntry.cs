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
using StardewValley.Buildings;

namespace WhatsReady
{
    class ModEntry : Mod
    {
        private List<int> animalDroppingsIds = new List<int>() { 107, 436, 438, 437, 439, 440, 442, 444, 446, 174, 182, 180, 176 };
        private List<int> animalDroppingCatIds = new List<int>() { -5, -6, -14, -17, -18 };
        private ModConfig cfg;
        private IDictionary<string, int> holdy = new Dictionary<string, int>();
        private IDictionary<string, int> holdy_previous = new Dictionary<string, int>();
        private IDictionary<string, SObject> ready_items = new Dictionary<string, SObject>();
        private SObject ready_item;

        public override void Entry(IModHelper helper)
        {
            this.cfg = this.Helper.ReadConfig<ModConfig>();

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.TimeChanged += this.OnTimeChange;

        }

        private void OnTimeChange(object sender, TimeChangedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            holdy.Clear();
            ready_items.Clear();

            //Monitor.Log($"{e.NewTime.ToString()} time changed.", LogLevel.Debug);

            foreach (GameLocation location in GetGameLocations())
            {
                if ((location is FarmCave && cfg.showCaveItems) || (!(location is FarmCave) && !cfg.showCaveItems))
                {
                    if (location.IsFarm || location.IsGreenhouse)
                    {
                        OverlaidDictionary.ValuesCollection locationObjects = GetLocationObjects(location);
                        CheckMachineItems(locationObjects);
                        CheckCrops(location);
                        if (cfg.showAnimalDroppings)
                            CheckAnimalDroppings(locationObjects);

                    }
                }
            }

            if (holdy.Count > 0)
            {
                ShowNotification();
            }

        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //if (e.Button.Equals(SButton.F5))
            //{
            //    int itemId = 430;
            //    int x, y;
            //    x = 74; y = 19;
            //    Game1.getLocationFromName("Farm").dropObject(new StardewValley.Object(itemId, 1, false, -1, 0), new Vector2(x, y) * 64f, Game1.viewport, true, (Farmer)null);
            //}

            if (e.Button.Equals(this.cfg.keyToCheck))
            {
                // print button presses to the console window
                Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

                holdy.Clear();
                holdy_previous.Clear();
                ready_items.Clear();

                foreach (GameLocation location in GetGameLocations())
                {
                    if ((location is FarmCave && cfg.showCaveItems) || (!(location is FarmCave) && !cfg.showCaveItems))
                    {
                        if (location.IsFarm || location.IsGreenhouse)
                        {
                            OverlaidDictionary.ValuesCollection locationObjects = GetLocationObjects(location);
                            CheckMachineItems(locationObjects);
                            CheckCrops(location);
                            if (cfg.showAnimalDroppings)
                                CheckAnimalDroppings(locationObjects);

                        }
                    }
                }

                if (holdy.Count > 0)
                {
                    ShowNotification();
                }
            }
        }

        private List<GameLocation> GetGameLocations()
        {
            List<GameLocation> gameLocations = new List<GameLocation>();
            foreach (BuildableGameLocation location in Game1.locations.OfType<BuildableGameLocation>())
            {
                if (location.IsFarm)
                {
                    gameLocations.Add(location);

                    foreach (Building building in location.buildings)
                    {
                        if (building.indoors.Value != null)
                            gameLocations.Add(building.indoors.Value);
                    }

                }
            }

            foreach (GameLocation location in Game1.locations)
            {
                if (location is FarmCave && cfg.showCaveItems && !gameLocations.Contains(location))
                    gameLocations.Add(location);

                if (location.IsGreenhouse && !gameLocations.Contains(location))
                    gameLocations.Add(location);
            }

            return gameLocations;
        }

        private OverlaidDictionary.ValuesCollection GetLocationObjects(GameLocation location)
        {
            return location.objects.Values;
        }

        private void CheckMachineItems(OverlaidDictionary.ValuesCollection locationObjects)
        {
            foreach (SObject obj in locationObjects)
            {
                if (obj is Chest || !IsSpawnedWorldItem(obj))
                {
                    if (obj.readyForHarvest) //machine items
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
        }

        private void CheckAnimalDroppings(OverlaidDictionary.ValuesCollection locationObjects)
        {
            List<SObject> truffles = new List<SObject>();

            foreach (SObject obj in locationObjects)
            {

                if (obj.ParentSheetIndex == 165) //autograbber
                {
                    Chest objItems = (Chest)obj.heldObject.Value;
                    foreach (SObject item in objItems.items)
                    {

                        if (animalDroppingCatIds.Contains(item.Category) || animalDroppingsIds.Contains(item.parentSheetIndex))
                        {
                            {
                                ready_item = item;

                                if (!holdy.ContainsKey(ready_item.name))
                                {
                                    holdy.Add(ready_item.name, 0);
                                    ready_items.Add(ready_item.name, ready_item);
                                }

                                holdy[ready_item.name]++;
                            }
                        }
                    }
                }

                //animal droppings
                if (animalDroppingCatIds.Contains(obj.Category) || animalDroppingsIds.Contains(obj.parentSheetIndex))
                {
                    {
                        ready_item = obj;

                        if (!holdy.ContainsKey(ready_item.name))
                        {
                            holdy.Add(ready_item.name, 0);
                            ready_items.Add(ready_item.name, ready_item);
                        }

                        holdy[ready_item.name]++;

                        if (ready_item.ParentSheetIndex == 430)
                        {
                            truffles.Add(ready_item);
                        }
                    }
                }
            }

            if (truffles.Count > 0 && cfg.harvestTrufflesToGrabbers)
                TruffleGrabber(truffles);
        }

        private void TruffleGrabber(List<SObject> truffles)
        {
            GameLocation farmLocation = null;
            List<Chest> grabbers = new List<Chest>();
            List<GameLocation> locations = new List<GameLocation>(GetGameLocations());
            
            foreach (GameLocation location in locations)
            {
                if (location is Farm)
                    farmLocation = location;

                if (location.name.Contains("Barn"))
                {
                    foreach (SObject obj in GetLocationObjects(location))
                    {

                        if (obj.ParentSheetIndex == 165) //autograbber
                        {
                            Chest objItems = (Chest)obj.heldObject.Value;
                            grabbers.Add(objItems);
                        }
                    }
                }
            }
            
            if (grabbers.Count > 0)
            {
                foreach (SObject truffle in truffles)
                {
                    foreach (Chest grabber in grabbers)
                    {
                        if (grabber.getRemainingStackSpace() > 0 || grabber.items.Contains(truffle))
                        {
                            if (grabber.items.Contains(truffle))
                            {
                                grabber.addToStack(truffle);
                            }
                            else
                            {
                                grabber.addItem(truffle);
                            }

                            farmLocation.destroyObject(truffle.tileLocation.Value, null);
                            showMessage("Harvesting truffle to autograbber");
                        }
                        else
                        {
                            this.Monitor.Log("No available grabber spots");
                        }
                    }
                }
            }
            else { this.Monitor.Log("No grabbers"); }
        }


        private void CheckCrops(GameLocation location)
        {
            IEnumerable<KeyValuePair<Vector2, TerrainFeature>> featurePairs = location.terrainFeatures.Pairs;
            foreach (KeyValuePair<Vector2, TerrainFeature> keyValuePair in featurePairs)
            {
                if (keyValuePair.Value is HoeDirt)
                {
                    var hoeDirt = (HoeDirt)keyValuePair.Value;
                    if (hoeDirt.readyForHarvest())
                    {
                        SObject crop = GetItemByIndex(hoeDirt.crop.indexOfHarvest);
                        if (cfg.showHarvestableFlowers || (!cfg.showHarvestableFlowers && crop.Category != -80))
                        {
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
        }

        private void ShowNotification()
        {
            this.Monitor.Log("--- Ready items ---");

            foreach (KeyValuePair<string, SObject> item in ready_items)
            {

                SObject o = item.Value;
                if (!holdy_previous.ContainsKey(o.name))
                {
                    holdy_previous.Add(o.name, -1);
                }

                if ((holdy_previous[o.name] < holdy[o.name]))
                {
                    Monitor.Log($"{o.name}: {holdy[o.name]}");
                    holdy_previous[o.name] = holdy[o.name];
                    //SObject obj = new SObject(o.ParentSheetIndex, o.Stack, false, o.Price, o.quality);
                    Game1.addHUDMessage(new HUDMessage(o.Name, holdy[o.name], true, Color.Green, o));
                }

            }
        }

        public static void showMessage(string msg)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true);
            hudmsg.whatType = 2;
            Game1.addHUDMessage(hudmsg);
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
