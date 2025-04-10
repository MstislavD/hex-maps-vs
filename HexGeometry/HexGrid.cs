using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HexGeometry
{
    public class HexGrid
    {
        public class Point(double x, double y)
        {
            public double X { get; } = x;
            public double Y { get; } = y;

            public static Point operator +(Point p1, Point p2) => new(p1.X + p2.X, p1.Y + p2.Y);
        }

        public class Hex(int index, Point[] points, int[] neighbors, int parent)
        {
            public int Index { get; } = index;
            public Point[] Points { get; } = points;

            public int[] Neighbors { get; } = neighbors;

            public int Parent { get; } = parent;

            public override string ToString()
            {
                return $"Index: {Index}; Neighbors: {string.Join(", ", Neighbors)}; Parent: {Parent}";
            }
        }

        readonly int columns;
        readonly int rows;
        readonly double width;
        readonly double height;
        readonly Hex[] hexes;

        public int Columns { get { return columns; } }
        public int Rows { get { return rows; } }

        public int Size => hexes.Length;

        public double Width { get { return width; } }
        public double Height { get { return height; } }

        public Hex GetHex(int index) => hexes[index];

        public IEnumerable<Hex> Hexes => hexes;

        public static HexGrid Create(int columns, int rows) => new(columns, rows);

        public static HexGrid CreateChild(HexGrid grid) => new(grid.Columns * 2, grid.Rows * 2 - 1);

        HexGrid(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
            hexes = new Hex[rows * columns];

            double l_radius = 1;
            double s_radius = l_radius * Math.Sqrt(3) / 2;

            width = s_radius * 2 * (columns + 0.5);
            height = l_radius * (rows * 1.5 + 0.5);

            double x_step = s_radius * 2;
            double y_step = l_radius * 3 / 2;

            double center_x = s_radius;
            double center_y = l_radius;

            int[][] n_indexes = [
                [-columns, 1, columns, columns - 1, -1, -columns - 1],
                [-columns + 1, 1, columns + 1, columns, -1, -columns]
                ];
     
            Point[] hex_points = Generate_hex_points(l_radius, s_radius);

            bool shifted_row = false;
            bool heir_hex = true;
            int parent = 0;

            for (int i = 0, count = 0; i < rows; i++)
            {                             
                for (int j = 0; j < columns; j++)
                {
                    Point center = new(center_x, center_y);
                    Point[] points = hex_points.Select(p => center + p).ToArray();
                    int[] neighbors = get_neighbor_indices(n_indexes, shifted_row, i, j, count);
                    int p = !shifted_row && heir_hex ? parent++ : -1;

                    hexes[count] = new(count++, points, neighbors, p);

                    center_x += x_step;
                    heir_hex = !heir_hex;
                }

                shifted_row = !shifted_row;
                heir_hex = shifted_row ? heir_hex : !heir_hex;

                center_x += -width + (shifted_row ? x_step : 0);
                center_y += y_step;               
            }
        }

        private int[] get_neighbor_indices(int[][] n_indexes, bool shift_row, int i, int j, int count)
        {
            int[] neighbors = n_indexes[shift_row ? 1 : 0].Select(i => i + count).ToArray();
            if (j == 0)
            {
                neighbors[4] = -1;
                if (!shift_row)
                {
                    neighbors[3] = neighbors[5] = -1;
                }
            }
            if (j == columns - 1)
            {
                neighbors[1] = -1;
                if (shift_row)
                {
                    neighbors[0] = neighbors[2] = -1;
                }
            }
            if (i == 0)
            {
                neighbors[0] = neighbors[5] = -1;
            }
            if (i == rows - 1)
            {
                neighbors[2] = neighbors[3] = -1;
            }
            return neighbors;
        }

        public static Point[] Generate_hex_points(double l_radius, double s_radius)
        {

            return [
                new Point(0, -l_radius ),
                new Point(s_radius, -l_radius / 2),
                new Point(s_radius, l_radius / 2),
                new Point(0, l_radius),
                new Point(-s_radius, l_radius / 2),
                new Point(-s_radius, -l_radius / 2)
                ];
        }

        public override string ToString()
        {
            return $"{Columns} x {Rows} hex grid";
        }
    }
}
