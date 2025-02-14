﻿using System.Collections.Generic;
using Cosette.Tuner.Common.Requests;
using Cosette.Tuner.SelfPlay.Genetics.Game;
using Cosette.Tuner.SelfPlay.Settings;
using GeneticSharp;

namespace Cosette.Tuner.SelfPlay.Web;

public static class RequestsFactory
{
    public static ChromosomeDataRequest CreateChromosomeRequest(int testId, double fitness, double elapsedTime, IChromosome chromosome, EvaluationParticipant referenceParticipant, EvaluationParticipant experimentalParticipant)
    {
        return new ChromosomeDataRequest
        {
            TestId = testId,
            ElapsedTime = elapsedTime,
            Fitness = fitness,

            ReferenceEngineStatistics = new SelfPlayStatisticsDataRequest
            {
                AverageTimePerGame = elapsedTime / SettingsLoader.Data.GamesPerFitnessTest,
                AverageDepth = referenceParticipant.AverageDepth,
                AverageNodesCount = referenceParticipant.AverageNodesCount,
                AverageNodesPerSecond = referenceParticipant.AverageNps,
                Wins = referenceParticipant.Wins,
                Draws = referenceParticipant.Draws
            },

            ExperimentalEngineStatistics = new SelfPlayStatisticsDataRequest
            {
                AverageTimePerGame = elapsedTime / SettingsLoader.Data.GamesPerFitnessTest,
                AverageDepth = experimentalParticipant.AverageDepth,
                AverageNodesCount = experimentalParticipant.AverageNodesCount,
                AverageNodesPerSecond = experimentalParticipant.AverageNps,
                Wins = experimentalParticipant.Wins,
                Draws = experimentalParticipant.Draws
            },

            Genes = CreateGenesRequest(chromosome)
        };
    }

    public static GenerationDataRequest CreateGenerationRequest(int testId, double elapsedTime, IChromosome chromosome)
    {
        return new GenerationDataRequest
        {
            TestId = testId,
            BestFitness = chromosome.Fitness.Value,
            ElapsedTime = elapsedTime,
            BestChromosomeGenes = CreateGenesRequest(chromosome)
        };
    }

    public static List<GeneDataRequest> CreateGenesRequest(IChromosome chromosome)
    {
        var genes = new List<GeneDataRequest>();
        for (var geneIndex = 0; geneIndex < SettingsLoader.Data.Genes.Count; geneIndex++)
        {
            genes.Add(new GeneDataRequest
            {
                Name = SettingsLoader.Data.Genes[geneIndex].Name,
                Value = (int)chromosome.GetGene(geneIndex).Value
            });
        }

        return genes;
    }
}
