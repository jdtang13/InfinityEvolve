using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityEvolve
{
    class Food : Entity
    {
        double calories;
        public Food(Random rng)
        {
            calories = (rng.NextDouble()) * 200;
        }

        public double Calories() { return calories; }
    }
}
