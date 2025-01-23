using System.ComponentModel;
using System.Diagnostics;
using HexGeometry;

namespace MapGeneration
{
    public class RegionMap
    {
        public struct Region(int index, int level, int parent)
        {
            public int Index { get; } = index;
            public readonly int Level { get; } = level;
            public readonly int Parent { get; } = parent;
        }

        HexGrid[] hexGrids;
        List<Region> regions;

        public static RegionMap Create(int columns, int rows, int levels, Random rnd) => new(columns, rows, levels, rnd);

        public IEnumerable<Region> Regions => regions;

        public HexGrid GetGridAtLevel(int level) => hexGrids[level];

        public Region GetRegionByHex(int gridLevel, int hexIndex)
        {
            int index =  hexIndex + hexGrids.Take(gridLevel).Sum(g => g.Rows * g.Columns);
            return regions[index];
        }

        public Region GetAncestorAtLevel(Region region, int level)
        {
            int index = region.Index;

            while (regions[index].Level > level)
            {
                index = regions[index].Parent;
            }

            return regions[index];
        }

        RegionMap(int rows, int columns, int levels, Random rnd)
        {
            hexGrids = new HexGrid[levels];
            regions = new List<Region>();
            hexGrids[0] = HexGrid.Create(rows, columns);
            regions.AddRange(hexGrids[0].Hexes.Select(h => new Region(h.Index, 0, -1)));

            for (int level = 1, regionParentCount = 0; level < levels; level++)
            {
                hexGrids[level] = HexGrid.CreateChild(hexGrids[level - 1]);

                foreach (HexGrid.Hex hex in hexGrids[level].Hexes)
                {
                    int parent = choose_parent(hexGrids[level], hex, rnd) + regionParentCount;
                    regions.Add(new(regions.Count, level, parent));
                }

                regionParentCount += hexGrids[level - 1].Rows * hexGrids[level - 1].Columns;
            }
        }

        static int choose_parent(HexGrid hexGrid, HexGrid.Hex hex, Random rnd)
        {
            if (hex.Parent > -1)
            {
                return hex.Parent;
            }

            int[] candidates = hex.Neighbors.Where(n => n > -1).Select(n => hexGrid.GetHex(n).Parent).Where(p => p > -1).ToArray();
            return candidates[rnd.Next(candidates.Length)];
        }
    }
}
