using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;

namespace Labs.Simplex;

public class ViewModel : ReactiveObject
{
    [Reactive] public int VariablesCount { get; set; } = 3;
    [Reactive] public Extremum Extremum { get; set; } = Extremum.Max;
    [Reactive]
    public Extremum[] Extremums { get; set; } = Enum.GetValues(typeof(Extremum))
                                                               .Cast<Extremum>()
                                                               .ToArray();

    [Reactive]
    public Condition[] ConditionsValues { get; set; } = Enum.GetValues(typeof(Condition))
                                                               .Cast<Condition>()
                                                               .ToArray();
    [Reactive] public int ConditionsCount { get; set; } = 3;

    [Reactive] public VariableModel[] Variables { get; set; } = Array.Empty<VariableModel>();
    [Reactive] public ConditionModel[] Conditions { get; set; } = Array.Empty<ConditionModel>();

    public ViewModel()
    {
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
}
