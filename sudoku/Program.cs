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

    private int BlankNumber = 0;

    private bool writeAnswer = false;

    static void Main(string[] args) {

        Application.Init();
        
        Program program = new Program();
        program.SetupUI();
        program.Solve();

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

    private int[,] board;
    private int[,] answer = new int[9,9];

    private void Solve() {
        board = sudokuBoard;
        if (SolveSudoku()) {
            PrintBoard();
        }else{
            Console.WriteLine("No solution exists.");
        }
    }

    private bool SolveSudoku() {
        for (int row = 0; row < 9; row++) {
            for (int col = 0; col < 9; col++) {
                if (board[row, col] == 0) {
                    for (int num = 1; num <= 9; num++) {
                        if (IsValidMove(row, col, num)) {
                            board[row, col] = num;

                            if (SolveSudoku()) {
                                return true;
                            }

                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsValidMove(int row, int col, int num) {
        for (int i = 0; i < 9; i++) {
            if (board[row, i] == num || board[i, col] == num) {
                return false;
            }
        }

        int boxRow = row - row % 3;
        int boxCol = col - col % 3;

        for (int i = boxRow; i < boxRow + 3; i++) {
            for (int j = boxCol; j < boxCol + 3; j++) {
                if (board[i, j] == num) {
                    return false;
                }
            }
        }

        return true;
    }

    private void PrintBoard() {
        for (int row = 0; row < 9; row++) {
            for (int col = 0; col < 9; col++) {
                // Console.Write(board[row, col] + " ");
                answer[row, col] = board[row, col];
            }
            // Console.WriteLine();
        }
    }

    private void GenerateSudokuBoard() {
        for (int i = 0; i < BoardSize; i++) {
            for (int j = 0; j < BoardSize; ++j) {
                if (sudokuBoard[i,j] == 0) {
                    BlankNumber++;
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
        // Console.WriteLine($"Pressed digit: {digit}");
        if (!isBlankCell[selectedCellRow, selectedCellCol]) {
            BlankNumber--;
        }
        if (HighLightCell[selectedCellRow, selectedCellCol]) {
            sudokuBoard[selectedCellRow, selectedCellCol] = digit;
            isBlankCell[selectedCellRow, selectedCellCol] = true;
            drawingArea.QueueDraw();
        }

        if (BlankNumber == 0) {
            if (isCompleted()) {
                ShowMessageDialog("Succeeded", "Congratulations! Sudoku is solved.");
            } else {
                ShowRetryDialog();
            }
        }
    }
    private bool isCompleted() {
        for (int i = 0; i < BoardSize; i++) {
            for (int j = 0; j < BoardSize; ++j) {
                if (sudokuBoard[i,j] != answer[i, j]) {
                    return false;
                }
            }
        }
        return true;
    }

    private void ShowMessageDialog(string title, string message) {
        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, message);
        dialog.Title = title;
        dialog.Run();
        dialog.Destroy();
    }

    private void ShowRetryDialog() {
        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.None, "Try again?");
        dialog.AddButton("Retry", ResponseType.Yes);
        dialog.AddButton("Answer", ResponseType.No);
        dialog.Title = "Failed";
        ResponseType response = (ResponseType)dialog.Run();
        dialog.Destroy();

        if (response == ResponseType.Yes) {
            // Clear the board and reset BlankNumber here
            ClearSudokuBoard();
            drawingArea.QueueDraw();
        } else {
            writeAnswer = true;
            drawingArea.QueueDraw();
        }
    }

    private void ClearSudokuBoard() {
        for (int i = 0; i < BoardSize; i++) {
            for (int j = 0; j < BoardSize; j++) {
                if (HighLightCell[i, j]) {
                    isBlankCell[i, j] = false;
                    sudokuBoard[i,j] = 0;
                    BlankNumber++;
                } else {
                    isBlankCell[i,j] = true;
                }
            }
        }
    }

    private void OnCellClick(object o, ButtonPressEventArgs args) {
        int x = (int)args.Event.X;
        int y = (int)args.Event.Y;

        int row = y / CellSize;
        int col = x / CellSize;

        if (row >= 0 && row < BoardSize && col >= 0 && col < BoardSize) {
            selectedCellRow = row;
            selectedCellCol = col;
            if (HighLightCell[selectedCellRow, selectedCellCol]) {
                drawingArea.QueueDraw();
            }
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
        if (!writeAnswer) {
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
        } else {
            for (int i = 0; i < BoardSize; i++) {
                for (int j = 0; j < BoardSize; j++) {
                    if (HighLightCell[i, j]) {
                        if (sudokuBoard[i,j] != answer[i, j]) {
                            cr.SetSourceRGB(1, 0, 0);
                        } else {
                            cr.SetSourceRGB(0, 0, 1);
                        }
                    } else {
                        cr.SetSourceRGB(0, 0, 0);
                    }
                    DrawNumber(cr, i, j, answer[i, j]);
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