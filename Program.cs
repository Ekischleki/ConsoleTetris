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
            ContainerConsole containerConsole = new(null, ContainerConsole.StringFormatingOptions.SplitAtSpace);

            AspectRatioContainer gameField = new(GameBase.GamefieldLength * 2, GameBase.GamefieldHeight, new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.Middle), ConsoleGUI.Positional.BoxPos.Pos.Middle, true);
            Gamefield gf = new(gameField, new(), desktopHost, this, containerConsole);
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
                    new OffsetContainer(new(0,0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner), new(10, 0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner))
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