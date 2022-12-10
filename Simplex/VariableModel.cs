using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Labs.Simplex;

public class VariableModel : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public double Value { get; set; }
}
