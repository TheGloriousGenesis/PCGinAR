using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BaseGeneticClass;
using eDmitriyAssets.NavmeshLinksGenerator;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

using BasicGeneticAlgorithmNS;

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

    private System.Random random;
    private BasicGeneticAlgorithm ga;

    void Run()
    {
        random = new System.Random(Constants.SEED);
        ga = new BasicGeneticAlgorithm(Constants.POPULATION_SIZE, Constants.CHROMOSONE_LENGTH, Constants.CROSSOVER_PROBABILITY,
            random, FitnessFunction, Constants.ELITISM, Constants.MUTATION_PROBABILITY, Constants.ITERATION);
        List<Chromosone> finalResult = ga.Run(FitnessFunction);
        setUpLevel(finalResult[0]);
    }

    void Remove()
    {
        generateGame.ResetGameArea();
    }

    private float FitnessFunction(Chromosone chromosone)
    {
        float score = 0;
        setUpLevel(chromosone);
        NavMeshPathStatus status = generateGame.PathStatus();

        if (status == NavMeshPathStatus.PathComplete)
        {
            score = 1;
        } else if (status == NavMeshPathStatus.PathPartial)
        {
            score = 0.5f;
        }

        score = (Mathf.Pow(2, score) - 1) / (2 - 1);
        chromosone.fitness = score;
        Debug.Log($"CHROMOSONE {chromosone.id_}: SCORE: {score}, STATUS: {status}");
        return score;
    }

    private void setUpLevel(Chromosone chromosone)
    {
        generateGame.CreateGame(Constants.playerType, chromosone);
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
