using System.Numerics;
using Raylib_cs;
namespace Kursova
{
    class Program
    {
        static void Main()
        {
            int screenWidth = 1280;
            int screenHeight = 720;
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(screenWidth, screenHeight, "3D Tic Tac Toe");

            Menu gameMenu = new Menu();
            GameEngine engine = new GameEngine();

            Camera3D camera = new Camera3D
            {
                Position = new Vector3(10.0f, 10.0f, 10.0f),
                Target = new Vector3(2.0f, 2.0f, 2.0f),
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = 45.0f,
                Projection = CameraProjection.Perspective
            };
            Raylib.UpdateCamera(ref camera, CameraMode.Orbital);
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                if (gameMenu.CurrentState == GameState.Quit)
                {
                    break;
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.Right))
                {
                    Raylib.DisableCursor();
                }

                if (Raylib.IsMouseButtonDown(MouseButton.Right))
                {
                    Raylib.UpdateCamera(ref camera, CameraMode.ThirdPerson);
                }

                if (Raylib.IsMouseButtonReleased(MouseButton.Right))
                {
                    Raylib.EnableCursor(); 
                }

                float wheel = Raylib.GetMouseWheelMove();
                if (wheel != 0)
                {
                    Raylib.UpdateCamera(ref camera, CameraMode.ThirdPerson);
                }
                Ray mouseRay = Raylib.GetMouseRay(Raylib.GetMousePosition(), camera);
                if (!Raylib.IsMouseButtonDown(MouseButton.Right))
                {
                    switch (gameMenu.CurrentState)
                    {
                        case GameState.Pvp:
                            engine.UpdateSelection(mouseRay);
                            if (engine.isGameOver) gameMenu.CurrentState = GameState.Ending;
                            break;

                        case GameState.Pve:
                            if (engine.isRedTurn)
                            {
                                engine.UpdateSelection(mouseRay);
                            }
                            else if (!engine.isGameOver)
                            {
                                engine.MakeAIMove();
                            }

                            if (engine.isGameOver)
                            {
                                gameMenu.CurrentState = GameState.Ending;
                            }
                            break;

                        case GameState.Ending:
                            if (Raylib.IsKeyPressed(KeyboardKey.R))
                            {
                                engine = new GameEngine();
                                gameMenu.CurrentState = GameState.Menu;
                            }
                            break;
                    }
                }
                

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.RayWhite);

                if (gameMenu.CurrentState == GameState.Menu)
                {
                    gameMenu.Draw();
                }
                else
                {
                    Raylib.BeginMode3D(camera);
                    if (engine.isGameOver && engine.winningLineIndices != null)
                    {
                        Vector3 start = engine.Cells[engine.winningLineIndices[0]].Position;
                        Vector3 end = engine.Cells[engine.winningLineIndices[2]].Position;
                        Raylib.DrawLine3D(start, end, Color.Gold);
                        Raylib.DrawSphere(start, 0.2f, Color.Gold);
                        Raylib.DrawSphere(end, 0.2f, Color.Gold);
                    }
                    engine.DrawAllCells();
                    Raylib.EndMode3D();

                    if (gameMenu.CurrentState != GameState.Ending)
                    {
                        DrawLayerSlices(engine, gameMenu);

                        string turn = engine.isRedTurn ? "RED'S TURN" : "BLUE'S TURN";
                        Raylib.DrawText(turn, 10, 10, 20, engine.isRedTurn ? Color.Red : Color.Blue);
                    }
                    else
                    {
                        DrawEndingUI(engine.winnerColor);
                    }
                }

                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }

        public static void DrawEndingUI(Color winnerColor)
        {
            int sw = Raylib.GetScreenWidth();
            int sh = Raylib.GetScreenHeight();

            string colorName = winnerColor.Equals(Color.Red) ? "RED" :
                winnerColor.Equals(Color.Blue) ? "BLUE" : "DRAW";
            string message = colorName == "DRAW" ? "IT'S A DRAW!" : $"{colorName} WINS!";

            Raylib.DrawRectangle(0, 0, sw, sh, Raylib.Fade(Color.Black, 0.6f));

            int fontSize = 50;
            int textWidth = Raylib.MeasureText(message, fontSize);
            Raylib.DrawText(message, sw / 2 - textWidth / 2, sh / 2 - 50, fontSize, winnerColor);

            string subText = "Press 'R' to return to Menu";
            int subWidth = Raylib.MeasureText(subText, 20);
            Raylib.DrawText(subText, sw / 2 - subWidth / 2, sh / 2 + 20, 20, Color.White);
        }
        public static void DrawLayerSlices(GameEngine engine, Menu gameMenu)
        {
            int size = 40;
            int padding = 5;
            int layerSpacing = 30;
            int startX = 50;
            int startY = 50;

            for (int y = 0; y < 3; y++)
            {
                int stackYOffset = startY + y * (3 * (size + padding) + layerSpacing);
                Raylib.DrawText($"Layer {y}", startX, stackYOffset - 20, 15, Color.Black);

                for (int z = 0; z < 3; z++)
                {
                    for (int x = 0; x < 3; x++) 
                    {
                        int index = (x * 9) + (y * 3) + z;

                        Color cellColor = engine.Cells[index].Color;

                        int posX = startX + x * (size + padding);
                        int posY = stackYOffset + z * (size + padding);

                        Rectangle rect = new Rectangle(posX, posY, size, size);
                        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect))
                        {
                            Color hoverColor = engine.isRedTurn ? Color.Red : Color.Blue;
                            Raylib.DrawRectangleLinesEx(rect, 2, Raylib.Fade(hoverColor, 0.7f));

                            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                            {
                                bool isPvP = gameMenu.CurrentState == GameState.Pvp;
                                bool isPlayersTurn = gameMenu.CurrentState == GameState.Pve && engine.isRedTurn;

                                if (isPvP || isPlayersTurn)
                                {
                                    engine.SelectFrom2D(index);
                                }
                            }
                        }

                        Color drawColor = cellColor.Equals(Color.Gray) ? Raylib.Fade(Color.LightGray, 0.3f) : cellColor;
                        Raylib.DrawRectangle(posX, posY, size, size, drawColor);
                        Raylib.DrawRectangleLines(posX, posY, size, size, Color.DarkGray);
                    }
                }
            }
        }
    }
}