﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Cosette.Tuner.Texel.Engine;

public class EngineOperator
{
    private readonly string _enginePath;
    private readonly string _engineArguments;
    private Process _engineProcess;
    private readonly Dictionary<string, string> _options;

    public EngineOperator(string path, string arguments)
    {
        _enginePath = path;
        _engineArguments = arguments;
        _options = new Dictionary<string, string>();
    }

    public void Init()
    {
        _engineProcess = Process.Start(new ProcessStartInfo
        {
            FileName = _enginePath,
            Arguments = _engineArguments,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        });
    }

    public void Restart()
    {
        if (!_engineProcess.HasExited)
        {
            _engineProcess.Close();
        }

        Init();
        ApplyOptions();
    }

    public void SetOption(string name, string value)
    {
        _options[name] = value;
    }

    public void ApplyOptions()
    {
        Write("uci");
        WaitForMessage("uciok");

        Write("isready");
        WaitForMessage("readyok");

        foreach (var option in _options)
        {
            Write($"setoption name {option.Key} value {option.Value}");
        }

        Write("isready");

        if (!WaitForMessage("readyok"))
        {
            throw new Exception("Invalid option passed to the engine");
        }

        Write("leave");
    }

    public void LoadEpd(string epdPath)
    {
        Write($"tuner load {epdPath}");

        var response = string.Empty;
        while (response.Contains("Ok"))
        {
            response = Read();
        }
    }

    public double Evaluate(double scalingFactor)
    {
        Write($"tuner evaluate {scalingFactor}");

        while (true)
        {
            var response = Read();
            if (response == "Ok")
            {
                continue;
            }

            return double.Parse(response, CultureInfo.InvariantCulture);
        }
    }

    public void Write(string message)
    {
        _engineProcess.StandardInput.WriteLine(message);
    }

    public string Read()
    {
        return _engineProcess.StandardOutput.ReadLine();
    }

    public bool WaitForMessage(string message)
    {
        while (true)
        {
            var response = Read();
            if (response.StartsWith("error"))
            {
                return false;
            }

            if (response == message)
            {
                return true;
            }
        }
    }
}
