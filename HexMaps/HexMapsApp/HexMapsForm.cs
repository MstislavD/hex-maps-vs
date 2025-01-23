using HexGeometry;
using MapGeneration;

namespace HexMapsApp
{
    public partial class HexMapsForm : Form
    {
        

        public HexMapsForm()
        {
            InitializeComponent();

            Shown += HexMapsForm_Shown;
        }

        private void HexMapsForm_Shown(object? sender, EventArgs e)
        {
            int columns = 10;
            int rows = 7;
            int levels = 3;
            int shown_level =0;

            Random rnd = new();
            RegionMap map = RegionMap.Create(columns, rows, levels, rnd);
            HexGrid hexGrid = map.GetGridAtLevel(levels - 1);
            Color[] colors = map.Regions.Select(r => random_color(rnd)).ToArray();

            float x_factor = (float)(ClientSize.Width / hexGrid.Width);
            float y_factor = (float)(ClientSize.Height / hexGrid.Height);
            float factor = Math.Min(x_factor, y_factor);

            Image image = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.White);

            foreach (HexGrid.Hex hex in hexGrid.Hexes)
            {
                RegionMap.Region region = map.GetRegionByHex(levels - 1, hex.Index);
                RegionMap.Region ancestor = map.GetAncestorAtLevel(region, shown_level);
                SolidBrush brush = new(colors[ancestor.Index]);
                g.FillPolygon(brush, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
                g.DrawPolygon(Pens.Black, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
            }

            CreateGraphics().DrawImage(image, 0, 0);
        }

        static PointF Screen_point(HexGrid.Point p, float factor) => new((float)p.X * factor, (float)p.Y * factor);

        static void Log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        static Color random_color(Random rnd) => Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
    }
}
