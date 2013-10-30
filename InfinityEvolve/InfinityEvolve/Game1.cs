using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace InfinityEvolve
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Entity[,] grid;

        Texture2D line;

        int SCREEN_WIDTH;
        int SCREEN_HEIGHT;

        int AVERAGES_HEIGHT_COORDINATE = 175;
        int AVERAGES_PANEL_HEIGHT = 390;
        int AVERAGES_BAR_OFFSET_WIDTH = 0;

        int SIDE_PANEL_WIDTH;// = 175;

        int TRUNCATED_STRING_LIMIT = 20;

        int TILE_WIDTH;
        int TILE_HEIGHT;

        Random rng;

        Texture2D antSprite;
        Texture2D antSpriteAlt;
        Texture2D foodSprite;
        Texture2D backgroundSprite;
        Texture2D cursorSprite;

        SpriteFont font;
        SpriteFont statMenuFont;

        SpriteSheet cursor;

        string debugMessage;
        double lastFoodSpawnTime;

        double foodSpawnInterval; // how many seconds before food should respawn
        int numberOfFoodsSpawned; // how many foods made
        int LINE_THICKNESS;
        int GRID_WIDTH;
        int GRID_HEIGHT;
        int STARTING_ANTS;

        double numberOfSecondsBeforeRoundEnds;

        double secondsCounter; // number of seconds before statistics screen transitions to ants screen
        Statistics.GameState gameState;

        Dictionary<string, Vector2> attributeRange; //TODO: use this to get min-max of a certain values INCLUSIVE
        Dictionary<string, double> attributes;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "Infinity 2013: Ant Evolution Game by Jonathan Tang";

            // full screen code
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferMultiSampling = false;
            graphics.IsFullScreen = true;

            //IsMouseVisible = true;
        }

        // todo: keep track of the last generation of ants that were alive... then average out
        //       all of their characteristics at the end, and analyze the difference between
        //       the starting 10 ants and the last generation of ants

        //  todo: implement DISEASE factor; it's a green spot that when stepped on produces disease
                   //  that wears down an ant's health. it can be passed on randomly to adjacent ants
        //             and it becomes more severe with time until the ants die.

   
        protected override void Initialize()
        {
            debugMessage = "";
            lastFoodSpawnTime = 0;

            //seeded by time
            rng = new Random((int)DateTime.Now.Ticks);

            //  TODO: store generations in an array in Statistics

            //  TODO: represent ants that are dead with a new icon

            foodSpawnInterval = 10; // how many seconds before food should respawn
            GRID_WIDTH = (3 * ((int) (7* (rng.NextDouble() * .7 + .3))));
            GRID_HEIGHT = (int)((2f/3f)*GRID_WIDTH);

            STARTING_ANTS = (int)((rng.NextDouble()*.15)*GRID_WIDTH*GRID_HEIGHT) + 2;
            if (STARTING_ANTS > 20) STARTING_ANTS = 20;

            numberOfFoodsSpawned = (int)((rng.NextDouble()*.10)*GRID_WIDTH*GRID_HEIGHT)+1; // how many foods made

            Statistics.EXTREME_MUTATION_CHANCE = rng.NextDouble() * .5;
            Statistics.runTimeInSeconds = 0;

            LINE_THICKNESS = 0;
            gameState = Statistics.GameState.ANTS_SCREEN;
            secondsCounter = 30.0;

            cursor = new SpriteSheet(Content.Load<Texture2D>("cursorSheet"), 50, 50);
            cursor.SetScale(TILE_WIDTH, TILE_HEIGHT);
            Statistics.birthEffects = new List<BirthEffect>();
            Statistics.foodEffects = new List<FoodEffect>();
            Statistics.foodFades = new List<FoodFade>();

            numberOfSecondsBeforeRoundEnds = 100;

            // TODO: remember to check that ALL statistics.attributes are refreshed
            // TODO: have a Dictionary that returns a list of ants associated with a particular generation number
            Statistics.numberOfMovements = 0;
            Statistics.longestAgeLived = 0;
            Statistics.largestNumberOfChildren = 0;
            Statistics.largestNumberOfFoodsEaten = 0;
            Statistics.topFiveAncestors = new List<Ant>();
            Statistics.largestPopulation = 0;
            Statistics.longestGenerationLived = 0;

            Statistics.timeOfLastMovement = 0;
            Statistics.systemHasMoved = false;

            Statistics.totalFoods = 0;
            Statistics.totalNumberOfAnts = 0;

            grid = new Entity[GRID_WIDTH, GRID_HEIGHT];

            SIDE_PANEL_WIDTH = (int) (graphics.GraphicsDevice.Viewport.Width * .2f);

            SCREEN_WIDTH = graphics.GraphicsDevice.Viewport.Width - SIDE_PANEL_WIDTH;
            SCREEN_HEIGHT = graphics.GraphicsDevice.Viewport.Height;

            TILE_WIDTH = SCREEN_WIDTH / GRID_WIDTH;
            TILE_HEIGHT = SCREEN_HEIGHT / GRID_HEIGHT;

            attributes = new Dictionary<string, double>();

            Statistics.attributeRange = new Dictionary<string, Vector2>();
            attributeRange = Statistics.attributeRange;

            Statistics.firstAverages = new Dictionary<string, double>();
            Statistics.currentSum = new Dictionary<string, double>();

            attributes["redColor"] = 100; //RGB color scheme
            attributeRange["redColor"] = new Vector2(0, 255);

            attributes["blueColor"] = 50;
            attributeRange["blueColor"] = new Vector2(0, 255);

            attributes["greenColor"] = 70;
            attributeRange["greenColor"] = new Vector2(0, 255);

            attributes["mutationFactor"] = .2;
            attributeRange["mutationFactor"] = new Vector2(0, 1);
            attributes["walkSpeed"] = 1000.0; //  how many milliseconds it takes to move a single step
            attributeRange["walkSpeed"] = new Vector2(100, 2000);

            attributes["desireForFood"] = 100.0; //  how much this ant prioritizes collecting food
            attributeRange["desireForFood"] = new Vector2(-4000, 4000);
            attributes["desireToFollowAnts"] = 10.0; //  how much this ant is willing to follow/avoid ants
            attributeRange["desireToFollowAnts"] = new Vector2(-300, 300);
            attributes["desireToExplore"] = 1.0; //  how much this ant is willing to explore new places
            attributeRange["desireToExplore"] = new Vector2(-200, 200);
            attributes["desireToMate"] = 500.0;   // influenced by number of children
            attributeRange["desireToMate"] = new Vector2(-600, 600);

            attributes["desireToContinueInSameDirection"] = 1500.0; //  prevents equal-sum scenarios
            attributeRange["desireToContinueInSameDirection"] = new Vector2(-2000, 2000);
            attributes["desireToGoBackwards"] = -1000.0; //  prevents oscillation
            attributeRange["desireToGoBackwards"] = new Vector2(-1500, 1500);

            attributes["timeToReexplore"] = 10.0; //  how long (seconds) an ant remembers a spot
            attributeRange["timeToReexplore"] = new Vector2(1, 30);
            attributes["decayFactor"] = 4.0;  //  analogous to the ant's sense of smell
            attributeRange["decayFactor"] = new Vector2(1, 8);
            attributes["healthThreshold"] = 200.0; //  if another ant meets this health threshold,
                                                   //   then the two will mate and produce offspring
            attributeRange["healthThreshold"] = new Vector2(1, 400);

            attributes["childbirthPercentage"] = 21.0; // % chance of having child when threshold met
            attributeRange["childbirthPercentage"] = new Vector2(1, 100);
            attributes["childbirthCost"] = 50.0; //  how much the parent pays to raise child
            attributeRange["childbirthCost"] = new Vector2(1, 300);
            attributes["childbirthCooldown"] = 10.0; //  how many seconds before having a child again
            attributeRange["childbirthCooldown"] = new Vector2(5, 30);

            //attributes["health"] = 150.0;     //  health is restored by eating food,
                                              //  and it is lowered by moving around and taking damage
            //attributeRange["health"] = new Vector2(0, 300);

            //attributes["strength"] = 10.0; //  TODO: damage dealt in a hit to a predator. influences
                                           //  construction of buildings, too

            attributes["oldestPossibleAge"] = 200.0; //  number of moves this ant can make before dying
            attributeRange["oldestPossibleAge"] = new Vector2(30, 400);

            attributes["eyesightRange"] = 100.0; //  number of tiles this ant can see
            attributeRange["eyesightRange"] = new Vector2(3, 150);

            //attributes["resistance"] = 5.0; //  TODO: immunity to cold, poison, and disease

            //       also, social model vs individualistic model. social model is a shared diffusion.

            //attributes["shelterShape"] = 22.0; // TODO: this number gets rounded down when used in calculation.
                                               // base-2 representation of the truncated version is
                                               // used. each digit represents having or not having
                                               // a block in a certain location in a 5x5 grid. 
                                               // digit ordering as follows: (1 = first digit's block location)
                                               // and alphabetical chars correspond to numbers beyond 9

            //  22 = 10110(b2) = _ 
            //                 _|  shape

            /*   PLDEM
             *   KC56F
             *   B4127
             *   JA38G
             *   OI9HN
             */

            line = new Texture2D(graphics.GraphicsDevice, 1, 1);
            line.SetData(new Color[] { Color.White });

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            antSprite = Content.Load<Texture2D>("antRealistic");
            antSpriteAlt = Content.Load<Texture2D>("antTransparent2");
            foodSprite = Content.Load<Texture2D>("food");
            backgroundSprite = Content.Load<Texture2D>("greenNoise");
            cursorSprite = Content.Load<Texture2D>("cursorSheet2");

            font = Content.Load<SpriteFont>("font");
            statMenuFont = Content.Load<SpriteFont>("statMenuFont");

            // initialize
            foreach (KeyValuePair<string, double> kvp in attributes)
            {
                Statistics.firstAverages[kvp.Key] = 0;
                Statistics.currentSum[kvp.Key] = 0;
            }

            for (int i = 0; i < STARTING_ANTS; i++)
            {
                Vector2 pos = new Vector2((int) rng.Next(GRID_WIDTH), (int) rng.Next(GRID_HEIGHT));

                //  half of the starting ants are mutants, half are not
                Ant ant = new Ant(attributes, rng, grid, i%2==0);
                grid[(int)pos.X, (int)pos.Y] = ant;

                foreach (KeyValuePair<string, double> kvp in attributes)
                {
                    Statistics.firstAverages[kvp.Key] += ant.Attributes()[kvp.Key] / STARTING_ANTS;
                }

                grid[(int)pos.X, (int)pos.Y].SetPosition(pos);
                grid[(int)pos.X, (int)pos.Y].SetSprite(antSprite);
            }
            string trunc = "";
            for (int i = 0; i < TRUNCATED_STRING_LIMIT; i++)
            {
                trunc += "a";
            }
            AVERAGES_BAR_OFFSET_WIDTH = (int)statMenuFont.MeasureString(trunc).X;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            else if (keyboardState.IsKeyDown(Keys.Space))
            {
                Statistics.totalNumberOfAnts = 0;
            }

            if (gameState == Statistics.GameState.ANTS_SCREEN)
            {
                numberOfSecondsBeforeRoundEnds -= gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;

                Statistics.runTimeInSeconds += gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;

                if (lastFoodSpawnTime == 0 || gameTime.TotalGameTime.TotalSeconds - lastFoodSpawnTime >= foodSpawnInterval)
                {
                    for (int i = 0; i < numberOfFoodsSpawned; i++)
                    {
                        Vector2 pos = new Vector2((int)rng.Next(GRID_WIDTH), (int)rng.Next(GRID_HEIGHT));

                        //  don't spawn food on top of other ants or foods
                        if (grid[(int)pos.X, (int)pos.Y] == null)
                        {
                            grid[(int)pos.X, (int)pos.Y] = new Food(rng);

                            grid[(int)pos.X, (int)pos.Y].SetPosition(pos);
                            grid[(int)pos.X, (int)pos.Y].SetSprite(foodSprite);

                            Statistics.totalFoods++;
                        }
                    }

                    lastFoodSpawnTime = gameTime.TotalGameTime.TotalSeconds;
                }

                foreach (Entity e in grid)
                {
                    if (e is Ant)
                    {
                        ((Ant)e).Update(grid, gameTime, rng);
                        //  note: update will null an ant that is dead
                    }

                    if (e != null)
                    {
                        char move = e.CreateMovement(grid, gameTime.TotalGameTime.TotalMilliseconds);
                        MoveInDirection(e, move);
                    }
                }

                if (Statistics.totalNumberOfAnts == 0 || 
                    (Math.Abs(gameTime.TotalGameTime.TotalSeconds - Statistics.timeOfLastMovement) > 30 && Statistics.systemHasMoved))
                {
                    //  if there have been 30 seconds w/o movement, then end
                    gameState = Statistics.GameState.STATISTICS_SCREEN;
                }
            }

            base.Update(gameTime);
        }

        void MoveInDirection(Entity e, char move)
        {
            int x = (int) e.Position().X;
            int y = (int) e.Position().Y;

            if (move == '0')
            {
                return;
            }
            else if (move == 'N' && y - 1 >= 0)
            {
                grid[x, y - 1] = e; //  move piece
                grid[x, y] = null;  //  eliminate old piece
                e.SetPosition(x, y-1);
            }
            else if (move == 'S' && y + 1 < GRID_HEIGHT)
            {
                grid[x, y + 1] = e; //  move piece
                grid[x, y] = null;  //  eliminate old piece
                e.SetPosition(x, y + 1);
            }
            else if (move == 'W' && x - 1 >= 0)
            {
                grid[x - 1, y] = e; //  move piece
                grid[x, y] = null;  //  eliminate old piece
                e.SetPosition(x - 1, y);
            }
            else if (move == 'E' && x + 1 < GRID_WIDTH)
            {
                grid[x + 1, y] = e; //  move piece
                grid[x, y] = null;  //  eliminate old piece
                e.SetPosition(x + 1, y);
            }
        }

        string TruncatedForm(string str)
        {
            if (str.Count() <= TRUNCATED_STRING_LIMIT)
            {
                return str;
            }
            int tmp = TRUNCATED_STRING_LIMIT / 2;
            return str.Substring(0, tmp - 3) 
                + "..." + str.Substring(str.Count() - tmp, tmp);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundSprite, new Vector2(0, 0), new Color(255,255,255,200));

            if (gameState == Statistics.GameState.ANTS_SCREEN)
            {

                spriteBatch.Draw(line, new Rectangle(SCREEN_WIDTH, 0, SIDE_PANEL_WIDTH, SCREEN_HEIGHT), Color.White*.20f);

                spriteBatch.DrawString(font, "Evolving Ants! \nby Jonathan Tang", new Vector2(SCREEN_WIDTH + 5, 5), Color.Black);

                string statMenuString = "Current population: " + Statistics.totalNumberOfAnts
                     + "\nExtreme mutation chance: " + ((int)(Statistics.EXTREME_MUTATION_CHANCE*1000))/1000f
                     + "\nFood spawning: " + numberOfFoodsSpawned + " every " + foodSpawnInterval + " seconds"
                     + "\nGrid size: " + GRID_WIDTH + "x" + GRID_HEIGHT
                     + "\nElapsed time: " + (int)(Statistics.runTimeInSeconds / 60) + " min " + (int)Statistics.runTimeInSeconds%60 + " s";
                spriteBatch.DrawString(statMenuFont, statMenuString, new Vector2(SCREEN_WIDTH + 5, 65), Color.Black);

                //  animate the fade effects for food icons
                #region foodFades
                foreach (FoodFade f in Statistics.foodFades)
                {
                    float coefficient = (float)((FoodFade.FOOD_FADE_TIME - f.TimeCounter())/FoodFade.FOOD_FADE_TIME);
                    if (coefficient < 0) coefficient = 0;

                    f.AddToTimeCounter(gameTime.ElapsedGameTime.TotalMilliseconds/1000f);
                    spriteBatch.Draw(f.Sprite(), new Rectangle((int)f.Position().X * TILE_WIDTH,
                        (int)f.Position().Y * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT),
                        Color.White * coefficient);

                }
                Statistics.foodFades.RemoveAll(f => (f.TimeCounter() >= FoodFade.FOOD_FADE_TIME));
                #endregion

                //  draw all grid elements, including ants
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    for (int j = 0; j < GRID_HEIGHT; j++)
                    {
                        if (grid[i, j] != null)
                        {
                            spriteBatch.Draw(grid[i, j].Sprite(),
                                new Rectangle(i * TILE_WIDTH, j * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT), grid[i, j].RGBColor());
                        }
                    }
                }

                #region drawLines
                for (int x = 1; x <= GRID_WIDTH; x++)
                {
                    //  draw vertical lines
                    spriteBatch.Draw(line, new Rectangle(x * TILE_WIDTH, 0,
                        LINE_THICKNESS, SCREEN_HEIGHT), Color.White);
                }
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    //  draw horizontal lines
                    spriteBatch.Draw(line, new Rectangle(0, y * TILE_HEIGHT,
                        SCREEN_WIDTH, LINE_THICKNESS), Color.Black);
                }
                #endregion

                // debugger message
                spriteBatch.DrawString(font, debugMessage, (new Vector2(0, SCREEN_HEIGHT - 50)), Color.CornflowerBlue);

                #region birthEffects
                //  draw all birth effects, then remove them if they are too old
                foreach (BirthEffect b in Statistics.birthEffects)
                {
                    //  draw a line from each parent to the child ONLY if 
                    //  each parent is still alive (non-null) when this happens

                    if (b.TimeCounter() >= BirthEffect.BIRTH_EFFECT_FADE_TIME)
                    {
                        continue;
                    }

                    Vector2 momPos = b.MomPosition();
                    Vector2 dadPos = b.DadPosition();
                    Vector2 babyPos = b.BabyPosition();

                    int thickness = 3;

                    //  position of the line starts at (x+.5)*Width, (y+.5)*Height
                    //  length of the line is equal to sqrt((x1-x2)^2 + (y1-y2)^2)
                    //  angle theta = arcsin(opposite/hypotenuse)
                    double deltaX = (momPos.X - babyPos.X)*TILE_WIDTH;
                    double deltaY = (momPos.Y - babyPos.Y)*TILE_HEIGHT;
                    Vector2 origin = new Vector2(0, 0);
                    float theta = 0;
                    int momBabyLineLength = (int)Math.Sqrt((deltaX) * (deltaX) + (deltaY) * (deltaY));

                    //  mom section
                    if (!b.Mom().IsDead() && !b.Baby().IsDead())
                    {

                        if (deltaX < 0)
                        {
                            theta = (float)Math.Asin(-deltaY / (momBabyLineLength));
                        }
                        else
                        {
                            theta = (float)Math.PI - (float)Math.Asin(-deltaY / (momBabyLineLength));
                        }

                        float coefficient = (float)((BirthEffect.BIRTH_EFFECT_FADE_TIME - b.TimeCounter())
                            / BirthEffect.BIRTH_EFFECT_FADE_TIME);
                        if (coefficient < 0) coefficient = 0;

                        //  draw line from mom to baby
                        spriteBatch.Draw(line,
                            new Rectangle((int)((momPos.X + .5) * TILE_WIDTH),
                                (int)((momPos.Y + .5) * TILE_HEIGHT), momBabyLineLength, thickness), null,
                            b.EffectColor()*coefficient,
                            theta, origin, SpriteEffects.None, 1f);
                    }

                    //  dad section
                    if (!b.Dad().IsDead() && !b.Baby().IsDead())
                    {
                        deltaX = (dadPos.X - babyPos.X) * TILE_WIDTH;
                        deltaY = (dadPos.Y - babyPos.Y) * TILE_HEIGHT;
                        origin = new Vector2(0, 0);
                        int dadBabyLineLength = (int)Math.Sqrt((deltaX) * (deltaX) + (deltaY) * (deltaY));

                        if (deltaX < 0)
                        {
                            theta = (float)Math.Asin(-deltaY / (dadBabyLineLength));
                        }
                        else
                        {
                            theta = (float)Math.PI - (float)Math.Asin(-deltaY / (dadBabyLineLength));
                        }

                        float coefficient = (float)((BirthEffect.BIRTH_EFFECT_FADE_TIME - b.TimeCounter())
                             / BirthEffect.BIRTH_EFFECT_FADE_TIME);
                        if (coefficient < 0) coefficient = 0;

                        //  draw line from dad to baby
                        spriteBatch.Draw(line,
                            new Rectangle((int)((dadPos.X + .5) * TILE_WIDTH),
                                (int)((dadPos.Y + .5) * TILE_HEIGHT), dadBabyLineLength, thickness), null,
                            b.EffectColor()*coefficient,
                            theta, origin, SpriteEffects.None, 1f);
                    } 

                    b.AddToTimeCounter(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0);

                    //  draw the flashing cursor on the baby's position
                    spriteBatch.Draw(cursorSprite, new Rectangle((int)babyPos.X * TILE_WIDTH, (int)babyPos.Y * TILE_HEIGHT,
                        TILE_HEIGHT, TILE_WIDTH), Color.White);

                    //cursor.SetPosition(new Vector2(babyPos.X * TILE_WIDTH, babyPos.Y * TILE_HEIGHT));
                    //cursor.Update(gameTime);
                    //cursor.Draw(spriteBatch);

                }
                Statistics.birthEffects.RemoveAll(b => (b.TimeCounter() >= BirthEffect.BIRTH_EFFECT_FADE_TIME));
                #endregion

                #region foodEffects
                // draw all food effects, remove them if they are too old
                foreach (FoodEffect f in Statistics.foodEffects)
                {
                    Vector2 antPos = f.AntPosition();
                    Vector2 foodPos = f.FoodPosition();
                    int thickness = 2;

                    //  position of the line starts at (x+.5)*Width, (y+.5)*Height
                    //  length of the line is equal to sqrt((x1-x2)^2 + (y1-y2)^2)
                    double deltaX = (antPos.X - foodPos.X) * TILE_WIDTH;
                    double deltaY = (antPos.Y - foodPos.Y) * TILE_HEIGHT;
                    Vector2 origin = new Vector2(0,0);

                    int lineLength = (int)Math.Sqrt((deltaX) * (deltaX) + (deltaY) * (deltaY));

                    // theta has two solutions due to nature of arcsin function

                    float theta = 0f;
                    if (deltaX < 0)
                    {
                        theta = (float)Math.Asin(-deltaY / (lineLength));
                    }
                    else
                    {
                        theta = (float)Math.PI - (float) Math.Asin(-deltaY / (lineLength));
                    }

                    float coefficient = (float)((FoodEffect.FOOD_EFFECT_FADE_TIME - f.TimeCounter())
                        / FoodEffect.FOOD_EFFECT_FADE_TIME);
                    if (coefficient < 0) coefficient = 0;

                    spriteBatch.Draw(line,
                        new Rectangle((int)((antPos.X + .5) * TILE_WIDTH),
                            (int)((antPos.Y + .5) * TILE_HEIGHT), lineLength, thickness), null,
                        Color.Green*coefficient, theta,
                        origin, SpriteEffects.None, 1f);

                    f.AddToTimeCounter(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0);

                }
                Statistics.foodEffects.RemoveAll(f => (f.TimeCounter() >= FoodEffect.FOOD_EFFECT_FADE_TIME));
                #endregion

                #region attributeStatSection

                spriteBatch.DrawString(font, "Attributes", new Vector2(SCREEN_WIDTH + 5, AVERAGES_HEIGHT_COORDINATE - 5), Color.Black);
                spriteBatch.DrawString(font, "Values", new Vector2(SCREEN_WIDTH + AVERAGES_BAR_OFFSET_WIDTH, AVERAGES_HEIGHT_COORDINATE - 5), Color.Black);

                float sectionHeight = AVERAGES_PANEL_HEIGHT / (float)attributes.Count;
                int counter = 0;
                foreach (KeyValuePair<string, double> kvp in attributes) {
                    counter++;
                    Vector2 minMax = attributeRange[kvp.Key];

                    spriteBatch.DrawString(statMenuFont, TruncatedForm(kvp.Key),
                        new Vector2(SCREEN_WIDTH+5, AVERAGES_HEIGHT_COORDINATE + counter * sectionHeight),
                        Color.Black);

                    int lineWidth = SIDE_PANEL_WIDTH - AVERAGES_BAR_OFFSET_WIDTH - 3;

                    int lineHeight = 3;

                    spriteBatch.Draw(line, new Rectangle(SCREEN_WIDTH + (int)AVERAGES_BAR_OFFSET_WIDTH,
                        AVERAGES_HEIGHT_COORDINATE + counter * (int)sectionHeight + (int)(sectionHeight/2), lineWidth,
                        lineHeight), Color.Blue);

                    int N = Statistics.totalNumberOfAnts;
                    Color currentColor = new Color((int)Statistics.currentSum["redColor"]/N,
                        (int)Statistics.currentSum["greenColor"]/N, (int)Statistics.currentSum["blueColor"]/N);

                    float firstOffset = (float)((Statistics.firstAverages[kvp.Key] - minMax.X)/(minMax.Y - minMax.X))*lineWidth;
                    spriteBatch.Draw(antSpriteAlt, new Rectangle(SCREEN_WIDTH + (int)AVERAGES_BAR_OFFSET_WIDTH + (int)firstOffset,
                        AVERAGES_HEIGHT_COORDINATE + counter * (int)sectionHeight + (int)(sectionHeight / 4) - 2,
                        (int)(sectionHeight/2)+2, (int)(sectionHeight/2)+2), Color.White);

                    float curOffset = (float)((Statistics.currentSum[kvp.Key]/N - minMax.X) / (minMax.Y - minMax.X)) * lineWidth;
                    spriteBatch.Draw(antSpriteAlt, new Rectangle(SCREEN_WIDTH + (int)AVERAGES_BAR_OFFSET_WIDTH + (int)curOffset,
                        AVERAGES_HEIGHT_COORDINATE + counter * (int)sectionHeight + (int)(sectionHeight / 4 -2),
                        (int)(sectionHeight / 2), (int)(sectionHeight / 2)), currentColor);
                #endregion

                    // draw key in side panel

                    float keyRatio = .80f;

                    spriteBatch.DrawString(font, "Key:", new Vector2(SCREEN_WIDTH + 10,
                        SCREEN_HEIGHT * keyRatio - 25), Color.Black);

                    spriteBatch.Draw(antSprite, new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT * keyRatio), Color.Brown);
                    spriteBatch.DrawString(statMenuFont, "Ant", new Vector2(SCREEN_WIDTH + 5 + antSprite.Width,
                        SCREEN_HEIGHT * keyRatio + antSprite.Height / 2 - 10), Color.Black);

                    spriteBatch.Draw(foodSprite, new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT * keyRatio + antSprite.Height + 5), Color.White);
                    spriteBatch.DrawString(statMenuFont, "Food", new Vector2(SCREEN_WIDTH + 5 + foodSprite.Width,
                        SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + foodSprite.Height / 2 - 10), Color.Black);

                    spriteBatch.Draw(line, new Rectangle(SCREEN_WIDTH,
                    (int)(SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + foodSprite.Height + 5), lineWidth / 2,
                    lineHeight), Color.Blue);

                    spriteBatch.Draw(antSpriteAlt, new Rectangle(SCREEN_WIDTH + 5, (int)(SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + foodSprite.Height + 3),
                        (int)(sectionHeight/2)+2,(int)(sectionHeight/2)+2), Color.White);
                    spriteBatch.DrawString(statMenuFont, "First Generation", new Vector2(SCREEN_WIDTH + 5 + foodSprite.Width,
                        SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + 2 * foodSprite.Height / 2), Color.Black);

                    spriteBatch.Draw(line, new Rectangle(SCREEN_WIDTH,
                    (int)(SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + foodSprite.Height + 5 + sectionHeight + 5), lineWidth / 2,
                    lineHeight), Color.Blue);

                    spriteBatch.Draw(antSpriteAlt, new Rectangle(SCREEN_WIDTH + 5, (int)(SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + foodSprite.Height + 5 + sectionHeight + 3),
                        (int)(sectionHeight / 2), (int)(sectionHeight / 2)), currentColor); 
                    spriteBatch.DrawString(statMenuFont, "Current Generation", new Vector2(SCREEN_WIDTH + 5 + foodSprite.Width,
                        SCREEN_HEIGHT * keyRatio + antSprite.Height + 5 + 2 * foodSprite.Height / 2 + sectionHeight), Color.Black);

                }
            }
            else if (gameState == Statistics.GameState.STATISTICS_SCREEN)
            {

                spriteBatch.Draw(line, new Rectangle(0, 0, SCREEN_WIDTH + SIDE_PANEL_WIDTH, SCREEN_HEIGHT), Color.White * .40f);

                string statString = "Total ant movements: " + Statistics.numberOfMovements
                    + "\nLongest ant age: " + Statistics.longestAgeLived
                    + "\nLargest number of children from one particular ant: " + Statistics.largestNumberOfChildren
                    + "\nLargest amount of food eaten: " + Statistics.largestNumberOfFoodsEaten
                    + "\nNumber of generations: " + Statistics.longestGenerationLived
                    + "\nHighest population at one time: " + Statistics.largestPopulation
                    + "\nElapsed time: " + (int)(Statistics.runTimeInSeconds / 60) + " min " + (int)Statistics.runTimeInSeconds % 60 + " s";

                string aboutString = "Coded by Jonathan Tang '13 in Visual C# with Microsoft XNA 4.0,"
                    + "\nfor Mr. Gieske's Honors Math Seminar: Infinity class.";

                spriteBatch.DrawString(font, statString,
                    (new Vector2(50, 50)), Color.Black);

                spriteBatch.DrawString(font, aboutString,
                    (new Vector2(50, SCREEN_HEIGHT - 150)), Color.Black);

                spriteBatch.DrawString(font, ((int)secondsCounter).ToString() + " seconds left until new session.", 
                    (new Vector2(50, SCREEN_HEIGHT - 50)), Color.Black);

                secondsCounter -= gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;
            }

            spriteBatch.End();

            if (secondsCounter <= 0)
            {
                //  reset the screen
                Initialize();
                LoadContent();
            }

            base.Draw(gameTime);
        }
    }
}
