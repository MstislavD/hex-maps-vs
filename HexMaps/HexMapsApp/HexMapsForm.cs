using HexGeometry;

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
            HexGrid hexGrid = HexGrid.Create(30, 20);
            Log($"{hexGrid} created");

            float x_factor = (float)(ClientSize.Width / hexGrid.Width);
            float y_factor = (float)(ClientSize.Height / hexGrid.Height);
            float factor = Math.Min(x_factor, y_factor);

            Image image = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.White);

            Random rnd = new Random();

            int hex_index = rnd.Next(0, 30 * 20);
            foreach (HexGrid.Hex hex in hexGrid.Hexes)
            {
                if (hex.Neighbors.Contains(hex_index))
                {
                    g.FillPolygon(Brushes.Red, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
                }
                g.DrawPolygon(Pens.Black, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
            }

            CreateGraphics().DrawImage(image, 0, 0);
        }

        static PointF Screen_point(HexGrid.Point p, float factor) => new((float)p.X * factor, (float)p.Y * factor);

        static void Log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }
    }
}
