using System.Drawing.Drawing2D;
using HexGeometry;
using MapGeneration;

namespace HexMapsApp
{
    public partial class HexMapsForm : Form
    {
        int menu_width;
        RegionMap? map;
        Random rnd = new();
        Color[]? region_colors;

        public HexMapsForm()
        {
            InitializeComponent();

            place_controls();
        }

        void place_controls()
        {
            DoubleBuffered = true;
            Shown += regenerate_map;
            Resize += redraw_map;

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.AutoSize = true;
            menuPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Button gen_button = new Button();
            gen_button.AutoSize = true;
            gen_button.Text = "Generate";
            gen_button.Click += regenerate_map;
            
            menuPanel.Controls.AddRange([gen_button]);

            Controls.Add(menuPanel);

            menu_width = menuPanel.Width;
        }

        private void redraw_map(object? sender, EventArgs e)
        {
            Invalidate();
        }

        void regenerate_map(object? sender, EventArgs e)
        {
            int columns = 10;
            int rows = 7;
            int levels = 5;

            map = RegionMap.Create(columns, rows, levels, rnd);
            region_colors = map.Regions.Select(r => random_color(rnd)).ToArray();

            Invalidate();
        }

        static PointF Screen_point(HexGrid.Point p, float factor) => new((float)p.X * factor, (float)p.Y * factor);

        static void Log<T>(T msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        static Color random_color(Random rnd) => Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

        protected override void OnPaint(PaintEventArgs e)
        {
            if (map is null) return;

            int shown_level = 0;

            HexGrid hexGrid = map.GetGridAtLevel(map.Levels - 1);

            int img_width = ClientSize.Width - menu_width;
            int img_height = ClientSize.Height;

            float x_factor = (float)(img_width / hexGrid.Width);
            float y_factor = (float)(img_height / hexGrid.Height);
            float factor = Math.Min(x_factor, y_factor);

            img_width = (int)(hexGrid.Width * factor) + 1;
            img_height = (int)(hexGrid.Height * factor) + 1;

            Image image = new Bitmap(img_width, img_height);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.White);

            foreach (HexGrid.Hex hex in hexGrid.Hexes)
            {
                RegionMap.Region region = map.GetRegionByHex(map.Levels - 1, hex.Index);
                RegionMap.Region ancestor = map.GetAncestorAtLevel(region, shown_level);
                SolidBrush brush = new(region_colors[ancestor.Index]);
                g.FillPolygon(brush, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
                g.DrawPolygon(Pens.Black, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
            }

            e.Graphics.DrawImage(image, menu_width, 0);
        }
    }
}
