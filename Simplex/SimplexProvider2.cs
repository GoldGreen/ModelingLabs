using System.Linq;

namespace Labs.Simplex;

internal class SimplexProvider2
{
    public void Calculate(VariableModel[] variables, ConditionModel[] conditions, Extremum extremum)
    {
        double[] c = variables.Select(x => x.Value).ToArray();

        double[,] a = new double[conditions.Length, variables.Length];

        for (int i = 0; i < conditions.Length; i++)
        {
            for (int j = 0; j < conditions[i].ConditionVariables.Length; j++)
            {
                a[i, j] = conditions[i].ConditionVariables[j].Value;
            }

        }

        double[] b = conditions.Select(x => x.Result).ToArray();

        SimplexSolver2 solver = new(a, b, c);

        var res = (solver.Value(), solver.Dual(), solver.Primal());
    }
}
