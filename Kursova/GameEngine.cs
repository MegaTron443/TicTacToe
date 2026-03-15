using Raylib_cs;
using System.Numerics;

namespace Kursova;

public class GameEngine
{
    public int[]? winningLineIndices = null;

    public bool isRedTurn = true;

    public bool isGameOver = false;

    public Color winnerColor = Color.Gray;
    public Cell? HoveredCell { get; private set; }
    public Cell[] Cells { get; set; }

    public GameEngine()
    {
        this.Cells = new Cell[27];
        int index = 0;
        float spacing = 2.0f; 
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    Vector3 pos = new Vector3(x * spacing, y * spacing, z * spacing);
                    Cells[index] = new Cell(pos, Color.Gray);
                    index++;
                }
            }
        }
    }

    public void UpdateSelection(Ray mouseRay)
    {
        if (isGameOver)
        {
            HoveredCell = null;
            return;
        }

        HoveredCell = null;
        float minDistance = float.MaxValue;

        foreach (var cell in Cells)
        {
            RayCollision collision = Raylib.GetRayCollisionBox(mouseRay, cell.GetBounds());
            if (collision.Hit && collision.Distance < minDistance)
            {
                minDistance = collision.Distance;
                HoveredCell = cell;
            }
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.Left) && HoveredCell != null)
        {
            if (HoveredCell.Color.Equals(Color.Gray))
            {
                HoveredCell.Color = isRedTurn ? Color.Red : Color.Blue;

                Color winner = CheckForWin();
                if (!winner.Equals(Color.Gray))
                {
                    isGameOver = true;
                    winnerColor = winner;
                    return;
                }

                if (IsGridFull())
                {
                    isGameOver = true;
                    winnerColor = Color.White;
                    return;
                }

                isRedTurn = !isRedTurn;
            }
        }
    }

    public void DrawAllCells()
    {
        foreach (var cell in Cells)
        {
            cell.Draw();
        }

        if (winningLineIndices != null)
        {
            foreach (int index in winningLineIndices)
            {
                Raylib.DrawCubeWires(Cells[index].Position, 1.1f, 1.1f, 1.1f, Color.Gold);
            }
        }

        if (HoveredCell != null && !isGameOver)
        {
            HoveredCell.DrawTargeted();
        }
    }

    private bool IsGridFull()
    {
        foreach (var cell in Cells)
        {
            if (cell.Color.Equals(Color.Gray)) return false;
        }
        return true;
    }

    public Color CheckForWin()
    {
        int[] directions = { 1, 3, 9, 4, 2, 10, 12, 13, 14, 8, 7, 5, 6 };

        for (int i = 0; i < Cells.Length; i++)
        {
            Color player = Cells[i].Color;
            if (player.Equals(Color.Gray)) continue;

            foreach (int step in directions)
            {
                if (IsSequenceMatch(i, step, player))
                {
                    winningLineIndices = new int[] { i, i + step, i + 2 * step };
                    return player;
                }
            }
        }

        return Color.Gray;
    }

    private bool IsSequenceMatch(int start, int step, Color player)
    {
        int second = start + step;
        int third = start + 2 * step;


        if (second >= 27 || third >= 27) return false;

        if (!Cells[second].Color.Equals(player) || !Cells[third].Color.Equals(player))
            return false;

        float expectedDist = (Cells[second].Position - Cells[start].Position).Length();
        float totalDist = (Cells[third].Position - Cells[start].Position).Length();

        return Math.Abs(totalDist - (2 * expectedDist)) < 0.1f;
    }

    public void MakeAIMove()
    {
        if (isGameOver) return;

        int bestMoveIndex = -1;
        int bestScore = int.MinValue;

        List<int[]> winningLines = GetWinningLines();

        for (int i = 0; i < Cells.Length; i++)
        {
            if (Cells[i].Color.Equals(Color.Gray))
            {
                int score = EvaluateMove(i, winningLines);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoveIndex = i;
                }
            }
        }

        if (bestMoveIndex != -1)
        {
            Cells[bestMoveIndex].Color = Color.Blue;

            Color winner = CheckForWin();
            if (!winner.Equals(Color.Gray))
            {
                isGameOver = true;
                winnerColor = winner;
            }
            else if (IsGridFull())
            {
                isGameOver = true;
                winnerColor = Color.White;
            }
            else
            {
                isRedTurn = true;
            }
        }
    }

    private int EvaluateMove(int cellIndex, List<int[]> lines)
    {
        int score = 0;

        foreach (var line in lines)
        {
            if (!line.Contains(cellIndex)) continue;

            int aiCount = 0;
            int playerCount = 0;
            int emptyCount = 0;

            foreach (int idx in line)
            {
                Color c = (idx == cellIndex) ? Color.Blue : Cells[idx].Color;
                if (c.Equals(Color.Blue)) aiCount++;
                else if (c.Equals(Color.Red)) playerCount++;
                else emptyCount++;
            }

            if (cellIndex == 13) score += 20;
            if (aiCount == 3) score += 1000;
            if (playerCount == 2 && aiCount == 1) score += 150;
            if (aiCount == 2 && emptyCount == 1) score += 50;
            if (aiCount == 1 && emptyCount == 2) score += 10;
        }

        return score;
    }

    private List<int[]> GetWinningLines()
    {
        List<int[]> lines = new List<int[]>();
        int[] directions = { 1, 3, 9, 4, 2, 10, 12, 13, 14, 8, 7, 5, 6 };

        for (int i = 0; i < Cells.Length; i++)
        {
            foreach (int step in directions)
            {
                int second = i + step;
                int third = i + 2 * step;

                if (second < 27 && third < 27)
                {
                    float d1 = (Cells[second].Position - Cells[i].Position).Length();
                    float d2 = (Cells[third].Position - Cells[i].Position).Length();
                    if (Math.Abs(d2 - (2 * d1)) < 0.1f)
                    {
                        lines.Add(new int[] { i, second, third });
                    }
                }
            }
        }
        return lines;
    }
    public void SelectFrom2D(int index)
    {
        if (isGameOver || index < 0 || index >= 27) return;

        if (Cells[index].Color.Equals(Color.Gray))
        {
            Cells[index].Color = isRedTurn ? Color.Red : Color.Blue;
            Color winner = CheckForWin();
            if (!winner.Equals(Color.Gray))
            {
                isGameOver = true;
                winnerColor = winner;
            }
            else if (IsGridFull())
            {
                isGameOver = true;
                winnerColor = Color.White;
            }
            else
            {
                // 5. Switch turns
                isRedTurn = !isRedTurn;
            }
        }
    }
}