using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace WhatsReady
{
    class ModConfig
    {
        public SButton keyToCheck { get; set; }
        public bool showCaveItems { get; set; }
        public bool showAnimalDroppings { get; set; }
        public bool showHarvestableFlowers { get; set; }

        public ModConfig()
        {
            this.keyToCheck = SButton.F3;
            this.showCaveItems = false;
            this.showAnimalDroppings = true;
            this.showHarvestableFlowers = false;
    }
    }
}
