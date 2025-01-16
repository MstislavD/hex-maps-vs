using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HexGeometry
{
    public class HexGrid
    {
        public readonly struct Point(double x, double y)
        {
            public double X { get; } = x;
            public double Y { get; } = y;

            public static Point operator +(Point p1, Point p2) => new(p1.X + p2.X, p1.Y + p2.Y);
        }

        public readonly struct Hex(HexGrid.Point[] points)
        {
            public Point[] Points { get; } = points;
        }

        readonly int columns;
        readonly int rows;
        readonly double width;
        readonly double height;
        readonly Hex[,] hexes;

        public int Columns { get { return columns; } }
        public int Rows { get { return rows; } }

        public double Width { get { return width; } }
        public double Height { get { return height; } }

        public IEnumerable<Hex> Hexes
        {
            get
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        yield return hexes[i, j];
                    }
                }
            }
        }

        public static HexGrid Create(int columns, int rows) => new(columns, rows);

        HexGrid(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
            hexes = new Hex[rows, columns];

            double l_radius = 1;
            double s_radius = l_radius * Math.Sqrt(3) / 2;

            width = s_radius * 2 * (columns + 0.5);
            height = l_radius * (rows * 1.5 + 0.5);

            double x_step = s_radius * 2;
            double y_step = l_radius * 3 / 2;

            double center_x = s_radius;
            double center_y = l_radius;
     
            Point[] hex_points = Generate_hex_points(l_radius, s_radius);

            bool shift = true;

            for (int i = 0; i < rows; i++, center_y += y_step, center_x -= width)
            {
                shift = !shift;
                center_x += shift ? x_step : 0;

                for (int j = 0; j < columns; j++, center_x += x_step)
                {
                    Point center = new(center_x, center_y);
                    Point[] points = hex_points.Select(p => center + p).ToArray();
                    hexes[i, j] = new(points);
                }
            }
        }

        static Point[] Generate_hex_points(double l_radius, double s_radius)
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
