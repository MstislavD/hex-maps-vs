namespace HexGeometry
{
    public class HexGrid
    {
        int columns;
        int rows;

        public int Columns { get { return columns; } }
        public int Rows { get { return rows; } }

        public static HexGrid Create(int columns, int rows) => new HexGrid(columns, rows);

        HexGrid(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
        }

        public override string ToString()
        {
            return $"{Columns} x {Rows} hex grid";
        }
    }
}
