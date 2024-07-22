
namespace MachineLearning
{


    public class Individual
    {
        public int[] Genes { get; set; } // Array representing the individual's solution
        public double Fitness { get; set; } // Fitness score of the individual

        public Individual(int genesLength)
        {
            Genes = new int[genesLength];
            // Initialize genes with random values (replace with your initialization logic)
            for (int i = 0; i < genesLength; i++)
            {
                Genes[i] = RandomHelper.Next(0, 2); // Example: genes as 0 or 1
            }
        }
    }


    // https://en.wikipedia.org/wiki/Evolution_strategy
    public class EvolutionaryAlgorithm
    {
        public int PopulationSize { get; set; }
        public double MutationRate { get; set; }
        public double CrossoverRate { get; set; }

        public EvolutionaryAlgorithm(int populationSize, double mutationRate, double crossoverRate)
        {
            PopulationSize = populationSize;
            MutationRate = mutationRate;
            CrossoverRate = crossoverRate;
        }



        public System.Collections.Generic.List<Individual> Evolve(System.Func<Individual, double> fitnessFunction, int generations)
        {
            System.Collections.Generic.List<Individual> population = new System.Collections.Generic.List<Individual>();

            // Initialize population
            for (int i = 0; i < PopulationSize; i++)
            {
                int geneLengthOfSolution = RandomHelper.Next(15, 23);
                population.Add(new Individual(geneLengthOfSolution));
            }

            // Evolve for a number of generations
            for (int gen = 0; gen < generations; gen++)
            {
                // Evaluate fitness of each individual
                foreach (Individual individual in population)
                {
                    individual.Fitness = fitnessFunction(individual);
                }

                // Selection (replace with your selection method) - here roulette wheel selection
                System.Collections.Generic.List<Individual> selected = RouletteWheelSelection(population);

                // Crossover (replace with your crossover method) - here single-point crossover
                System.Collections.Generic.List<Individual> offspring = Crossover(selected);

                // Mutation
                Mutate(offspring);

                // Combine offspring with a portion of the previous generation (elitism)
                // population = population.Take(PopulationSize / 2).ToList();

                System.Collections.Generic.List<Individual> lss = new System.Collections.Generic.List<Individual>();
                for (int i = 0; i < PopulationSize / 2; ++i)
                {
                    lss.Add(population[i]);
                }
                population = lss;


                population.AddRange(offspring);
            }

            return population;
        }


        private void Mutate(System.Collections.Generic.List<Individual> offspring)
        {
            foreach (Individual individual in offspring)
            {
                // Loop through each gene in the individual
                for (int i = 0; i < individual.Genes.Length; i++)
                {
                    // Apply mutation with a certain probability (mutationRate)
                    
                    if (RandomHelper.NextDouble() < MutationRate)
                    {
                        // Flip the gene value (assuming genes are 0 or 1)
                        individual.Genes[i] = (individual.Genes[i] == 0) ? 1 : 0;

                        // Alternatively, you can use a different mutation strategy based on your problem
                        // For example, for numeric genes, you could add/subtract a small random value
                    }
                }
            }
        }



        private System.Collections.Generic.List<Individual> RouletteWheelSelection(System.Collections.Generic.List<Individual> population)
        {
            System.Collections.Generic.List<Individual> selected = new System.Collections.Generic.List<Individual>();
            double totalFitness = 0; //  population.Sum(i => i.Fitness);

            for (int i = 0; i < population.Count; ++i)
            {
                totalFitness += population[i].Fitness;
            }


            // Spin the roulette wheel for each slot in the new population
            for (int i = 0; i < PopulationSize; i++)
            {
                double slice = RandomHelper.NextDouble() * totalFitness;
                double currentFitness = 0;
                foreach (Individual individual in population)
                {
                    currentFitness += individual.Fitness;
                    if (currentFitness >= slice)
                    {
                        selected.Add(individual);
                        break;
                    }
                }
            }

            return selected;
        }

        private System.Collections.Generic.List<Individual> Crossover(System.Collections.Generic.List<Individual> population)
        {
            System.Collections.Generic.List<Individual> offspring = new System.Collections.Generic.List<Individual>();
            for (int i = 0; i < population.Count; i += 2)
            {
                if (RandomHelper.NextDouble() < CrossoverRate)
                {
                    int crossoverPoint = RandomHelper.Next(1, population[0].Genes.Length);
                    Individual offspring1 = new Individual(population[0].Genes.Length);
                    Individual offspring2 = new Individual(population[0].Genes.Length);

                    System.Array.Copy(population[i].Genes, 0, offspring1.Genes, 0, crossoverPoint);
                    System.Array.Copy(population[i + 1].Genes, crossoverPoint, offspring1.Genes, crossoverPoint, population[0].Genes.Length - crossoverPoint);

                    System.Array.Copy(population[i + 1].Genes, 0, offspring2.Genes, 0, crossoverPoint);
                    System.Array.Copy(population[i].Genes, crossoverPoint, offspring2.Genes, crossoverPoint, population[0].Genes.Length - crossoverPoint);

                    offspring.Add(offspring1);
                    offspring.Add(offspring2);
                }
                else
                {
                    offspring.Add(population[i]);
                    offspring.Add(population[i + 1]);
                }
            }

            return offspring;


        }
    }
}