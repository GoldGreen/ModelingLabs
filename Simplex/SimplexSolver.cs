using System.Collections.Generic;

public class SimplexSolver
{
    //source - симплекс таблица без базисных переменных
    private double[,] table; //симплекс таблица

    private int m, n;

    private List<int> basis; //список базисных переменных

    public SimplexSolver(double[,] source)
    {
        m = source.GetLength(0);
        n = source.GetLength(1);
        table = new double[m, n + m - 1];
        basis = new List<int>();

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < table.GetLength(1); j++)
            {
                if (j < n)
                    table[i, j] = source[i, j];
                else
                    table[i, j] = 0;
            }
            //выставляем коэффициент 1 перед базисной переменной в строке
            if ((n + i) < table.GetLength(1))
            {
                table[i, n + i] = 1;
                basis.Add(n + i);
            }
        }

        n = table.GetLength(1);
    }

    //result - в этот массив будут записаны полученные значения X
    public double[] Calculate(int variablesCount, out IEnumerable<double[,]> history)
    {
        int mainCol, mainRow; //ведущие столбец и строка

        List<double[,]> historyList = new()
        {
            table
        };

        while (!IsItEnd())
        {
            mainCol = FindMainCol();
            mainRow = FindMainRow(mainCol);
            basis[mainRow] = mainCol;

            double[,] new_table = new double[m, n];

            for (int j = 0; j < n; j++)
                new_table[mainRow, j] = table[mainRow, j] / table[mainRow, mainCol];

            for (int i = 0; i < m; i++)
            {
                if (i == mainRow)
                    continue;

                for (int j = 0; j < n; j++)
                    new_table[i, j] = table[i, j] - table[i, mainCol] * new_table[mainRow, j];
            }
            table = new_table;
            historyList.Add(table);
        }

        double[] result = new double[variablesCount];
        for (int i = 0; i < result.Length; i++)
        {
            int k = basis.IndexOf(i + 1);
            if (k != -1)
                result[i] = table[k, 0];
            else
                result[i] = 0;
        }

        history = historyList;
        return result;
    }

    private bool IsItEnd()
    {
        bool flag = true;

        for (int j = 1; j < n; j++)
        {
            if (table[m - 1, j] < 0)
            {
                flag = false;
                break;
            }
        }

        return flag;
    }

    private int FindMainCol()
    {
        int mainCol = 1;

        for (int j = 2; j < n; j++)
            if (table[m - 1, j] < table[m - 1, mainCol])
                mainCol = j;

        return mainCol;
    }

    private int FindMainRow(int mainCol)
    {
        int mainRow = 0;

        for (int i = 0; i < m - 1; i++)
            if (table[i, mainCol] > 0)
            {
                mainRow = i;
                break;
            }

        for (int i = mainRow + 1; i < m - 1; i++)
            if ((table[i, mainCol] > 0) && ((table[i, 0] / table[i, mainCol]) < (table[mainRow, 0] / table[mainRow, mainCol])))
                mainRow = i;

        return mainRow;
    }
}