using System.ComponentModel;
using System.Diagnostics;
using HexGeometry;

namespace MapGeneration
{
    public class RegionMap
    {
        public class Region(int level, int parent)
        {
            public int Level { get; } = level;
            public int Parent { get; } = parent;
        }

        HexGrid[] hexGrids;
        Region[][] regions;

        public int Levels => hexGrids.Length;

        public static RegionMap Create(int columns, int rows, int levels, Random rnd) => new(columns, rows, levels, rnd);

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

        RegionMap(int rows, int columns, int levels, Random rnd)
        {
            hexGrids = new HexGrid[levels];
            regions = new Region[levels][];
            hexGrids[0] = HexGrid.Create(rows, columns);
            regions[0] = hexGrids[0].Hexes.Select(h => new Region(0, -1)).ToArray();

            IParentGenerator parent_generator = new EqualSizeParentGenerator(this, rnd);

            for (int level = 1; level < levels; level++)
            {
                hexGrids[level] = HexGrid.CreateChild(hexGrids[level - 1]);
                regions[level] = new Region[hexGrids[level].Size];

                foreach (int index in permutation(hexGrids[level].Size, rnd))
                {
                    HexGrid.Hex hex = hexGrids[level].GetHex(index);
                    int parent = parent_generator.GetParent(level, hex);
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

    interface IParentGenerator
    {
        int GetParent(int level, HexGrid.Hex hex);
    }

    class RandomParentGenerator : IParentGenerator 
    {
        RegionMap map;
        Random rnd;
        public RandomParentGenerator(RegionMap map, Random rnd)
        {
            this.map = map;
            this.rnd = rnd;
        }

        public int GetParent(int level, HexGrid.Hex hex)
        {
            if (hex.Parent > -1)
            {
                return hex.Parent;
            }

            HexGrid grid = map.GetGridAtLevel(level);
            int[] candidates = hex.Neighbors.Where(n => n > -1).Select(n => grid.GetHex(n).Parent).Where(p => p > -1).ToArray();
            return candidates[rnd.Next(candidates.Length)];
        }
    }

    class EqualSizeParentGenerator : IParentGenerator 
    {
        RegionMap map;
        Random rnd;
        Dictionary<HexGrid.Hex, int> hex_to_size = new Dictionary<HexGrid.Hex, int>();
        public EqualSizeParentGenerator(RegionMap map, Random rnd)
        {
            this.map = map;
            this.rnd = rnd;
        }

        public int GetParent(int level, HexGrid.Hex hex)
        {
            if (hex.Parent > -1)
            {
                return hex.Parent;
            }

            HexGrid grid = map.GetGridAtLevel(level);
            HexGrid parent_grid = map.GetGridAtLevel(level - 1);

            (int, int)[] candidates =
                hex.Neighbors
                .Where(n => n > -1)
                .Select(n => grid.GetHex(n).Parent)
                .Where(p => p > -1)
                .Select(i => (i, hex_to_size.GetValueOrDefault(parent_grid.GetHex(i))))
                .OrderByDescending(t => t.Item2)
                .ToArray();

            candidates = candidates.Where(t => t.Item2 == candidates[0].Item2).ToArray();

            int index = rnd.Next(candidates.Length);

            HexGrid.Hex parent = parent_grid.GetHex(index);
            if (hex_to_size.ContainsKey(parent_grid.GetHex(index)))
            {
                hex_to_size[parent] += 1;
            }
            else
            {
                hex_to_size[parent] = 1;
            }
            
            return candidates[index].Item1;

        }
    }
}
