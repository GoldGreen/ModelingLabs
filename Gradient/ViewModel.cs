using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Labs.Gradient;

internal class ViewModel : ReactiveObject
{
    [Reactive] public string Expression { get; set; }
    [Reactive] public double DerivativeDelta { get; set; }

    [Reactive] public double Epsilon { get; set; }
    [Reactive] public double LearningRate { get; set; }

    [Reactive] public int IterationsCount { get; set; }

    [Reactive] public double StartX1 { get; set; } = 0;
    [Reactive] public double StartX2 { get; set; } = 0;
    [Reactive] public GradientResult Result { get; set; }

    [Reactive] public IEnumerable<PlotModel> PlotModels { get; set; }

    public IReadOnlyList<string> Expressions { get; set; } = new string[]
    {
        "x1^3-x1*x2+x2^2-2*x1+3*x2-4",
        "(x2-x1^2)^2+(1-x1)^2",
        "(x2^2+x1^2-1)^2+(x1+x2-1)^2",
        "(x1^2+x2-11)^2+(x1+x2^2-7)^2",
        "4*(x1-5)^2+(x2-6)^2",
    };

    public ICommand CalculateCommand { get; }
    public ICommand ChangeExpressionCommand { get; }
    public ICommand ResetCommand { get; }

    public ViewModel()
    {
        Reset();
        Expression = Expressions[0];
        CalculateCommand = ReactiveCommand.CreateFromTask(Calculate);
        ResetCommand = ReactiveCommand.Create(Reset);
        CreatePlots(Enumerable.Empty<History>());
    }

    private void Reset()
    {
        DerivativeDelta = 0.000001;
        Epsilon = 0.000001;
        LearningRate = 0.01;
        IterationsCount = 200_000;
        StartX1 = 0;
        StartX2 = 0;
    }

    private async Task Calculate()
    {
        try
        {
            var result = await Task.Run(() =>
            {
                Model model = new(Expression, DerivativeDelta);
                return model.Fit(StartX1, StartX2, Epsilon, LearningRate, IterationsCount);
            });

            CreatePlots(result.History);
            Result = result;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    private void CreatePlots(IEnumerable<History> history)
    {
        PlotModels = new[]
        {
            CreatePlotModel($"({Expression}) x1", "Значения x1", "Итерация", "X1", history, x => x.Iteration, x => x.X1),
            CreatePlotModel($"({Expression}) x2", "Значения x2", "Итерация", "X2", history, x => x.Iteration, x => x.X2),
            CreatePlotModel($"({Expression}) Минимум", "Значения минимума", "Итерация", "Минимум", history, x => x.Iteration, x => x.Result),
            CreatePlotModel($"({Expression}) Общая ошибка", "Значения ошибки", "Итерация", "Ошибка", history, x => x.Iteration, x => x.X1Error + x.X2Error)
        };
    }

    private PlotModel CreatePlotModel(string title,
                                      string seriesName,
                                      string xAxisName,
                                      string yAxisName,
                                      IEnumerable<History> history,
                                      Func<History, double> xSelector,
                                      Func<History, double> ySelector)
    {

        PlotModel model = new()
        {
            Title = title,
        };

        model.Axes.Add(new LinearAxis
        {
            Title = xAxisName,
            Position = AxisPosition.Bottom,
        });

        model.Axes.Add(new LinearAxis
        {
            Title = yAxisName,
            Position = AxisPosition.Left
        });

        LineSeries lineSeries = new()
        {
            Title = seriesName
        };
        lineSeries.Points.AddRange(history.Select(x => new DataPoint(xSelector(x), ySelector(x))));
        model.Series.Add(lineSeries);

        return model;

    }
}
