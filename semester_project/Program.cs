using static System.Console;
using Cairo;
using Gtk;


class Program {
    private const int BoardSize = 9;
    private const int CellSize = 50;
    private const double ThinLineWidth = 1.0;
    private const double ThickLineWidth = 3.0;
    private int[,] sudokuBoard = new int[,] {
        {0,2,9,5,8,0,3,7,6},
        {0,1,0,0,4,0,9,0,0},
        {7,0,8,3,0,2,0,5,0},
        {8,0,0,0,7,0,0,3,0},
        {9,3,0,6,2,0,0,4,0},
        {2,0,0,0,0,0,0,6,1},
        {1,0,0,0,6,0,4,2,3},
        {6,0,4,2,5,0,0,1,0},
        {3,7,0,0,1,4,0,9,0}
    };
    private bool[,] isBlankCell = new bool[BoardSize, BoardSize];
    private bool[,] HighLightCell = new bool[BoardSize, BoardSize];

    private int selectedCellRow = -1;
    private int selectedCellCol = -1;

    static void Main() {
        Application.Init();
        
        Program program = new Program();
        program.SetupUI();

        Application.Run();
    }

    private DrawingArea drawingArea;

    private void SetupUI() {
        Window window = new Window("Sudoku board");
        window.SetDefaultSize(450, 450);
        window.DeleteEvent += delegate { Application.Quit(); };

        GenerateSudokuBoard();

        drawingArea = new DrawingArea();
        drawingArea.CanFocus = true;
        drawingArea.Drawn += OnDraw;
        drawingArea.KeyPressEvent += OnKeyPress;
        drawingArea.Events |= Gdk.EventMask.ButtonPressMask;
        drawingArea.ButtonPressEvent += OnCellClick;

        window.Add(drawingArea);
        window.ShowAll();

        drawingArea.GrabFocus();
    }

    private void GenerateSudokuBoard() {
        for (int i = 0; i < BoardSize; i++) {
            for (int j = 0; j < BoardSize; ++j) {
                if (sudokuBoard[i,j] == 0) {
                    isBlankCell[i,j] = false;
                    HighLightCell[i, j] = true;
                } else {
                    isBlankCell[i,j] = true;
                    HighLightCell[i, j] = false;
                }
            }
        }
    }

    private void OnKeyPress(object o, KeyPressEventArgs args) {
        Gdk.Key key = args.Event.Key;
        uint keyValue = (uint)key;

        int digit = (int)(keyValue) - 48;
        Console.WriteLine($"Pressed digit: {digit}");
        sudokuBoard[selectedCellRow, selectedCellCol] = digit;
        isBlankCell[selectedCellRow, selectedCellCol] = true;
        drawingArea.QueueDraw();
    }

    private void OnCellClick(object o, ButtonPressEventArgs args) {
        int x = (int)args.Event.X;
        int y = (int)args.Event.Y;

        int row = y / CellSize;
        int col = x / CellSize;

        if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize) {
            selectedCellRow = row;
            selectedCellCol = col;
            drawingArea.QueueDraw();
        }
    }

    private void OnDraw(object o, DrawnArgs args) {
        var cr = args.Cr;
        cr.SetSourceRGB(0, 0, 0);

        for (int i = 1; i <= BoardSize; i++) {

            // Draw horizontal lines
            cr.MoveTo(0, i * CellSize);
            cr.LineTo(BoardSize * CellSize, i * CellSize);
            cr.LineWidth = (i % 3 == 0) ? ThickLineWidth : ThinLineWidth;
            cr.Stroke();

            // Draw vertical lines
            cr.MoveTo(i * CellSize, 0);
            cr.LineTo(i * CellSize, BoardSize * CellSize);
            cr.LineWidth = (i % 3 == 0) ? ThickLineWidth : ThinLineWidth;
            cr.Stroke();
        }

        //Draw numbers
        for (int i = 0; i < BoardSize; i++) {
            for (int j = 0; j < BoardSize; j++) {
                if (isBlankCell[i, j]) {
                    if (HighLightCell[i, j]) {
                        cr.SetSourceRGB(0, 0, 1);
                    } else {
                        cr.SetSourceRGB(0, 0, 0);
                    }
                    DrawNumber(cr, i, j, sudokuBoard[i, j]);
                }
            }
        }

        // Draw highlight
        if (selectedCellRow != -1 && selectedCellCol != -1) {
            double x = selectedCellCol * CellSize;
            double y = selectedCellRow * CellSize;
            cr.NewPath();
            cr.Rectangle(x, y, CellSize, CellSize);
            cr.LineWidth = 2.0;
            cr.SetSourceRGB(1, 0, 0);
            cr.Stroke();
        }
    }

    private void DrawNumber(Cairo.Context cr, int row, int col, int number) {
        cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Bold);
        cr.SetFontSize(20);
        cr.MoveTo((col * CellSize) + (CellSize / 3), (row * CellSize) + (CellSize / 2));
        cr.ShowText(number.ToString());
    }
}
