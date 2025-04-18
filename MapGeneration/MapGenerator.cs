using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGeneration
{
    public class MapGenerator
    {
        int seed = 0;
        RegionMap.RegionGeneration reg_gen = RegionMap.RegionGeneration.Random;

        public int Seed { get => seed; set { if (value != seed) { seed = value; generate_map(); } } }
        public RegionMap.RegionGeneration RegionGeneration { get => reg_gen; set { if (reg_gen != value) { reg_gen = value; generate_map(); } } }

        public RegionMap? Map { get; private set; }

        public event Action? OnGenerationComplete;

        void generate_map()
        {
            int columns = 10;
            int rows = 7;
            int levels = 5;

            Random rnd = new Random(Seed);
            Map = RegionMap.Create(columns, rows, levels, RegionGeneration, rnd);

            OnGenerationComplete?.Invoke();
        }
    }
}
