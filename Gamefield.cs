using ConsoleGUI;
using ConsoleGUI.Buffering;
using ConsoleGUI.Container;
using ConsoleGUI.Drawables;
using ConsoleGUI.Host;
using ConsoleGUI.Input;
using ConsoleGUI.Positional;
using ConsoleGUI.String;

namespace Tetris
{

    public class TilePointer : Drawable
    {
        private Tile? tile;

        public Tile? Tile
        {
            get => tile;
            set
            {
                tile = value;
                if (tile != null && ContainerParent != null)
                    tile.ContainerParent = ContainerParent;



            }
        }

        public TilePointer(Tile? tile)
        {
            this.Tile = tile;
        }

        public override void DrawDrawable(IDrawer currentFrame)
        {
            Tile?.DrawDrawable(currentFrame);
        }
        protected override void FinishInitialising()
        {
            if (Tile != null)
                Tile.ContainerParent = ContainerParent;
        }
        public override void UpdateDrawable()
        {
            Tile?.UpdateDrawable();
        }

        protected override void OnRationChanged()
        {
        }
    }

    public class Tile : Drawable
    {
        public static readonly BetterChar EmptyChar = new(' ', ConsoleColor.Black, ConsoleColor.Black);
        private BetterChar renderChar;
        bool boxNeedsUpdate = false;
        public BetterChar RenderChar
        {
            get
            {
                return renderChar;
            }
            set
            {
                boxNeedsUpdate = true;
                renderChar = value;
            }
        }
        private Offset offset;

        public Offset Offset
        {
            get { return offset; }
            set { boxNeedsUpdate = true; offset = value; }
        }



        private Shape box;

        public Tile(BetterChar renderChar, Offset renderOffset) : base()
        {
            this.renderChar = renderChar;
            this.offset = renderOffset;
            boxNeedsUpdate = true;
        }

        public override void DrawDrawable(IDrawer drawer)
        {
            box.DrawDrawable(drawer);
        }


        protected override void FinishInitialising()
        {
            box = new(RenderChar, ContainerParent.BoxPos.Lenght - 1, ContainerParent.BoxPos.Height - 1, Offset, Shape.ShapeType.Box)
            {
                ContainerParent = this.ContainerParent
            };
        }

        public void RatioChanged()
        {
            OnRationChanged();
        }
        protected override void OnRationChanged()
        {
            boxNeedsUpdate = true;
        }


        public override void UpdateDrawable()
        {
            if (boxNeedsUpdate)
            {
                box = new(RenderChar, ContainerParent.BoxPos.Lenght - 1, ContainerParent.BoxPos.Height - 1, Offset, Shape.ShapeType.Box) { ContainerParent = box.ContainerParent };
            }
        }
    }
    internal class Gamefield : ComplexDrawable
    {
        private readonly Project project;
        private readonly GameBase game;
        private readonly ContainerConsole display;
        private AspectRatioContainer aspectRatioContainer;
        private int score = 0;
        private int oldScore = -1;
        public Gamefield(AspectRatioContainer gameFieldContainer, GameBase gameBase, IDesktopHost desktopHost, Project project, ContainerConsole display) : base(desktopHost)
        {
            this.display = display;
            aspectRatioContainer = gameFieldContainer;
            this.project = project;
            game = gameBase;
            SplitContainer rasterY = new SplitContainer(new(0, 0), SplitContainer.SplitContainerOptions.DontSplitAtY);
            AlreadySplitContainer[] rasterYSub = new AlreadySplitContainer[GameBase.GamefieldLength];

            for (int i = 0; i < rasterYSub.Length; i++)
            {
                SplitContainer rasterX = new(new(0, 0), SplitContainer.SplitContainerOptions.DontSplitAtX);
                rasterYSub[i] = new()
                {
                    ContainerChildren = new[] { rasterX }
                };
                AlreadySplitContainer[] rasterXSub = new AlreadySplitContainer[GameBase.GamefieldHeight];
                for (int j = 0; j < GameBase.GamefieldHeight; j++)
                {
                    rasterXSub[j] = new();
                    var tilePointer = new TilePointer(null);
                    gameBase.rendered[i, j] = tilePointer;
                    rasterXSub[j].DrawableChildren = new[] { tilePointer };
                }
                rasterX.ContainerChildren = rasterXSub;
            }

            gameBase.rendered[3, 5].Tile = new(new(' ', ConsoleColor.Black, ConsoleColor.DarkRed), new(0, 0, BoxPos.Pos.UpLeftCorner));




            rasterY.ContainerChildren = rasterYSub;
            gameFieldContainer.ContainerChildren = new[] { rasterY };
            gameBase.CreateHand();
        }

        public override void DrawDrawable(IDrawer currentFrame)
        {
            aspectRatioContainer.BoxPos.RenderDrawBox(currentFrame, ConsoleColor.White);
        }
        private double timeSinceLastDrop = 0;

        private const double TIME_SINCE_LAST_DROP_MAX = 20; //Every 20 frames

        private List<KeyInfo> keyQueue = new(2);

        private object keyLock = new object();
        protected override void OnKeyPressed(KeyInfo keyInfo)
        {
            lock (keyLock)
            {
                keyQueue.Add(keyInfo);
            }
        }
        public override void UpdateDrawable()
        {
            game.CreateRender();
            timeSinceLastDrop += project.DeltaTime;
            if (timeSinceLastDrop > TIME_SINCE_LAST_DROP_MAX)
            {
                timeSinceLastDrop -= TIME_SINCE_LAST_DROP_MAX;
                if (!game.TryMove(0, 1))
                {
                    game.PlaceHand();
                    score += game.ClearAllCompleteLines();
                    game.CreateHand();
                }
            }
            lock (keyLock)
            {
                foreach (var key in keyQueue)
                {
                    switch (char.ToLower(key.key))
                    {
                        case 'e':
                            game.TrySpin(GameBase.RotationType.Clockwise);
                            break;
                        case 'q':
                            game.TrySpin(GameBase.RotationType.Counterclockwise);
                            break;
                        case 'a':
                            game.TryMove(-1, 0);
                            break;
                        case 'd':
                            game.TryMove(1, 0);
                            break;
                        case 's':
                            if (!game.TryMove(0, 1))
                            {
                                game.PlaceHand();
                                score += game.ClearAllCompleteLines();
                                game.CreateHand();
                            }
                            break;
                        case 'c':
                            game.ClearAllCompleteLines();
                            break;
                        case ' ':
                            while (game.TryMove(0, 1)) { }
                            game.PlaceHand();
                            score += game.ClearAllCompleteLines();
                            game.CreateHand();
                            break;
                    }
                }
                keyQueue.Clear();
            }
            if (oldScore != score)
            {
                oldScore = score;
                display.ClearAll();
                display.WriteLine($"Score: {score}", ConsoleColor.White, ConsoleColor.Black);
            }
        }

        protected override void FinishInitialising()
        {
        }

        protected override void OnRationChanged()
        {
        }
    }
}
