using HexGeometry;

namespace HexMapsApp
{
    public partial class HexMapsForm : Form
    {
        public HexMapsForm()
        {
            InitializeComponent();

            onStart();
        }

        void onStart()
        {
            HexGrid hexGrid = HexGrid.Create(10, 10);
            log($"{hexGrid} created");
        }

        void log(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }
    }
}
