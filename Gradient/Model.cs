using Ciloci.Flee;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Labs.Gradient;

public class Model
{
    private readonly ExpressionContext context;
    private readonly IDynamicExpression dynamicExpression;
    private readonly double delta;

    public Model(string expression, double delta)
    {
        context = new();
        context.Variables["x1"] = 0.0d;
        context.Variables["x2"] = 0.0d;
        dynamicExpression = context.CompileDynamic(expression);
        this.delta = delta;
    }

    private double Func(double x1, double x2)
    {
        context.Variables["x1"] = x1;
        context.Variables["x2"] = x2;

        return (double)dynamicExpression.Evaluate();
    }

    private double GradX1(double x1, double x2)
    {
        return (Func(x1 + delta, x2) - Func(x1, x2)) / delta;
    }

    private double GradX2(double x1, double x2)
    {
        return (Func(x1, x2 + delta) - Func(x1, x2)) / delta;
    }

    public GradientResult Fit(double startX1, double startX2, double epsilon, double lr, int iterationsCount)
    {
        var st = Stopwatch.StartNew();
        List<History> history = new();

        double x2 = startX2;
        double x1 = startX1;

        int iterationNumber = 0;
        for (; iterationNumber < iterationsCount; iterationNumber++)
        {
            double g = Func(x1, x2);

            double x1Error = GradX1(x1, x2);
            double x2Error = GradX2(x1, x2);

            (x1, x2) = (x1 - lr * x1Error, x2 - lr * x2Error);
            double y = Func(x1, x2);

            history.Add(new(iterationNumber, x1, x2, x1Error, x2Error, y));

            if (Math.Abs(y - g) < epsilon)
            {
                iterationNumber++;
                break;
            }
        }

        return new(history[^1], iterationNumber, history, st.Elapsed);
    }
}
