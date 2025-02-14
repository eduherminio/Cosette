﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cosette.Tuner.Web.ViewModels.ChartJs;

public class ChartJsDataset<T>
{
    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("borderColor")]
    public string BorderColor { get; set; }

    [JsonProperty("backgroundColor")]
    public string BackgroundColor { get; set; }

    [JsonProperty("fill")]
    public string Fill { get; set; }

    [JsonProperty("data")]
    public List<T> Data { get; set; }
}
