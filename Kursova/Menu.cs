using System.Numerics;
using Raylib_cs;

namespace Kursova;

public class Menu
{
    public GameState CurrentState = GameState.Menu;

    public void Draw()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        int btnWidth = 250;
        int centerX = sw / 2 - btnWidth / 2;

        Raylib.ClearBackground(Color.DarkGray);

        int titleWidth = Raylib.MeasureText("3D TIC-TAC-TOE", 40);
        Raylib.DrawText("3D TIC-TAC-TOE", sw / 2 - titleWidth / 2, sh / 4, 40, Color.Gold);

        if (GuiButton(new Rectangle(centerX, sh / 2, btnWidth, 50), "Player vs Player"))
        {
            CurrentState = GameState.Pvp;
        }

        if (GuiButton(new Rectangle(centerX, sh / 2 + 70, btnWidth, 50), "Player vs AI"))
        {
            CurrentState = GameState.Pve;
        }

        if (GuiButton(new Rectangle(centerX, sh / 2 + 140, btnWidth, 50), "Exit Game"))
        {
            CurrentState = GameState.Quit;
        }
    }

    private bool GuiButton(Rectangle rect, string text)
    {
        Vector2 mousePoint = Raylib.GetMousePosition();
        bool isHovered = Raylib.CheckCollisionPointRec(mousePoint, rect);

        Color btnColor = isHovered ? Color.LightGray : Color.Gray;
        Raylib.DrawRectangleRec(rect, btnColor);
        Raylib.DrawText(text, (int)rect.X + 10, (int)rect.Y + 15, 20, Color.Black);

        return isHovered && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }
}