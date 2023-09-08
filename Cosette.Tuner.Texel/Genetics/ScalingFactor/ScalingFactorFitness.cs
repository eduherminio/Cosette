﻿using System;
using System.Diagnostics;
using Cosette.Tuner.Common.Services;
using Cosette.Tuner.Texel.Engine;
using Cosette.Tuner.Texel.Settings;
using Cosette.Tuner.Texel.Web;
using GeneticSharp;

namespace Cosette.Tuner.Texel.Genetics
{
    public class ScalingFactorFitness : IFitness
    {
        private int _testId;
        private WebService _webService;

        private EngineOperator _engineOperator;

        public ScalingFactorFitness(int testId, WebService webService)
        {
            _testId = testId;
            _webService = webService;

            _engineOperator = new EngineOperator(SettingsLoader.Data.EnginePath, SettingsLoader.Data.EngineArguments);
            _engineOperator.Init();
            _engineOperator.LoadEpd(SettingsLoader.Data.PositionsDatabasePath);
        }

        public double Evaluate(IChromosome chromosome)
        {
            var stopwatch = Stopwatch.StartNew();

            var scalingFactor = (double)(int) chromosome.GetGene(0).Value / 1000;
            var error = _engineOperator.Evaluate(scalingFactor);

            var fitness = 1.0 - error;
            var elapsedTime = (double)stopwatch.ElapsedMilliseconds / 1000;

            var chromosomeRequest = RequestsFactory.CreateChromosomeRequest(_testId, fitness, elapsedTime, chromosome);
            _webService.SendChromosomeData(chromosomeRequest).GetAwaiter().GetResult();

            Console.WriteLine($"[{DateTime.Now}] Run done! Fitness: {fitness}");
            return fitness;
        }
    }
}
