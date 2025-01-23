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
            Random rnd = new();
            HexGrid parentGrid = HexGrid.Create(15, 10);
            Color[] colors = parentGrid.Hexes.Select(h => random_color(rnd)).ToArray();

            HexGrid hexGrid = HexGrid.CreateChild(parentGrid);

            float x_factor = (float)(ClientSize.Width / hexGrid.Width);
            float y_factor = (float)(ClientSize.Height / hexGrid.Height);
            float factor = Math.Min(x_factor, y_factor);

            Image image = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.White);

            foreach (HexGrid.Hex hex in hexGrid.Hexes)
            {

                int parent = choose_parent(hexGrid, hex, rnd);
                SolidBrush brush = new(colors[parent]);
                g.FillPolygon(brush, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
                g.DrawPolygon(Pens.Black, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
            }

            CreateGraphics().DrawImage(image, 0, 0);
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

        static PointF Screen_point(HexGrid.Point p, float factor) => new((float)p.X * factor, (float)p.Y * factor);

        static void Log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        static Color random_color(Random rnd) => Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
    }
}
