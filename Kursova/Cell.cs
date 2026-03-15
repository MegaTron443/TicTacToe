using System.Numerics;
using Raylib_cs;

namespace Kursova;

public class Cell
{
    public Color Color { get; set; }
    public Vector3 Position { get; set; }
    public float Size { get; set; } = 1.0f;

    public Cell(Vector3 position, Color color)
    {
        this.Position = position;
        this.Color = color;
    }

    public void Draw()
    {
        if (this.Color.Equals(Color.Gray))
        {
            Raylib.DrawCubeWires(Position, Size, Size, Size, Raylib.Fade(Color.Black, 0.2f));
            Raylib.DrawCube(Position, Size, Size, Size, Raylib.Fade(Color.Gray, 0.1f));
        }
        else
        {
            Raylib.DrawCube(Position, Size, Size, Size, Color);
            Raylib.DrawCubeWires(Position, Size, Size, Size, Color.Black);
        }
    }

    public void DrawTargeted()
    {
        Raylib.DrawCubeWires(Position, Size, Size, Size, Color.Gold);
    }

    public BoundingBox GetBounds()
    {
        return new BoundingBox(
            new Vector3(Position.X - Size / 2, Position.Y - Size / 2, Position.Z - Size / 2),
            new Vector3(Position.X + Size / 2, Position.Y + Size / 2, Position.Z + Size / 2)
        );
    }
}