﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Cosette.Tuner.Web.Services;
using Cosette.Tuner.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Cosette.Tuner.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMapper _mapper;
    private readonly TestService _testService;
    private readonly ChromosomeService _chromosomeService;
    private readonly GenerationService _generationService;
    private readonly ChartJsService _chartJsService;

    public HomeController(IMapper mapper, TestService testService, ChromosomeService chromosomeService, GenerationService generationService, ChartJsService chartJsService)
    {
        _mapper = mapper;
        _testService = testService;
        _chromosomeService = chromosomeService;
        _generationService = generationService;
        _chartJsService = chartJsService;
    }

    [HttpGet]
    [Route("{id?}")]
    public async Task<IActionResult> Index(int? id)
    {
        var test = id.HasValue ? await _testService.GetTestById(id.Value) : await _testService.GetLastTest();
        var allTests = await _testService.GetAll();
        var allGenerations = test is null ? new() : await _generationService.GetAll(test.Id);
        var bestGenerations = test is null ? new() : await _generationService.GetBest(test.Id, 25);
        var allChromosomes = test is null ? new() : await _chromosomeService.GetAll(test.Id);
        var bestChromosomes = test is null ? new() : await _chromosomeService.GetBest(test.Id, 25);

        var generationFitnessData = _chartJsService.GenerateGenerationFitnessData(allGenerations);
        var chromosomeFitnessData = _chartJsService.GenerateChromosomeFitnessData(allChromosomes);
        var averageElapsedTimeData = _chartJsService.GenerateAverageElapsedTimeData(allGenerations);
        var averageDepthData = _chartJsService.GenerateAverageDepthData(allChromosomes);
        var averageNodesData = _chartJsService.GenerateAverageNodesData(allChromosomes);
        var averageTimePerGameData = _chartJsService.GenerateAverageTimePerGameData(allChromosomes);

        return View(new MainViewModel
        {
            CurrentTest = _mapper.Map<TestViewModel>(test),
            Tests = _mapper.Map<List<TestViewModel>>(allTests),
            AllGenerations = _mapper.Map<List<GenerationViewModel>>(allGenerations),
            BestGenerations = _mapper.Map<List<GenerationViewModel>>(bestGenerations),
            AllChromosomes = _mapper.Map<List<ChromosomeViewModel>>(allChromosomes),
            BestChromosomes = _mapper.Map<List<ChromosomeViewModel>>(bestChromosomes),

            GenerationFitnessChartJson = JsonConvert.SerializeObject(generationFitnessData),
            ChromosomeFitnessChartJson = JsonConvert.SerializeObject(chromosomeFitnessData),
            AverageElapsedTimeChartJson = JsonConvert.SerializeObject(averageElapsedTimeData),
            AverageDepthChartJson = JsonConvert.SerializeObject(averageDepthData),
            AverageNodesChartJson = JsonConvert.SerializeObject(averageNodesData),
            AverageTimePerGameChartJson = JsonConvert.SerializeObject(averageTimePerGameData)
        });
    }
}
