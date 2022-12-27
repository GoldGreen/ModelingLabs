using DynamicData;
using Labs.Simplex.Algotithm;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace Labs.Simplex;

public class ViewModel : ReactiveObject
{
    public Extremum[] Extremums { get; } = Enum.GetValues(typeof(Extremum))
                                               .Cast<Extremum>()
                                               .ToArray();

    public Condition[] ConditionsValues { get; } = Enum.GetValues(typeof(Condition))
                                                       .Cast<Condition>()
                                                       .ToArray();

    [Reactive] public int VariablesCount { get; set; } = 3;
    [Reactive] public Extremum Extremum { get; set; } = Extremum.Max;

    [Reactive] public int ConditionsCount { get; set; } = 3;
    [Reactive] public ResultModel Result { get; set; }
    [Reactive] public SeriesCollection SeriesCollection { get; set; }

    [Reactive] public VariableModel[] Variables { get; set; } = Array.Empty<VariableModel>();
    [Reactive] public ConditionModel[] Conditions { get; set; } = Array.Empty<ConditionModel>();

    public ICommand CalculateCommand { get; }

    public ViewModel()
    {
        CalculateCommand = ReactiveCommand.Create(Calculate);

        this.WhenAnyValue(x => x.VariablesCount)
            .Subscribe(count =>
            {
                var newVariables = new VariableModel[count];
                for (int i = 0; i < newVariables.Length; i++)
                {
                    if (i < Variables.Length)
                    {
                        newVariables[i] = Variables[i];
                    }
                    else
                    {
                        newVariables[i] = new VariableModel();
                    }
                    newVariables[i].Name = $"x{i + 1}";
                }
                Variables = newVariables;

                foreach (var condition in Conditions)
                {
                    var newConditionVariables = new ConditionVariable[count];
                    for (int i = 0; i < newConditionVariables.Length; i++)
                    {
                        if (i < condition.ConditionVariables.Length)
                        {
                            newConditionVariables[i] = condition.ConditionVariables[i];
                        }
                        else
                        {
                            newConditionVariables[i] = new ConditionVariable();
                        }
                        newConditionVariables[i].Name = $"x{i + 1}";
                    }
                    condition.ConditionVariables = newConditionVariables;
                }
            });

        this.WhenAnyValue(x => x.ConditionsCount)
            .Subscribe(count =>
            {
                var newConditions = new ConditionModel[count];
                for (int i = 0; i < newConditions.Length; i++)
                {
                    if (i < Conditions.Length)
                    {
                        newConditions[i] = Conditions[i];
                    }
                    else
                    {
                        newConditions[i] = new ConditionModel()
                        {
                            ConditionVariables = Enumerable.Range(0, VariablesCount)
                                                           .Select(i => new ConditionVariable
                                                           {
                                                               Name = $"x{i + 1}",
                                                           }).ToArray()
                        };
                    }
                }

                Conditions = newConditions;
            });
    }

    private void Calculate()
    {
        try
        {
            SimplexProvider provider = new();
            Result = provider.Calculate(Variables, Conditions, Extremum);
            if (Result.Method.Variables == 2)
            {
                PlotCharts(Result.Method);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    private void PlotCharts(Method method)
    {
        var seriesCollection = new SeriesCollection();

        seriesCollection.AddRange(method.Subjects.Select(x =>
        {
            var values = new ChartValues<ObservablePoint>();

            values.Add(new ObservablePoint(0, x.Result / x.Variables[1]));
            values.Add(new ObservablePoint(x.Result / x.Variables[0], 0));
            return new LineSeries() { Values = values };
        }));

        SeriesCollection = seriesCollection;
    }
}
