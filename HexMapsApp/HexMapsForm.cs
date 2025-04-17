using System.Drawing.Drawing2D;
using System.IO.MemoryMappedFiles;
using HexGeometry;
using MapGeneration;

namespace HexMapsApp
{
    public partial class HexMapsForm : Form
    {
        int menu_width;
        RegionMap? map;
        int seed;
       
        ComboBox? reg_gen_mode_dropdown;

        public HexMapsForm()
        {
            InitializeComponent();

            place_controls();
        }

        void place_controls()
        {
            // This code probably belongs to InitializeComponent() in HexMapsForm.Designer.cs

            ToolTip toolTip = new ToolTip();

            DoubleBuffered = true;
            Shown += regenerate_map;
            Resize += redraw_map;

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.AutoSize = true;
            menuPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Button gen_button = new Button();
            gen_button.AutoSize = true;
            gen_button.Text = "Generate";
            gen_button.Click += regenerate_map;

            reg_gen_mode_dropdown = drop_down_list(typeof(RegionMap.RegionGeneration), toolTip);
            reg_gen_mode_dropdown.SelectedIndexChanged += regenerate_map_keep_seed;

            Label info = new Label();
            info.AutoSize = true;
            new_info += (s, str) => info.Text = str;
            menuPanel.SizeChanged += (s, e) => info.Location = new Point(menuPanel.Right + 5, 5);

            CheckBox analyse_check = new CheckBox();
            analyse_check.Text = "Analysis";
            analyse_check.AutoSize = true;
            generation_complete += (s, e) => { if (analyse_check.Checked) new_info?.Invoke(s, map.AnalyzeRegions()); };
            analyse_check.CheckedChanged += (s, e) => info.Visible = analyse_check.Checked;
            analyse_check.CheckedChanged += (s, e) => { if (analyse_check.Checked) new_info?.Invoke(s, map.AnalyzeRegions()); };

            menuPanel.Controls.AddRange([gen_button, reg_gen_mode_dropdown, analyse_check]);

            Controls.AddRange([menuPanel, info]);

            menu_width = menuPanel.Width;         

            generation_complete += redraw_map;
        }

        event EventHandler<string>? new_info;

        event EventHandler? generation_complete;


        static ComboBox drop_down_list(Type enumType, ToolTip toolTip)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.DataSource = Enum.GetValues(enumType);
            comboBox.DropDownWidth = Enum.GetNames(enumType).Max(x => TextRenderer.MeasureText(x, DefaultFont).Width);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            toolTip.SetToolTip(comboBox, enum_name(enumType));
            return comboBox;
        }

        static string enum_name(Type enumType)
        {
            return enumType.Name.Aggregate("", (name, c) => name + (char.IsLower(c) ? c : $" {c}"));
        }

        private void redraw_map(object? sender, EventArgs e)
        {
            Invalidate();
        }

        void regenerate_map(object? sender, EventArgs e)
        {
            seed = new Random().Next();
            regenerate_map_keep_seed(sender, e) ;
        }
        void regenerate_map_keep_seed(object? sender, EventArgs e)
        {
            int columns = 10;
            int rows = 7;
            int levels = 5;

            Random rnd = new Random(seed);

            RegionMap.RegionGeneration reg_gen =
                (RegionMap.RegionGeneration)(reg_gen_mode_dropdown?.SelectedItem ?? RegionMap.RegionGeneration.Random);
            map = RegionMap.Create(columns, rows, levels, reg_gen, rnd);

            generation_complete?.Invoke(this, e);
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

            int region_level = 0;
            int cell_level = map.Levels - 1;

            HexGrid hexGrid = map.GetGridAtLevel(cell_level);

            Random rnd = new Random(seed);
            var region_to_brush = map.GetRegions(region_level).ToDictionary(r => r, r => new SolidBrush(random_color(rnd)));

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
                RegionMap.Region region = map.GetRegionByHex(cell_level, hex.Index);
                RegionMap.Region ancestor = map.GetAncestorAtLevel(region, region_level);
                SolidBrush brush = region_to_brush[ancestor];
                g.FillPolygon(brush, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
                g.DrawPolygon(Pens.Black, hex.Points.Select(p => Screen_point(p, factor)).ToArray());
            }

            e.Graphics.DrawImage(image, menu_width, 0);
        }
    }
}
