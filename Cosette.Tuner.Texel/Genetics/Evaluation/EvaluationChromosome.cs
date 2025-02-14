﻿using Cosette.Tuner.Texel.Settings;
using GeneticSharp;

namespace Cosette.Tuner.Texel.Genetics;

public class EvaluationChromosome : ChromosomeBase
{
    public EvaluationChromosome() : base(SettingsLoader.Data.Genes.Count)
    {
        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        var gene = SettingsLoader.Data.Genes[geneIndex];
        var value = RandomizationProvider.Current.GetInt(gene.MinValue, gene.MaxValue + 1);

        return new Gene(value);
    }

    public override IChromosome CreateNew()
    {
        return new EvaluationChromosome();
    }
}
