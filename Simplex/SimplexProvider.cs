using System.Linq;

namespace Labs.Simplex;

internal class SimplexProvider
{
    public void Calculate(VariableModel[] variables, ConditionModel[] conditions, Extremum extremum)
    {
        int addedCount = conditions.Count(x => x.Condition != Condition.Equal);

        double[,] table = new double[conditions.Length + 1, variables.Length + addedCount + 1];

        table[table.GetLength(0) - 1, 0] = 0;//Мб потом C

        double variableKoef = extremum == Extremum.Max ? -1 : 1;
        for (int i = 0; i < variables.Length; i++)
        {
            table[table.GetLength(0) - 1, i + 1] = variables[i].Value * variableKoef;
        }

        int indexOfAddedVariable = variables.Length + 1;
        for (int i = 0; i < conditions.Length; i++)
        {
            double conditionKoef = conditions[i].Condition == Condition.MoreOrEqual ? -1 : 1;
            table[i, 0] = conditions[i].Result;

            for (int j = 0; j < conditions[i].ConditionVariables.Length; j++)
            {
                table[i, j + 1] = conditions[i].ConditionVariables[j].Value * conditionKoef;
            }

            if (conditions[i].Condition != Condition.Equal)
            {
                table[i, indexOfAddedVariable++] = 1;
            }
        }

        SimplexSolver solver = new(table);
        var result = solver.Calculate(variables.Length, out var history);

        var extremumResult = variables.Select((x, i) => x.Value * result[i]).Sum();
    }
}
