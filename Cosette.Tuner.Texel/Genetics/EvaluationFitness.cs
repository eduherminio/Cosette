﻿using System;
using System.Diagnostics;
using Cosette.Tuner.Common.Services;
using Cosette.Tuner.Texel.Engine;
using Cosette.Tuner.Texel.Genetics.Epd;
using Cosette.Tuner.Texel.Settings;
using Cosette.Tuner.Texel.Web;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;

namespace Cosette.Tuner.Texel.Genetics
{
    public class EvaluationFitness : IFitness
    {
        private int _testId;
        private EpdLoader _epdLoader;
        private WebService _webService;

        private EngineOperator _engineOperator;

        public EvaluationFitness(int testId, EpdLoader epdLoader, WebService webService)
        {
            _testId = testId;
            _epdLoader = epdLoader;
            _webService = webService;

            _engineOperator = new EngineOperator(SettingsLoader.Data.EnginePath, SettingsLoader.Data.EngineArguments);
            _engineOperator.Init();
        }

        public double Evaluate(IChromosome chromosome)
        {
            for (var geneIndex = 0; geneIndex < SettingsLoader.Data.Genes.Count; geneIndex++)
            {
                var geneName = SettingsLoader.Data.Genes[geneIndex].Name;
                var geneValue = chromosome.GetGene(geneIndex).ToString();

                _engineOperator.SetOption(geneName, geneValue);
            }

            while (true)
            {
                try
                {
                    _engineOperator.ApplyOptions();
                    break;
                }
                catch
                {
                    _engineOperator.Restart();
                }
            }

            var sum = 0.0;
            var stopwatch = Stopwatch.StartNew();

            foreach (var position in _epdLoader.Positions)
            {
                var evaluation = _engineOperator.Evaluate(position.Fen);
                var sigmoidEvaluation = Sigmoid(evaluation);
                var desiredEvaluation = GetDesiredEvaluation(position.Result);

                sum += Math.Pow(desiredEvaluation - sigmoidEvaluation, 2);
            }

            var elapsedTime = (double)stopwatch.ElapsedMilliseconds / 1000;
            var error = sum / _epdLoader.Positions.Count;
            var fitness = 1.0 - error;

            var chromosomeRequest = RequestsFactory.CreateChromosomeRequest(_testId, fitness, elapsedTime, chromosome);
            _webService.SendChromosomeData(chromosomeRequest).GetAwaiter().GetResult();

            Console.WriteLine($"[{DateTime.Now}] Run done! Fitness: {fitness}");
            return fitness;
        }

        private double Sigmoid(int evaluation)
        {
            return 1.0 / (1 + Math.Pow(10, -SettingsLoader.Data.ScalingConstant * evaluation / 400));
        }

        private double GetDesiredEvaluation(GameResult gameResult)
        {
            switch (gameResult)
            {
                case GameResult.BlackWon: return 0;
                case GameResult.Draw: return 0.5;
                case GameResult.WhiteWon: return 1;
            }

            throw new InvalidOperationException();
        }
    }
}
