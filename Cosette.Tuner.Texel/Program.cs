﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cosette.Tuner.Common.Requests;
using Cosette.Tuner.Common.Services;
using Cosette.Tuner.Texel.Genetics;
using Cosette.Tuner.Texel.Genetics.ScalingFactor;
using Cosette.Tuner.Texel.Settings;
using Cosette.Tuner.Texel.Web;
using GeneticSharp;

namespace Cosette.Tuner.Texel;

class Program
{
    private static int _testId;
    private static WebService _webService;
    private static Stopwatch _generationStopwatch;

    static async Task Main(string[] args)
    {
        Console.WriteLine($"[{DateTime.Now}] Tuner start");
        SettingsLoader.Init("settings.json");

        IFitness fitness = null;
        IChromosome chromosome = null;

        _webService = new WebService();
        await _webService.EnableIfAvailable();
        _testId = await _webService.RegisterTest(new RegisterTestRequest
        {
            Type = TestType.Texel
        });

        var scalingFactorMode = args.Contains("scaling_factor");
        if (scalingFactorMode)
        {
            SettingsLoader.Data.Genes.Clear();
            SettingsLoader.Data.Genes.Add(new GeneInfo
            {
                Name = "ScalingFactor",
                MinValue = 0,
                MaxValue = 2000
            });
            SettingsLoader.Data.Genes.Add(new GeneInfo
            {
                Name = "Unused",
                MinValue = 0,
                MaxValue = 1
            });

            fitness = new ScalingFactorFitness(_testId, _webService);
            chromosome = new ScalingFactorChromosome();
        }
        else
        {
            fitness = new EvaluationFitness(_testId, _webService);
            chromosome = new EvaluationChromosome();
        }

        var selection = new EliteSelection();
        var crossover = new UniformCrossover(0.5f);
        var mutation = new UniformMutation(true);
        var population = new Population(SettingsLoader.Data.MinPopulation, SettingsLoader.Data.MaxPopulation, chromosome);
        var geneticAlgorithm = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new GenerationNumberTermination(SettingsLoader.Data.GenerationsCount)
        };
        geneticAlgorithm.GenerationRan += GeneticAlgorithm_GenerationRan;

        _generationStopwatch = new Stopwatch();
        _generationStopwatch.Start();
        geneticAlgorithm.Start();

        Console.WriteLine("Best solution found has {0} fitness.", geneticAlgorithm.BestChromosome.Fitness);
        Console.ReadLine();
    }

    private static void GeneticAlgorithm_GenerationRan(object sender, EventArgs e)
    {
        var geneticAlgorithm = (GeneticAlgorithm)sender;
        var genesList = new List<string>();

        for (var geneIndex = 0; geneIndex < SettingsLoader.Data.Genes.Count; geneIndex++)
        {
            var name = SettingsLoader.Data.Genes[geneIndex].Name;
            var value = geneticAlgorithm.BestChromosome.GetGene(geneIndex).ToString();

            genesList.Add($"{name}={value}");
        }
        
        var generationDataRequest = RequestsFactory.CreateGenerationRequest(_testId, _generationStopwatch.Elapsed.TotalSeconds, geneticAlgorithm.BestChromosome);
        _webService.SendGenerationData(generationDataRequest).GetAwaiter().GetResult();
        
        Console.WriteLine("======================================");
        Console.WriteLine($"[{DateTime.Now}] Generation done!");
        Console.WriteLine($" - best chromosome: {string.Join(", ", genesList)}");
        Console.WriteLine($" - best fitness: {geneticAlgorithm.BestChromosome.Fitness}");
        Console.WriteLine("======================================");

        _generationStopwatch.Restart();
    }
}
