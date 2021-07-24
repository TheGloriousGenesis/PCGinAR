using System.Collections.Generic;
using BaseGeneticClass;
using GeneticAlgorithms;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestImplementation : MonoBehaviour
{
    //[Header("Genetic Algorithm")]
    //[SerializeField] int hey;

    //[Header("Other")]
    //[SerializeField] Text target;
    //[SerializeField] Text best;
    //[SerializeField] Text bestFitness;
    //[SerializeField] Text numGenerations;
    //[SerializeField] Transform populationParent;
    //[SerializeField] Text textPrefab;

    [Header("Game Property")]
    [SerializeField] GenerateGame generateGame;
    [SerializeField] Path3D multiplePaths;
    
    private Random random;
    private BasicGeneticAlgorithm ga;

    void Run()
    {
        random = new Random(Constants.SEED);
        ga = new BasicGeneticAlgorithm(Constants.POPULATION_SIZE, Constants.CHROMOSONE_LENGTH, Constants.CROSSOVER_PROBABILITY,
            random, Constants.ELITISM, Constants.MUTATION_PROBABILITY, Constants.ITERATION, Constants.K);
        List<Chromosome> finalResult = ga.Run(FitnessFunction);
        setUpLevel(finalResult[1]);
    }

    void Remove()
    {
        generateGame.ResetGameArea();
    }

    private double FitnessFunction(Chromosome chromosome)
    {
      System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
      float score = 0;
      timer.Start();
      setUpLevel(chromosome);
      timer.Stop();
      NavMeshPath path = generateGame.PathStatus();
      int numberOfPaths = calculateNumberOfPaths();
      if (path.status == NavMeshPathStatus.PathComplete)
      {
          score += 1;
      }
      
      if (path.GetTotalDistance() > Constants.MAX_PLATFORM_DIMENSION)
      {
          score += 1;
      }
      else
      {
          score -= 0.5f;
      }

      if (0 <= numberOfPaths && numberOfPaths <= 5)
      {
          score += 2;
      }
      else if (6 <= numberOfPaths && numberOfPaths <= 10)
      {
          score += 1;
      }
      else if (10 <= numberOfPaths && numberOfPaths <= 20)
      {
          score += 0.5f;
      }
      score = (Mathf.Pow(2, score) - 1) / (2 - 1);
      chromosome.fitness = score;
      
      return timer.Elapsed.TotalMilliseconds;
    }

    private void setUpLevel(Chromosome chromosome)
    {
        generateGame.CreateGame(Constants.playerType, chromosome);
    }

    private int calculateNumberOfPaths()
    {
        List<List<Vector3>> allPaths = multiplePaths.FindPaths();
        return allPaths.Count;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(TestImplementation))]
    [CanEditMultipleObjects]
    public class TestImplementation_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Start Generation"))
            {
                foreach (var targ in targets)
                {
                    ((TestImplementation)targ).Run();
                }
            }

            if (GUILayout.Button("Clear Generation"))
            {
                foreach (var targ in targets)
                {
                    ((TestImplementation)targ).Remove();
                }
            }
        }
    }

#endif
}
