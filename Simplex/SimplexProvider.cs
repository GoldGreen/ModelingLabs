using Labs.Simplex.Algotithm;
using System.Linq;

namespace Labs.Simplex;

public class SimplexProvider
{
    public ResultModel Calculate(VariableModel[] variables, ConditionModel[] conditions, Extremum extremum)
    {
        SimplexSolver simplexSolver = new(variables.Length,
                                          extremum,
                                          variables.Select(x => x.Value).ToArray(),
                                          conditions.Select(x => (x.ConditionVariables.Select(x => x.Value).ToArray(), x.Condition, x.Result)).ToArray());

        var res = simplexSolver.Resolve(out string log);
        return new(res, log, simplexSolver.Method);
    }
}
