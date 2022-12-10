using System;
using System.Collections.Generic;

namespace Labs.Gradient;

public record GradientResult(History Last, int IterationsCount, IReadOnlyList<History> History, TimeSpan TrainTime);
