using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace InfinityEvolve
{
    public class Statistics
    {

        public enum GameState
        {
            ANTS_SCREEN, STATISTICS_SCREEN
        };

        
        public static int numberOfMovements = 0;
        public static int longestAgeLived = 0;
        public static int largestNumberOfChildren = 0;
        public static int largestNumberOfFoodsEaten = 0;
        public static List<BirthEffect> birthEffects; // use this to track the animations for ants being born
        public static List<FoodEffect> foodEffects; // use this to track the animations for ants eating food
        public static List<FoodFade> foodFades;

        public static double EXTREME_MUTATION_CHANCE = .05;

        public static Dictionary<string, Vector2> attributeRange = new Dictionary<string, Vector2>();

        //  average values for the first generation of ants
        public static Dictionary<string, double> firstAverages = new Dictionary<string, double>();

        //  sum of the values for CURRENT population of ants... add to it during birth, subtract during death
        //  currentSum/numAnts = currentAverage
        //  public static double CURRENT_AVERAGE_REBUILD_TIME = 5;
        public static Dictionary<string, double> currentSum = new Dictionary<string, double>();

        public static List<Ant> topFiveAncestors = new List<Ant>();
        public static int largestPopulation = 0;

        public static double runTimeInSeconds = 0;

        //  important current statistics
        public static int longestGenerationLived = 0; //i.e. current generation
        public static int totalNumberOfAnts = 0;
        public static int totalFoods = 0;

        public static int timeOfLastMovement = 0;
        public static bool systemHasMoved = false;
    }
}
