using HexGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGeneration
{
    public partial class RegionMap
    {
        interface IParentGenerator
        {
            int GetParent(HexGrid.Hex hex);
        }

        class RandomParentGenerator : IParentGenerator
        {
            RegionMap map;
            Random rnd;
            int level;
            public RandomParentGenerator(RegionMap map, int level, Random rnd)
            {
                this.map = map;
                this.rnd = rnd;
                this.level = level;
            }

            public int GetParent(HexGrid.Hex hex)
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
            int level;
            int[] reg_size;

            public EqualSizeParentGenerator(RegionMap map, int level, Random rnd)
            {
                this.map = map;
                this.rnd = rnd;
                this.level = level;
                reg_size = new int[map.GetGridAtLevel(level - 1).Size];
            }

            public int GetParent(HexGrid.Hex hex)
            {
                if (hex.Parent > -1)
                {
                    return hex.Parent;
                }

                HexGrid grid = map.GetGridAtLevel(level);
                HexGrid parent_grid = map.GetGridAtLevel(level - 1);

                int[] candidates =
                    hex.Neighbors
                    .Where(n => n > -1)
                    .Select(n => grid.GetHex(n).Parent)
                    .Where(p => p > -1)
                    .OrderBy(hi => reg_size[hi])
                    .ToArray();

                candidates = candidates.Where(i => reg_size[i] == reg_size[candidates[0]]).ToArray();

                int index = rnd.Next(candidates.Length);
                HexGrid.Hex parent = parent_grid.GetHex(index);
                reg_size[candidates[index]] += 1;

                return candidates[index];
            }
        }
    }
}
