using System.ComponentModel;
using System.Diagnostics;
using HexGeometry;

namespace MapGeneration
{
    public partial class RegionMap
    {

        public enum RegionGeneration { Random, EqualSize }
        public class Region(int level, int parent)
        {
            public int Level { get; } = level;
            public int Parent { get; } = parent;
        }

        HexGrid[] hexGrids;
        Region[][] regions;

        public int Levels => hexGrids.Length;

        public static RegionMap Create(int columns, int rows, int levels, RegionGeneration reg_gen, Random rnd) => 
            new(columns, rows, levels, reg_gen, rnd);

        public IEnumerable<Region> GetRegions(int gridLevel) => regions[gridLevel];

        public HexGrid GetGridAtLevel(int level) => hexGrids[level];

        public Region GetRegionByHex(int gridLevel, int hexIndex) => regions[gridLevel][hexIndex];

        public Region GetAncestorAtLevel(Region region, int level)
        { 
            while (region.Level > level)
            {
                region = regions[region.Level - 1][region.Parent];
            }
            return region;
        }

        public string AnalyzeRegions()
        {
            int level = hexGrids.Length - 1;
            var agg = regions[level].AggregateBy(r => GetAncestorAtLevel(r, 0), r => 0, (size, r) => size + 1).Select(r => r.Value).ToList();
            agg.Sort();

            string result = "";
            result += $"Min: {agg[0]}";
            result += $"\nQuantile 25%: {agg[(int)(agg.Count * 0.25)]}";
            result += $"\nQuantile 50%: {agg[(int)(agg.Count * 0.5)]}";
            result += $"\nQuantile 75%: {agg[(int)(agg.Count * 0.75)]}";
            result += $"\nMax: {agg[agg.Count - 1]}";

            return result ;


            //var region_to_size = agg.ToDictionary();
            //for (int i = 0; i < hexGrids[0].Size; i++)
            //{
            //    Debug.WriteLine($"{i}: {region_to_size[regions[0][i]]}");
            //}
        }

        RegionMap(int rows, int columns, int levels, RegionGeneration reg_gen, Random rnd)
        {
            hexGrids = new HexGrid[levels];
            regions = new Region[levels][];
            hexGrids[0] = HexGrid.Create(rows, columns);
            regions[0] = hexGrids[0].Hexes.Select(h => new Region(0, -1)).ToArray();

            for (int level = 1; level < levels; level++)
            {
                IParentGenerator parent_generator = reg_gen == RegionGeneration.Random ? 
                    new RandomParentGenerator(this, level, rnd) : 
                    new EqualSizeParentGenerator(this, level, rnd);

                hexGrids[level] = HexGrid.CreateChild(hexGrids[level - 1]);
                regions[level] = new Region[hexGrids[level].Size];

                foreach (int index in permutation(hexGrids[level].Size, rnd))
                {
                    HexGrid.Hex hex = hexGrids[level].GetHex(index);
                    int parent = parent_generator.GetParent(hex);
                    regions[level][index] = new(level, parent);
                }
            }
        }

        static IEnumerable<int> permutation(int length, Random rnd)
        {
            int[] indices = Enumerable.Range(0, length).ToArray();

            while (length > 0)
            {
                int random_i = rnd.Next(length);
                int result = indices[random_i];
                indices[random_i] = indices[length - 1];
                length -= 1;
                yield return result;
            }           
        }
    }    
}
