using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace InfinityEvolve
{
    public class Ant : Entity
    {
        Dictionary<string, double> attributes;

        double[,] diffusionGrid;

        List<Ant> children = new List<Ant>();

        List<Food> foods = new List<Food>();
        List<Ant> ants = new List<Ant>();
        int[,] lastSeen; //  the time a particular grid piece was last seen

        double RANGE_OFFSET = 0.5; //  if this = .5, then each statistic can be mutated positively or negatively
        double EXTREME_MUTATION_CHANCE = Statistics.EXTREME_MUTATION_CHANCE;

        double MOVEMENT_COST = 2;
        double STANDING_COST = 1;
        double MINIMUM_CHILDBIRTH_TIME = 5; // 5 seconds
        double DEFAULT_STARTING_HEALTH = 150;

        double health;

        bool isDead = false;
        int generation = 0;
        int moves = 0;
        int foodsEaten = 0;
        char lastMove = '0';

        int numChildren = 0;

        double timeOfLastUpdate = 0;

        double timeOfLastChild = 0; //  last childbirth time

        public Ant(Dictionary<string, double> template, Random rng, Entity[,] grid, bool guaranteedMutation)
        {
            Statistics.totalNumberOfAnts++;
            if (Statistics.totalNumberOfAnts > Statistics.largestPopulation)
            {
                Statistics.largestPopulation = Statistics.totalNumberOfAnts;
            }

            attributes = new Dictionary<string, double>();

            foreach (KeyValuePair<string, double> kvp in template)
            {
                //  chance of totally random mutation for a given characteristic
                if (rng.NextDouble() <= EXTREME_MUTATION_CHANCE || guaranteedMutation)
                {
                    // old code
                    //attributes[kvp.Key] = (2 * template[kvp.Key]) * (rng.NextDouble() - RANGE_OFFSET) * (template["mutationFactor"]);
                    double range = Statistics.attributeRange[kvp.Key].Y - Statistics.attributeRange[kvp.Key].X;
                    attributes[kvp.Key] = range * rng.NextDouble() + Statistics.attributeRange[kvp.Key].X;
                }
                else
                {
                    attributes[kvp.Key] = template[kvp.Key] + (rng.NextDouble() - RANGE_OFFSET) * template["mutationFactor"] * (Statistics.attributeRange[kvp.Key].Y - Statistics.attributeRange[kvp.Key].X);

                    if (attributes[kvp.Key] > Statistics.attributeRange[kvp.Key].Y)
                    {
                        attributes[kvp.Key] = Statistics.attributeRange[kvp.Key].Y;
                    }
                    else if (attributes[kvp.Key] < Statistics.attributeRange[kvp.Key].X)
                    {
                        attributes[kvp.Key] = Statistics.attributeRange[kvp.Key].X;
                    }
                }

                Statistics.currentSum[kvp.Key] += attributes[kvp.Key];
            }

            isObstacle = true;
            health = DEFAULT_STARTING_HEALTH;

            lastSeen = new int[grid.GetLength(0), grid.GetLength(1)];
            diffusionGrid = new double[grid.GetLength(0), grid.GetLength(1)];

        }

        //  give birth to an ant
        public Ant(Ant dad, Ant mom, Random rng, Entity[,] grid)
        {
            bool foundSpawn = false;
            int xMin = Math.Min((int)dad.position.X, (int)mom.position.X);
            int xMax = Math.Max((int)dad.position.X, (int)mom.position.X);

            int yMin = Math.Min((int)dad.position.Y, (int)mom.position.Y);
            int yMax = Math.Max((int)dad.position.Y, (int)mom.position.Y);
            for (int i = (int)xMin - 1; i < grid.GetLength(0) && i <= xMax + 1 && i >= 0; i++)
            {
                for (int j = (int)yMin - 1; j < grid.GetLength(1) && j <= yMax+1 && j >= 0; j++)
                {
                    if (grid[i, j] == null)
                    {
                        position.X = i;
                        position.Y = j;

                        foundSpawn = true;
                        break;
                    }
                }
                if (foundSpawn) break;
            }

            //  only add it to the grid if you found a proper spot
            if (foundSpawn)
            {
                attributes = new Dictionary<string, double>();

                foreach (KeyValuePair<string, double> kvp in mom.attributes)
                {
                    //  chance of totally random mutation
                    if (rng.NextDouble() <= EXTREME_MUTATION_CHANCE)
                    {
                        double range = Statistics.attributeRange[kvp.Key].Y - Statistics.attributeRange[kvp.Key].X;
                        attributes[kvp.Key] = range * rng.NextDouble() + Statistics.attributeRange[kvp.Key].X;

                        //old code
                        //attributes[kvp.Key] = (dad.attributes[kvp.Key] + mom.attributes[kvp.Key]) * (rng.NextDouble() - RANGE_OFFSET) * (dad.attributes["mutationFactor"] + mom.attributes["mutationFactor"]) / 2.0;
                    }
                    else
                    {
                        attributes[kvp.Key] = (dad.attributes[kvp.Key] + mom.attributes[kvp.Key]) / 2.0
                            + (rng.NextDouble() - RANGE_OFFSET) * ((dad.attributes["mutationFactor"] + mom.attributes["mutationFactor"]) / 2.0) * (Statistics.attributeRange[kvp.Key].Y - Statistics.attributeRange[kvp.Key].X);
                        
                        if (attributes[kvp.Key] > Statistics.attributeRange[kvp.Key].Y)
                        {
                            attributes[kvp.Key] = Statistics.attributeRange[kvp.Key].Y;
                        }
                        else if (attributes[kvp.Key] < Statistics.attributeRange[kvp.Key].X)
                        {
                            attributes[kvp.Key] = Statistics.attributeRange[kvp.Key].X;
                        }
                    }

                    Statistics.currentSum[kvp.Key] += attributes[kvp.Key];
                }

                Statistics.birthEffects.Add(new BirthEffect(mom, dad, this));
                if (Statistics.totalNumberOfAnts > Statistics.largestPopulation)
                {
                    Statistics.largestPopulation = Statistics.totalNumberOfAnts;
                }

                Statistics.totalNumberOfAnts++;
                dad.children.Add(this);
                mom.children.Add(this);

                dad.numChildren++;
                mom.numChildren++;

                if (dad.numChildren > Statistics.largestNumberOfChildren)
                {
                    Statistics.largestNumberOfChildren = dad.numChildren;
                }
                if (mom.numChildren > Statistics.largestNumberOfChildren)
                {
                    Statistics.largestNumberOfChildren = mom.numChildren;
                }

                int generationMax = dad.generation;
                if (mom.generation > generationMax) generationMax = mom.generation;

                this.generation = generationMax + 1;

                if (this.generation > Statistics.longestGenerationLived)
                {
                    Statistics.longestGenerationLived = this.generation;
                }

                grid[(int)position.X, (int)position.Y] = this;

                isObstacle = true;
                diffusionGrid = new double[grid.GetLength(0), grid.GetLength(1)];
                lastSeen = new int[grid.GetLength(0), grid.GetLength(1)];
            }

        }

         double Score(double value, Vector2 start, Vector2 goal) {

            double decay = attributes["decayFactor"];

            double dist = Math.Abs(start.X-goal.X) + Math.Abs(start.Y-goal.Y);
            double score = value - (.5*value/decay)*(dist);

            if (value>0){ 
              if (score <0) {
                score =0;
              }
            }
            else {
                if (score > 0)
                    score = 0;
            }
                
            return score;
         }

         public char BackwardsDirection(char dir)
         {
             switch (dir)
             {
                 case '0':
                     return '0';
                 case 'N':
                     return 'S';
                 case 'W':
                     return 'E';
                 case 'E':
                     return 'W';
             }
             return '0';
         }

         public double[,] CalculateDiffusion(Entity[,] grid, double curTime)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Vector2 currentPos = new Vector2(i, j);

                    if (curTime - lastSeen[i, j]
                        >= attributes["timeToReexplore"])
                    {
                        diffusionGrid[i, j] = attributes["desireToExplore"];
                    }
                    else
                    {
                        diffusionGrid[i, j] = 0;
                    }

                    foreach (Food food in foods)
                    {
                        diffusionGrid[i, j]
                            += Score(attributes["desireForFood"]*food.Calories(), food.Position(), currentPos);
                    }
                    foreach (Ant ant in ants)
                    {
                        diffusionGrid[i, j]
                            += Score(attributes["desireToFollowAnts"], ant.Position(), currentPos);

                        if (ant.health >= attributes["healthThreshold"])
                        {
                            if (curTime - timeOfLastChild >= attributes["childbirthCooldown"]
                            && curTime - timeOfLastChild >= MINIMUM_CHILDBIRTH_TIME)
                            {
                                diffusionGrid[i, j]
                                    += Score((attributes["desireToMate"] +
                                    (ant.health - attributes["healthThreshold"])) / (children.Count + 1)
                                    , ant.Position(), currentPos);
                            }
                        }
                    }

                    Vector2 sameDirMove = directionTranslate(lastMove);
                    if (i == sameDirMove.X && j == sameDirMove.Y && lastMove != '0')
                    {
                        //  prevents equal-sum scenarios
                        diffusionGrid[i, j] += attributes["desireToContinueInSameDirection"];
                    }

                    Vector2 backwardsDirMove = directionTranslate(BackwardsDirection(lastMove));
                    if (i == backwardsDirMove.X && j == backwardsDirMove.Y && lastMove != '0')
                    {
                        //  prevents oscillation
                        diffusionGrid[i, j] += attributes["desireToGoBackwards"];
                    }

                }
            }

            return diffusionGrid;
        }

        void ProcessVision(Entity[,] grid, double curTime)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    int dist = Math.Abs((int) position.X - i) + Math.Abs((int) position.Y - j);

                    if (dist > attributes["eyesightRange"])
                    {
                        continue;  // don't account for things you cannot see
                    }

                    Entity e = grid[i, j];
                    if (e != null)
                    {
                        if (e is Ant)
                        {
                            ants.Add((Ant)e);
                        }
                        else if (e is Food)
                        {
                            foods.Add((Food)e);
                        }
                    }

                    lastSeen[i, j] = (int)curTime;
                }
            }
        }

        public bool IsDead() { return isDead; }

        public bool isParentOf(Ant a)
        {
            foreach (Ant ant in children)
            {
                if (a == ant) return true;
            }
            return false;
        }

        public Vector2 directionTranslate(char dir)
        {
            switch (dir)
            {
                case '0':
                    return position;
                case 'N':
                    return new Vector2(position.X, position.Y - 1);
                case 'S':
                    return new Vector2(position.X, position.Y + 1); 
                case 'W':
                    return new Vector2(position.X - 1, position.Y);
                case 'E':
                    return new Vector2(position.X + 1, position.Y); 
            }

            return position;
        }

        public void Update(Entity[,] grid, GameTime gameTime, Random rng)
        {
            //  update health every second
            if (gameTime.TotalGameTime.TotalSeconds - timeOfLastUpdate > 1)
            {
                timeOfLastUpdate = gameTime.TotalGameTime.TotalSeconds;
                health -= STANDING_COST;
            }

            int curTime = (int) gameTime.TotalGameTime.TotalSeconds;
            Vector2 newPosition = position;
            foreach (char c in Entity.Directions)
            {
                newPosition = directionTranslate(c);

                //  give birth to ant sometimes
                if (newPosition.Y >= 0 && newPosition.Y < grid.GetLength(1) &&
                        newPosition.X >= 0 && newPosition.X < grid.GetLength(0) &&
                        grid[(int)newPosition.X, (int)newPosition.Y] is Ant)
                {
                    Ant other = (Ant) grid[(int)newPosition.X, (int)newPosition.Y];

                    if (curTime - timeOfLastChild >= attributes["childbirthCooldown"]
                        && curTime - other.timeOfLastChild >= other.attributes["childbirthCooldown"]
                        && curTime - other.timeOfLastChild >= MINIMUM_CHILDBIRTH_TIME
                        && curTime - timeOfLastChild >= MINIMUM_CHILDBIRTH_TIME)
                    {
                        if (rng.NextDouble() <= attributes["childbirthPercentage"] / 100)
                        {
                            Ant child = new Ant(this, other, rng, grid);
                            child.sprite = other.sprite;

                            timeOfLastChild = curTime;
                            other.timeOfLastChild = curTime;

                            //  health of parents go into raising the child
                            child.health = other.attributes["childbirthCost"] + attributes["childbirthCost"];

                            other.health -= 1.5*other.attributes["childbirthCost"];
                            health -= 1.5*attributes["childbirthCost"];
                        }
                    }
                }
            }

            // punish ants for having high eyesight, strength, etc
            health -= attributes["eyesightRange"] * .0001;

            //  death
            if (health <= 0 || moves >= attributes["oldestPossibleAge"])
            {
                //  kill this ant if it has low health
                Die(grid);
            }
        }

        public void Die(Entity[,] grid)
        {
            Statistics.totalNumberOfAnts--;

            foreach (KeyValuePair<string, double> kvp in attributes)
            {
                //  remove these attributes from the current pool
                Statistics.currentSum[kvp.Key] -= attributes[kvp.Key];
            }

            ((Ant)grid[(int)position.X, (int)position.Y]).isDead = true;
            grid[(int)position.X, (int)position.Y] = null;
        }

        // NOTE: curTime is in milliseconds
        public override char CreateMovement(Entity[,] grid, double curTime)
        {
            if (curTime - timeOfLastMove < attributes["walkSpeed"])
                return '0'; //  don't move if the time isn't right

            timeOfLastMove = curTime; // update movetime

            ProcessVision(grid, curTime / 1000.0);
            CalculateDiffusion(grid, curTime / 1000.0);

            char dir = '0';
            double max = 0;
            Vector2 newPosition = position;

            foreach (char c in Entity.Directions)
            {
                newPosition = directionTranslate(c);

                //  absorbs a food piece just by being next to it
                if (newPosition.Y >= 0 && newPosition.Y < grid.GetLength(1) &&
                        newPosition.X >= 0 && newPosition.X < grid.GetLength(0) &&
                        grid[(int)newPosition.X, (int)newPosition.Y] is Food)
                {
                    health += ((Food)grid[(int)newPosition.X, (int)newPosition.Y]).Calories();

                    Statistics.foodFades.Add(new FoodFade(newPosition, grid[(int)newPosition.X, (int)newPosition.Y].Sprite()));

                    grid[(int)newPosition.X, (int)newPosition.Y] = null;

                    Statistics.foodEffects.Add(new FoodEffect(this, newPosition));

                    foodsEaten++;
                    Statistics.totalFoods--;

                    if (foodsEaten > Statistics.largestNumberOfFoodsEaten)
                    {
                        Statistics.largestNumberOfFoodsEaten = foodsEaten;
                    }
                }

                //  don't walk onto a position that has an ant
                if (newPosition.Y >= 0 && newPosition.Y < grid.GetLength(1) &&
                    newPosition.X >= 0 && newPosition.X < grid.GetLength(0) &&
                    grid[(int)newPosition.X, (int)newPosition.Y] == null)
                {
                    //  only calculate diffusion for valid locations
                    if (diffusionGrid[(int)newPosition.X, (int)newPosition.Y] > max)
                    {
                        max = diffusionGrid[(int)newPosition.X, (int)newPosition.Y];
                        dir = c;
                    }
                }
            }

            if (dir != '0')
            {
                health -= MOVEMENT_COST;
                moves++;
                Statistics.numberOfMovements++;
                if (moves > Statistics.longestAgeLived)
                {
                    Statistics.longestAgeLived = moves;
                }

                Statistics.timeOfLastMovement = (int)(curTime/1000.0);
                Statistics.systemHasMoved = true;
            }

            lastMove = dir;

            return dir;
        }

        public override Color RGBColor()
        {
            int alpha = (int)health + 150;
            if (alpha > 255) alpha = 255;

            return new Color((int)attributes["redColor"], (int)attributes["greenColor"],
                (int)attributes["blueColor"], alpha);
        }

        public Dictionary<string, double> Attributes()
        {
            return attributes;
        }

        public void ChangeAttribute(string key, double value)
        {
            attributes[key] = value;
        }

    }
}
