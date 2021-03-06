Jonathan Tang

Honors Math Seminar � Infinity

Mr. Gieske

Infinity and the Evolution of Artificial Computer Systems


### Note: Please view the full LaTeX writeup of this file here: https://dl.dropboxusercontent.com/u/5703256/InfinityFinal.pdf


The concept of infinity and the evolution of learning systems are closely related to one another, and it is easy to see how a process of natural selection and programmatic �mutations� can cause small variations to accumulate into an ideal solution given infinite time. Moreover, it is oftentimes impractical or even impossible to formulate truly intelligent algorithms in words alone, so evolution remains a very viable mechanism for creating intelligent systems. Using the Microsoft Visual C# programming language and the XNA 4.0 development toolkit, I created a system of evolving artificial ant-like animals with numerical attributes that serve as a simple representation of real ant characteristics � attributes which evolve according to a process of natural selection in order to adjust to a randomized environment. 

The environment that I designed is a two-dimensional grid containing ants, represented by colorful insect-like icons, and pieces of food, represented by yellow diamond shapes. The dimensions of the grid, the rate of food generation, the size of the starting population of ants, and the probability of �mutation,� are parameters that are randomized from trial to trial, allowing unique results to emerge in response to different environments. Ants may move around freely on the map so long as they do not occupy the positions of other ants, but as they move, their associated numerical indicators of healthiness continually decrease. Consuming food by moving to locations adjacent to grid squares containing food pieces can increase an ant�s health, which is necessary to stave off an ant�s inevitable death, caused either by having a health value less than or equal to zero or by old age. Ants may also freely mate with other ants, discriminating against potential mates by only mating with �healthy� ants that satisfy a predetermined numerical standard that varies from ant to ant. Reproduction reduces the health of each parent ant by an arbitrary donation quantity particular to each ant, and the child ants produced this way receive a starting storage of health directly proportional to the amount that its parents contributed; selfish donations will lead to weak child ants that die easily, and overly generous donations can kill the contributing parents. Over time, the ants produce offspring that may be quite different from the starting population of ants both in terms of appearance and in survival capability. 

The representation of evolution I chose for this project was a heuristic very similar to genetic algorithms, which essentially encode attributes in numerical �chromosomes� that can be inherited and subsequently mutated. I implemented the following two operations for these virtual chromosomes: crossover, which is when two parent ants each contribute pieces of genetic information to a child ant�s attributes, and mutation, which is when a chromosome is altered and randomized in order to simply simulate genetic mutation. The great appeal of genetic algorithms is that they, if well-designed, can theoretically produce near-perfect solutions to any numerical fitness criterion in time significantly faster than simple brute-force methodologies. One of the significant downsides of genetic algorithms, however, is that they are most suited for addressing purely numerical problems. It is therefore necessary to define the problem of artificial ant intelligence in numerical terms. The fitness function for this problem is simple and can emerge naturally: �fit� ants are those that survive longer and have more offspring. Throughout an ant�s lifespan, however, it will need to make several important decisions about whether to stay near a swarm of ants, whether to explore the map if no food can be found, and whether to grab a piece of food or take the time to mate with another ant. These decisions can be modeled using a diffusion algorithm, which gives numerical weights to certain map objectives, such as food and other ants, and diffuses those weights outward with weight decreasing as distance increases. Each ant performs its movements based on the grid location with highest weight adjacent to its location. This diffusion model is not only completely numerical but also allows for desirable emergent behavior: an ant can focus on a piece of food and be drawn towards it, but it might also divert from its original path if that food is taken by another ant or if another, more enticing objective catches its attention.

Each ant possesses a set of attributes, with each attribute belonging to only one of two classes: diffusion attributes and mechanical attributes. 
The diffusion attributes are weight values particular to each ant that can be interpreted as an ant�s subjective �desire� to pursue specific goals: for example, an ant with a food diffusion value of 2000 units and a mating diffusion value of 100 units will tend to prioritize finding food over mating with other ants. 
In contrast, mechanical attributes are qualities more closely related to the physical ways in which an ant interacts with its environment. Movement speed determines the number of milliseconds that must pass before an ant takes a step. Another attribute determines the maximum number of steps an ant can walk before it dies of old age. Eyesight range is the value used to determine which tiles within a particular radius from an ant�s position are factored into its diffusion calculations. Notably, these qualities, despite appearing to be only positive, carry with them certain downsides that prevent the system from degenerating into a process of continually increasing the numerical values of positive traits. Because ants lose health with every step that they take, a higher movement speed will allow an ant quicker access to food at the cost of more rapidly decreasing health. In addition, because old ants may contain weaker genes than new ants, it is not necessarily desirable for an ant to consume the resources of the environment by living to an older age. Finally, higher eyesight may not be beneficial to certain ants, as ants with full vision of the grid tend to oscillate between two positions, becoming indecisive as to which objectives they wish to pursue. It is these environmental complexities that make lively the particular setting of this artificial intelligence experiment.

## Childbirth

An abridged algorithm for creating a child ant from two parent ants is contained in the pdf version of this file.

Essentially, a child ant generally possesses combined characteristics from both of its parents with some minor variations caused by random chance. On occasion, a given attribute may be totally replaced by a random value between the minimum and maximum limits for that attribute. The probability of this occurrence is based on a variable that changes from experiment to experiment, allowing for some trials to contain little variation and others to possess populations with extreme genetic diversity.

## Graphical interface

For ease of viewing, the graphical interface of the Ants program uses animated events to highlight significant changes in the environment. Whenever an ant consumes a piece of food by moving next to it, a thin green line is drawn between the ant and the food. When two ants successfully mate and produce offspring, two thick lines leading from the parents to the child will be drawn on the screen, with the color of the line corresponding to the color of the child. In addition, a yellow outline is placed around the tile that a child ant occupies for the first three seconds of its existence. On occasion, a child ant may appear to pop into existence with as few as one or even zero parent ants; this phenomenon occurs when a parent ant eliminates itself from the grid by donating too much of its health to its child. Similarly, a yellow outline placed around an empty tile indicates the position at which a child ant disappeared within the first three seconds of its life.
In addition, a graph on the right side of the display shows all the attributes of the ant population, with lines drawn corresponding to the values for particular numerical traits. White indicators on the lines show the average numerical value of the first generation of ants in relation to the minimum and maximum values possible. Another indicator, colored according to the average RGB value of all currently living ants, displays the average numerical values for the current population of ants. 

## Conclusions and comments on final results

There is something sublime and beautiful about the progressions of evolving systems. They seem natural and perfect, for they bring with them results determined not by arbitrary human judgment but instead shaped by environmental forces. There are few things more entertaining than watching the tranquil motions of ants in a nest, birds flying in groups, and rippling water. Watching the differences and similarities of the genetic compositions of first generation ants and currently surviving ants can lead to some truly remarkable sights, such as ants that share a particular color, indicating the success of one particular ancestor, or attributes that seem to approach particular values without ever going under or over them, indicating the existence of a perfect solution to a particular environment. And like the concept of infinity itself, there never seems to be a defined endpoint for the ever-busy hands of evolution. 

## Appendix: Detailed breakdown of attributes

redColor: the red component of an ant�s RGB color. Color has no effect on ant-to-ant interactions, but it serves as a useful genetic visualization for human viewers.

blueColor: the blue component of an ant�s RGB color.

greenColor: the green component of an ant�s RGB color.

mutationFactor: the percentage variation that occurs when a child ant�s attributes are being determined by those of its parents.

walkSpeed: the number of milliseconds it takes for an ant to move a single step.

desireForFood: the diffusion value for collecting food. Negative values will cause the ant to avoid food altogether.

desireToFollowAnts: the diffusion value for ant swarm behavior. Higher values of this attribute will cause ants to clump together while negative values will lead to lone wolf behavior.

desireToExplore: the diffusion value applied to territory that an ant has either forgotten or not yet seen.

desireToMate: the diffusion value applied to visible ants that meet an ant�s standard for mating.

desireToContinueInSameDirection: a value that, if positive, causes ants to favor walking in the same direction.

desireToGoBackwards: a value that, if positive, causes ants to favor walking the directions opposite their last movements.

timeToReexplore: a number, in seconds, the determines how long an ant remembers seen territory as having already been explored.

decayFactor: a parameter that factors into diffusion calculations and is analogous to an ant�s sense of smell. Decay factors closer to 1 will lead to stronger diffusions across the grid, while large, positive decay factors will cause diffusion weights to be more difficult to detect over longer distances.

healthThreshold: a numerical threshold that other ants must satisfy before being able to successfully mate with an ant.

childbirthPercentage: the probability of an ant mating with another valid, healthy ant when the two ants occupy adjacent locations.

childbirthCost: the quantity of health that an ant donates to its children.

childbirthCooldown: the amount of time, in seconds, that must pass before an ant is able to have a child again.

oldestPossibleAge: the number of moves an ant can make before dying.

eyesightRange: the number of tiles an ant can see.
