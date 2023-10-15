using ConsoleGUI.String;
using System.Diagnostics;
using static Tetris.Block;

namespace Tetris
{



    public class Block
    {
        public BetterChar renderChar;
        public BlockTileOffset[][] blockTileOffset;
        public short blockID;
        public Block(BetterChar renderChar, BlockTileOffset[][] blockTileOffset, short blockID)
        {
            this.renderChar = renderChar;
            this.blockTileOffset = blockTileOffset;
            this.blockID = blockID;
        }

        public struct BlockTileOffset
        {
            public int x, y;
            public BlockTileOffset(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }

    internal class GameOver : Exception { }
    internal class GameBase
    {
        public const int GamefieldLength = 10;
        public const int GamefieldHeight = 16;

        public Tile[,] baseTiles = new Tile[GamefieldLength, GamefieldHeight];

        public GameBase()
        {
            /*
            for (int i = 0; i < GamefieldLength; i++)
            {
                for (int j = 0; j < GamefieldHeight; j++)
                {
                    baseTiles[i, j] = new(Tile.EmptyChar, new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner));
                }
            }
            */
        }

        public TilePointer[,] rendered = new TilePointer[GamefieldLength, GamefieldHeight];
        public short handX, handY, handRotation;
        public short predictionX, predictionY;
        public bool showPrediction;
        public bool UpdatePrediction { get; private set; }
        public Block? handBlock;
        private readonly BetterChar predictionChar = new('.', ConsoleColor.White, ConsoleColor.Black);


        public void CreatePrediction()
        {
            if (handBlock == null)
            {
                return;
            }
            UpdatePrediction = false;
            predictionX = handX;
            predictionY = handY;
            while(!CheckCollision(predictionX, predictionY, handRotation, handBlock, baseTiles))
            {
                predictionY++;
            }
            predictionY--;
        
        }

        public void RenderPrediction()
        {
            foreach (var tile in handBlock.blockTileOffset[handRotation])
            {

                if (rendered[predictionX + tile.x, predictionY + tile.y].Tile == null)
                    rendered[predictionX + tile.x, predictionY + tile.y].Tile = new(predictionChar, new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner));
                else
                    rendered[predictionX + tile.x, predictionY + tile.y].Tile.RenderChar = predictionChar;


            }
        }
        public void CreateRender()
        {


            for (int i = 0; i < GamefieldLength; i++)
            {
                for (var j = 0; j < GamefieldHeight; j++)
                {
                    rendered[i, j].Tile = baseTiles[i, j];
                }
            }
            RenderPrediction();
            foreach (var tile in handBlock.blockTileOffset[handRotation])
            {

                if (rendered[handX + tile.x, handY + tile.y].Tile == null)
                    rendered[handX + tile.x, handY + tile.y].Tile = new(handBlock.renderChar, new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner));
                else
                    rendered[handX + tile.x, handY + tile.y].Tile.RenderChar = handBlock.renderChar;


            }

        }

        public void CreateHand(int? handID = null)
        {
            UpdatePrediction = true;
            handID ??= Random.Shared.Next(0, blocks.Length);
            handX = GamefieldLength / 2;
            handY = 0;
            handRotation = 0;
            handBlock = blocks[handID ?? 0];
            while (CheckCollision(handX, handY, handRotation, handBlock, baseTiles))
            {
                handY++;
                if (handY < 0)
                    throw new GameOver();
            }
        }


        public void MoveDownBaseAtY(int y)
        {
            for (; y > 0; y--)
            {
                for (int x = 0; x < GamefieldLength; x++)
                {

                    if (y - 1 < 0)
                        baseTiles[x, y].RenderChar = Tile.EmptyChar;
                    else
                        baseTiles[x, y] = baseTiles[x, y - 1];


                }
            }
        }


        public int ClearAllCompleteLines()
        {
            UpdatePrediction = true;

            ushort completedLineAmt = 0;
            for (int y = 0; y < GamefieldHeight; y++)
            {
                bool lineComplete = true;
                for (int x = 0; x < GamefieldLength; x++)
                {
                    if (baseTiles[x, y] == null)
                        lineComplete = false;
                }
                if (lineComplete)
                {
                    completedLineAmt++;
                    MoveDownBaseAtY(y);
                }

            }
            return completedLineAmt * 15 * completedLineAmt;
        }
        public Block GetRandomBlock()
        {
            return blocks[Random.Shared.Next(0, blocks.Length - 1)];
        }

        public static void AddBlockToArray(Tile[,] array, Block block, ushort x, ushort y, ushort rotationIndex, int offsetX, int offsetY)
        {
            foreach (var tile in block.blockTileOffset[rotationIndex])
            {
                array[x + tile.x, y + tile.y] = new(block.renderChar, new(offsetX, offsetY, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner));
            }
        }

        public bool TryMove(short movX, short movY)
        {
            if (movX != 0)
                UpdatePrediction = true;


            var movedX = (short)(handX + movX);
            var movedY = (short)(handY + movY);
            if (!CheckCollision(movedX, movedY, handRotation, handBlock ?? throw new InvalidOperationException("Can't move a hand block while it's not initialised"), baseTiles))
            {



                handX = movedX;
                handY = movedY;
                return true;
            }
            return false;
        }
        public enum RotationType
        {
            Counterclockwise,
            Clockwise
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotationType"></param>
        /// <param name="useSRS"></param>
        /// <returns>Whether the rotation was successful</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool TrySpin(RotationType rotationType, bool useSRS = true)
        {
            short newRotIndex = handRotation;
            checked
            {
                if (rotationType == RotationType.Clockwise)
                {
                    newRotIndex += 1;
                    if (newRotIndex > 3)
                        newRotIndex = 0;
                }
                else
                {
                    if (newRotIndex == 0)
                        newRotIndex = 3;
                    else
                        newRotIndex -= 1;

                }
            }
            if (!CheckCollision(handX, handY, newRotIndex, handBlock ?? throw new InvalidOperationException("Can't move a hand block while it's not initialised"), baseTiles))
            {
                UpdatePrediction = true;

                handRotation = newRotIndex;
                return true;
            }
            if (!useSRS)
                return false;
            var testTable = sRSTests.FirstOrDefault(x => x.applicableBlocks.Contains(handBlock.blockID));
            if (testTable == null)
            {
                Debug.Assert(false);
                return false; //No fitting Srs test table was found 
            }
            var testArray = testTable.testArray.FirstOrDefault(x => x.rotationStateFrom == handRotation && x.rotationStateTo == newRotIndex);
            if (testArray == null)
            {
                Debug.Assert(false);
                return false; //No fitting test case
            }

            foreach (var test in testArray.tests)
            {
                if (!CheckCollision((short)(handX + test.Item1), (short)(handY + test.Item2), newRotIndex, handBlock ?? throw new InvalidOperationException("Can't move a hand block while it's not initialised"), baseTiles))
                {
                    int newHandX = handX + test.Item1;
                    if (newHandX < 0)
                        continue;
                    handX = (short)newHandX;
                    int newHandY = handY + test.Item2;
                    if (newHandY < 0)
                        continue;
                    handY = (short)newHandY;
                    handRotation = newRotIndex;
                    UpdatePrediction = true;
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rotationIndex"></param>
        /// <param name="block"></param>
        /// <param name="checkTiles"></param>
        /// <returns>Whether there was a collision</returns>
        public static bool CheckCollision(short x, short y, short rotationIndex, Block block, Tile?[,] checkTiles)
        {

            foreach (var tile in block.blockTileOffset[rotationIndex])
            {
                if (x + tile.x > GamefieldLength - 1 || x + tile.x < 0)
                    return true;
                if (y + tile.y > GamefieldHeight - 1 || y + tile.y < 0)
                    return true;
                if (checkTiles[x + tile.x, y + tile.y] != null) //There's an overlap
                    return true;
            }
            return false;
        }

        internal void PlaceHand()
        {
            foreach (var tile in handBlock.blockTileOffset[handRotation])
            {
                baseTiles[handX + tile.x, handY + tile.y] = new(handBlock.renderChar, new(0, 0, ConsoleGUI.Positional.BoxPos.Pos.UpLeftCorner));
            }
        }

        private class SRSTestTable
        {

            public class SRSTestArray
            {
                public (short, short)[] tests;
                public ushort rotationStateFrom, rotationStateTo;

                public SRSTestArray(ushort rotationStateFrom, ushort rotationStateTo, (short, short)[] tests)
                {
                    this.tests = tests;
                    this.rotationStateFrom = rotationStateFrom;
                    this.rotationStateTo = rotationStateTo;
                }
            }

            public SRSTestArray[] testArray;
            public short[] applicableBlocks;

            public SRSTestTable(short[] applicableBlocks, SRSTestArray[] sRSTestArray)
            {
                testArray = sRSTestArray;
                this.applicableBlocks = applicableBlocks;
            }
        }


        /// <summary>
        /// list of srs checks according to https://tetris.wiki/Super_Rotation_System#Wall_Kicks
        /// </summary>
        private SRSTestTable[] sRSTests = new SRSTestTable[]
        {
            new( new short[] //J, L, S, T, Z Tetromino Wall Kick Data
            {
                1,2,3,4,5,6
            },
                new SRSTestTable.SRSTestArray[]
            {
                new(0, 1, new (short,short)[] { ( 0, 0),  (-1, 0) ,(-1,+1) ,( 0,-2) ,(-1,-2) }), //0->R	( 0, 0)	(-1, 0)	(-1,+1)	( 0,-2)	(-1,-2)
                new(1, 0, new (short,short)[] { ( 0, 0),  (+1, 0), (+1,-1) ,( 0,+2), (+1,+2) }), //R->0	( 0, 0)	(+1, 0)	(+1,-1)	( 0,+2)	(+1,+2)
                new(1, 2, new (short,short)[] { ( 0, 0), (+1, 0), (+1,-1), ( 0,+2), (+1,+2) }), //R->2	( 0, 0)	(+1, 0)	(+1,-1)	( 0,+2)	(+1,+2)
                new(2, 1, new (short,short)[] { ( 0, 0), (-1, 0), (-1,+1), ( 0,-2), (-1,-2) }), //2->R	( 0, 0)	(-1, 0)	(-1,+1)	( 0,-2)	(-1,-2)
                new(2, 3, new (short,short)[] { ( 0, 0), (-1, 0), (-1,+1), ( 0,-2), (-1,-2) }), //2->L	( 0, 0)	(+1, 0)	(+1,+1)	( 0,-2)	(+1,-2)
                new(3, 2, new (short,short)[] { ( 0, 0), (-1, 0), (-1,-1), ( 0,+2), (-1,+2) }), //L->2	( 0, 0)	(-1, 0)	(-1,-1)	( 0,+2)	(-1,+2)
                new(3, 0, new (short,short)[] { ( 0, 0), (-1, 0), (-1,-1), ( 0,+2), (-1,+2) }), //L->0	( 0, 0)	(-1, 0)	(-1,-1)	( 0,+2)	(-1,+2)
                new(0, 3, new (short,short)[] { ( 0, 0), (+1, 0), (+1,+1), ( 0,-2), (+1,-2) }), //0->L	( 0, 0)	(+1, 0)	(+1,+1)	( 0,-2)	(+1,-2)
            }),
            new( new short[] //I Tetromino Wall Kick Data
            {
                0
            },
                new SRSTestTable.SRSTestArray[]
            {
                new(0, 1, new (short,short)[] { ( 0, 0), (-2, 0), (+1, 0), (-2,-1), (+1,+2) }), //0->R	( 0, 0)	(-2, 0)	(+1, 0)	(-2,-1)	(+1,+2)
                new(1, 0, new (short,short)[] { ( 0, 0), (+2, 0), (-1, 0), (+2,+1), (-1,-2) }), //R->0	( 0, 0)	(+2, 0)	(-1, 0)	(+2,+1)	(-1,-2)
                new(1, 2, new (short,short)[] { ( 0, 0), (-1, 0), (+2, 0), (-1,+2), (+2,-1) }), //R->2	( 0, 0)	(-1, 0)	(+2, 0)	(-1,+2)	(+2,-1)
                new(2, 1, new (short,short)[] { ( 0, 0), (+1, 0), (-2, 0), (+1,-2), (-2,+1) }), //2->R	( 0, 0)	(+1, 0)	(-2, 0)	(+1,-2)	(-2,+1)
                new(2, 3, new (short,short)[] { ( 0, 0), (+2, 0), (-1, 0), (+2,+1), (-1,-2) }), //2->L	( 0, 0)	(+2, 0)	(-1, 0)	(+2,+1)	(-1,-2)
                new(3, 2, new (short,short)[] { ( 0, 0), (-2, 0), (+1, 0), (-2,-1), (+1,+2) }), //L->2	( 0, 0)	(-2, 0)	(+1, 0)	(-2,-1)	(+1,+2)
                new(3, 0, new (short,short)[] { ( 0, 0), (+1, 0), (-2, 0), (+1,-2), (-2,+1) }), //L->0	( 0, 0)	(+1, 0)	(-2, 0)	(+1,-2)	(-2,+1)
                new(0, 3, new (short,short)[] { ( 0, 0), (-1, 0), (+2, 0) ,(-1,+2), (+2,-1) }), //0->L	( 0, 0)	(-1, 0)	(+2, 0)	(-1,+2)	(+2,-1)
            })
        };


        /*
        SRS

        I Tetromino Wall Kick Data
Test 1	Test 2	Test 3	Test 4	Test 5
0->R	( 0, 0)	(-2, 0)	(+1, 0)	(-2,-1)	(+1,+2)
R->0	( 0, 0)	(+2, 0)	(-1, 0)	(+2,+1)	(-1,-2)
R->2	( 0, 0)	(-1, 0)	(+2, 0)	(-1,+2)	(+2,-1)
2->R	( 0, 0)	(+1, 0)	(-2, 0)	(+1,-2)	(-2,+1)
2->L	( 0, 0)	(+2, 0)	(-1, 0)	(+2,+1)	(-1,-2)
L->2	( 0, 0)	(-2, 0)	(+1, 0)	(-2,-1)	(+1,+2)
L->0	( 0, 0)	(+1, 0)	(-2, 0)	(+1,-2)	(-2,+1)
0->L	( 0, 0)	(-1, 0)	(+2, 0)	(-1,+2)	(+2,-1)
        0 = rot0
        R = rot1
        2 = rot2
        L = rot3
        J, L, S, T, Z Tetromino Wall Kick Data
Test 1	Test 2	Test 3	Test 4	Test 5
0->R	( 0, 0)	(-1, 0)	(-1,+1)	( 0,-2)	(-1,-2)

R->0	( 0, 0)	(+1, 0)	(+1,-1)	( 0,+2)	(+1,+2)

R->2	( 0, 0)	(+1, 0)	(+1,-1)	( 0,+2)	(+1,+2)

2->R	( 0, 0)	(-1, 0)	(-1,+1)	( 0,-2)	(-1,-2)

2->L	( 0, 0)	(+1, 0)	(+1,+1)	( 0,-2)	(+1,-2)

L->2	( 0, 0)	(-1, 0)	(-1,-1)	( 0,+2)	(-1,+2)

L->0	( 0, 0)	(-1, 0)	(-1,-1)	( 0,+2)	(-1,+2)

0->L	( 0, 0)	(+1, 0)	(+1,+1)	( 0,-2)	(+1,-2)





        */

        /// <summary>
        /// All block states
        /// </summary>
        public Block[] blocks = new[]
        {
            new Block(new('I', ConsoleColor.Cyan, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(0,1),
                        new(1,1),
                        new(2,1),
                        new(3,1),
                    },
                    new BlockTileOffset[]
                    {
                        new(2,0),
                        new(2,1),
                        new(2,2),
                        new(2,3),
                    },
                    new BlockTileOffset[]
                    {
                        new(0,2),
                        new(1,2),
                        new(2,2),
                        new(3,2),
                    },
                    new BlockTileOffset[]
                    {
                        new(1,0),
                        new(1,1),
                        new(1,2),
                        new(1,3),
                    },

                }, 0
            ),
                new Block(new('J', ConsoleColor.Blue, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(0,1),
                        new(2,1),
                        new(1,1),
                    },
                    new BlockTileOffset[]
                    {
                        new(2,0),
                        new(1,0),
                        new(1,1),
                        new(1,2),
                    },
                    new BlockTileOffset[]
                    {
                        new(2,2),
                        new(0,1),
                        new(2,1),
                        new(1,1),
                    },
                    new BlockTileOffset[]
                    {
                        new(0,2),
                        new(1,0),
                        new(1,1),
                        new(1,2),
                    },

                }, 1
            ),
                new Block(new('L', ConsoleColor.DarkRed, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(2,0),
                        new(0,1),
                        new(2,1),
                        new(1,1),
                    },
                    new BlockTileOffset[]
                    {
                        new(2,2),
                        new(1,0),
                        new(1,1),
                        new(1,2),
                    },
                    new BlockTileOffset[]
                    {
                        new(0,2),
                        new(0,1),
                        new(2,1),
                        new(1,1),
                    },
                    new BlockTileOffset[]
                    {
                        new(1,2),
                        new(1,0),
                        new(1,1),
                        new(0,0),
                    },


                } ,2
            ),
                 new Block(new('o', ConsoleColor.DarkYellow, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(0,1),
                        new(1,0),
                        new(1,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(0,1),
                        new(1,0),
                        new(1,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(0,1),
                        new(1,0),
                        new(1,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(0,1),
                        new(1,0),
                        new(1,1),

                    },


                }, 3
            ),
                  new Block(new('S', ConsoleColor.Green, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(1,0),
                        new(2,0),
                        new(0,1),
                        new(1,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(1,0),
                        new(1,1),
                        new(2,1),
                        new(2,2),

                    },
                    new BlockTileOffset[]
                    {
                        new(1,1),
                        new(2,1),
                        new(0,2),
                        new(1,2),

                    },
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(0,1),
                        new(1,1),
                        new(1,2),

                    },


                }, 4
            ),
                  new Block(new('T', ConsoleColor.Magenta, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(0,1),
                        new(1,0),
                        new(1,1),
                        new(2,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(1,0),
                        new(1,1),
                        new(1,2),
                        new(2,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(0,1),
                        new(1,2),
                        new(1,1),
                        new(2,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(1,0),
                        new(1,1),
                        new(1,2),
                        new(0,1),

                    },


                }, 5

            ),
                  new Block(new('Z', ConsoleColor.Red, ConsoleColor.Black),
                new[]
                {
                    new BlockTileOffset[]
                    {
                        new(0,0),
                        new(1,0),
                        new(1,1),
                        new(2,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(2,0),
                        new(1,1),
                        new(1,2),
                        new(2,1),

                    },
                    new BlockTileOffset[]
                    {
                        new(0,1),
                        new(1,2),
                        new(1,1),
                        new(2,2),

                    },
                    new BlockTileOffset[]
                    {
                        new(1,0),
                        new(0,1),
                        new(0,2),
                        new(1,1),

                    },


                }, 6

            ),
        };

    }
}




