using ConsoleGUI;
using ConsoleGUI.Container;
using ConsoleGUI.Drawables;
using ConsoleGUI.Host;

namespace Tetris
{
    public class TetrisProject : Project
    {
        private IDesktopHost desktopHost;
        public TetrisProject(IDesktopHost host) : base(host) 
        {
            desktopHost = host;
        }

        private static void Main(string[] args)
        {
            TetrisProject project = new(IDesktopHost.GetSuitableDesktopHost(OSCheck.GetCurrentPlattform()));
            project.StartProject();
        }

        public override void InitProject()
        {
            Tile tile = new( new('a', ConsoleColor.DarkMagenta, ConsoleColor.DarkBlue), new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner));

            AspectRatioContainer gameField = new(GameBase.GamefieldLength * 2, GameBase.GamefieldHeight, new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.Middle), ConsoleGUI.Positional.BoxPos.Pos.Middle);
            Gamefield gf = new(gameField, new(), desktopHost, this);
            ContainerConsole containerConsole = new(null, ContainerConsole.StringFormatingOptions.SplitAtSpace);
            containerConsole.WriteLine("Score: 0", ConsoleColor.White, ConsoleColor.Black);
            RootContainer = new ConsoleContainer()
            {
                ContainerChildren = new[]
                {
                    new OffsetContainer(new(5, 2, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner), new(-5, -2, ConsoleGUI.Positional.BoxPos.Pos.DownRightCorner))
                    {
                        ContainerChildren = new[]
                        {
                            gameField
                        }
                    },
                    new OffsetContainer(new(0,0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner), new(0, 10, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner))
                    {
                        DrawableChildren = new[]
                        {
                            containerConsole
                        }
                    }
                    
                },
                DrawableChildren = new[]{
                    gf
                }
                
            };
            logger = new NoLog();

        }

        public override void Update()
        {
            debugging = false;
        }
    }
}