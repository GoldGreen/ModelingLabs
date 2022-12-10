using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Labs.Simplex;

public class ConditionModel : ReactiveObject
{
    [Reactive]
    public Condition Condition { get; set; }

    [Reactive]
    public ConditionVariable[] ConditionVariables { get; set; }

    [Reactive]
    public double Result { get; set; }
}
