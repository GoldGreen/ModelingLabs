using Simplex.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Labs.Simplex.Algotithm;

internal class SimplexSolver
{
    public Method Method { get; }

    public SimplexSolver(int variables,
                        Extremum function,
                        double[] objectiveVariables,
                        (double[] variables, Condition sign, double result)[] subjects)
    {
        Method = new()
        {
            Function = function,
            Variables = variables,
            ObjectiveVariables = objectiveVariables,
            Subjects = subjects.Select(x => new Subject
            {
                Result = x.result,
                Sign = x.sign,
                Variables = x.variables
            }).ToArray()
        };
    }

    public string Resolve(out string log)
    {
        string resolveLog = "";
        string res = string.Empty;

        Tech(out var techLog);
        resolveLog += techLog;

        if (Method.Function == Extremum.Max)
        {
            var Mmethod = Method.Subjects.Any(x => x.Sign == Condition.MoreOrEqual);

            if (Mmethod)
            {
                res = Maximaze(out var maxLog);
                resolveLog += maxLog;
            }
            else
            {
                res = Maximaze2(out var max2Log);
                resolveLog += max2Log;
            }
        }
        else if (Method.Function == Extremum.Min)
        {
            res = Minimaze(out var minLog);
            resolveLog += minLog;
        }

        log = resolveLog;
        return res;
    }

    private void Tech(out string log)
    {
        var text = string.Empty;

        //File.WriteAllText(_log, text);

        text += "Исходные данные:";
        text += Environment.NewLine;
        text += Environment.NewLine;

        for (int i = 0; i < Method.ObjectiveVariables.Length; i++)
        {
            var b = Method.ObjectiveVariables[i];

            if (b != 0.0d)
            {
                text += (b >= 0 && i > 0) ? "+" : string.Empty;
                text += string.Format("{0:F1}*x{1}", b, i + 1);
            }
        }

        text += " → ";
        text += Method.Function.ToString().Substring(0, 3);

        text += Environment.NewLine;
        text += Environment.NewLine;

        for (int i = 0; i < Method.Subjects.Length; i++)
        {
            for (int j = 0; j < Method.Subjects[i].Variables.Length; j++)
            {
                var b = Method.Subjects[i].Variables[j];

                if (b != 0.0d)
                {
                    text += (b >= 0 && j > 0) ? "+" : string.Empty;
                    text += string.Format("{0:F1}*x{1}", b, j + 1);
                }
            }

            text += " ";

            switch (Method.Subjects[i].Sign)
            {
                case Condition.Equal:
                    text += "=";
                    break;
                case Condition.LessOrEqual:
                    text += "<=";
                    break;
                case Condition.MoreOrEqual:
                    text += ">=";
                    break;
            }

            text += " ";

            text += Method.Subjects[i].Result.ToString("F1");
            text += Environment.NewLine;
        }

        log = text;
        //File.AppendAllText(_log, text);
    }

    private string Maximaze(out string log)
    {
        var result = 0;
        var text = string.Empty;
        var smpTable = new SimplexTable[10];

        for (int i = 0; i < 10; i++)
        {
            smpTable[i] = new SimplexTable();

            smpTable[i].baz.letter = '?';
            smpTable[i].baz.index = 0;
            smpTable[i].fr_mem = 0;
            smpTable[i].ocenka = 0;

            for (int c = 0; c < 16; c++)
            {
                smpTable[i].vars[c] = 0;
            }
        }

        text += Environment.NewLine;


        // Введем выравнивающие переменные

        text += "Введем выравнивающие и искусственные переменные:";
        text += Environment.NewLine;
        text += Environment.NewLine;

        var xVar = Method.Variables;
        var xSynt = 0;

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            for (int c = 0; c < Method.Variables; c++)
            {
                double k = Method.Subjects[r].Variables[c];

                if (c != 0)
                {
                    text += " ";
                }

                if (c != 0 && k > 0)
                {
                    text += "+";
                }

                if (k != 0)
                {
                    text += string.Format("{0:F1}*x{1}", k, c + 1);
                }
            }

            if (Method.Subjects[r].Sign == Condition.Equal ||
                Method.Subjects[r].Sign == Condition.LessOrEqual)
            {
                text += string.Format(" +{0:F1}*x{1}", 1, xVar + r + 1);

                if (Method.Subjects[r].Result < 0)
                {
                    text += string.Format(" -{0:F1}*y{1}", 1, ++xSynt);

                    for (int c = 0; c < Method.Variables; c++)
                    {
                        smpTable[r].vars[c] = -1 * Method.Subjects[r].Variables[c];
                    }

                    smpTable[r].baz.letter = 'y';
                    smpTable[r].baz.index = xSynt;
                }
                else
                {
                    smpTable[r].baz.letter = 'x';
                    smpTable[r].baz.index = xVar + r + 1;

                    for (int c = 0; c < Method.Variables; c++)
                    {
                        smpTable[r].vars[c] = Method.Subjects[r].Variables[c];
                    }
                }
            }
            else
            {
                text += string.Format(" -{0:F1}*x{1}", 1, xVar + r + 1);

                if (Method.Subjects[r].Result > 0)
                {
                    text += string.Format(" +{0:F1}*y{1}", 1, ++xSynt);

                    for (int c = 0; c < Method.Variables; c++)
                    {
                        smpTable[r].vars[c] = Method.Subjects[r].Variables[c];
                    }

                    smpTable[r].vars[Method.Variables + r] = -1;
                    smpTable[r].baz.letter = 'y';
                    smpTable[r].baz.index = xSynt;
                }
                else
                {
                    smpTable[r].baz.letter = 'x';
                    smpTable[r].baz.index = xVar + r + 1;

                    for (int c = 0; c < Method.Variables; c++)
                    {
                        smpTable[r].vars[c] = -Method.Subjects[r].Variables[c];
                    }
                }
            }

            Method.SynthVariables = xSynt;

            text += " =";

            if (Method.Subjects[r].Result > 0)
            {
                smpTable[r].fr_mem = Method.Subjects[r].Result;
            }
            else
            {
                smpTable[r].fr_mem = -Method.Subjects[r].Result;
            }

            text += string.Format(" {0:F1}", Method.Subjects[r].Result);
            text += Environment.NewLine;
        }

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
            {
                if (smpTable[r].baz.index == (c + 1) && smpTable[r].baz.letter == 'x')
                {
                    smpTable[r].vars[c] = 1;
                }

                if (smpTable[r].baz.index == (c + 1 - Method.Variables - Method.Subjects.Length) && smpTable[r].baz.letter == 'y')
                {
                    smpTable[r].vars[c] = 1;
                }

                if (smpTable[r].baz.index == (c + 1 - Method.Variables) && smpTable[r].baz.letter == 'y' && c > Method.Subjects.Length + Method.Variables)
                {
                    if (Method.Subjects[r + 1].Result > 0)
                    {
                        if (Method.Subjects[r].Sign == Condition.MoreOrEqual)
                        {
                            smpTable[r].vars[c] = 1;
                        }
                        else
                        {
                            smpTable[r].vars[c] = -1;
                        }
                    }
                    else
                    {
                        smpTable[r].vars[c] = -1;
                    }
                }
            }
        }

        for (int c = 0; c < Method.Variables; c++)
        {
            smpTable[Method.Subjects.Length].vars[c] = -Method.Subjects[1].Variables[c];
        }

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
            {
                if (smpTable[r].baz.letter == 'y')
                {
                    smpTable[Method.Subjects.Length + 1].vars[c] += -smpTable[r].vars[c];
                }
            }
        }

        text += "F(x) =";

        for (int r = 0; r < Method.Variables; r++)
        {
            double k = Method.Subjects[1].Variables[r];

            if (r != 0 && k > 0)
            {
                text += " +";
            }

            if (k != 0)
            {
                text += string.Format("{0:F1}*x{1}", k, r + 1);
            }
        }

        if (xSynt > 0)
        {
            text += " -M*(y1";

            for (int r = 1; r < xSynt; r++)
            {
                text += string.Format(" +y{0}", r + 1);
            }

            text += ")";
            //imp = 1;
        }

        //вычисление значения М-функции

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            if (smpTable[r].baz.letter == 'y')
            {
                smpTable[Method.Subjects.Length + 1].fr_mem = smpTable[Method.Subjects.Length + 1].fr_mem - smpTable[r].fr_mem;
            }
        }

        text += " → Max";
        text += Environment.NewLine;
        text += Environment.NewLine;


        // Решение задачи

        text += "Решение задачи:";

        text += Environment.NewLine;
        text += Environment.NewLine;

        bool b_n_opt = true;
        var tick = 0;

        //File.AppendAllText(_log, text);
        text = string.Empty;

        do
        {
            if (Method.SynthVariables == 0 && tick > 1)
            {
                text += Res(smpTable);
            }

            text += string.Format("Шаг №{0}", tick + 1);

            text += Environment.NewLine;
            text += Environment.NewLine;

            //File.AppendAllText(_log, text);

            MStep(smpTable, ref result, ref tick);

            text = string.Empty;
            var n_opt = 0;

            for (int c = 0; c < Method.Variables + Method.Subjects.Length; c++)
            {
                if (smpTable[Method.Subjects.Length].vars[c] >= 0 && Method.SynthVariables == 0)
                {
                    n_opt++;
                }
            }

            if (n_opt != Method.Variables + Method.Subjects.Length)
            {
                b_n_opt = true;
            }
            else
            {
                b_n_opt = false;
                result = 1;
            }

            tick++;

            if (tick > 50 || result < 0)
            {
                break;
            }
        }
        while (b_n_opt);

        PrintTable2(smpTable, -1, -1, false);

        text += string.Format("F(x) = {0}", smpTable[Method.Subjects.Length].fr_mem);
        text += Environment.NewLine;
        text += "X => ";

        for (int p = 0; p < Method.Subjects.Length + Method.Variables; p++)
        {
            int p2 = 0;

            for (int p1 = 0; p1 < Method.Subjects.Length + Method.Variables; p1++)
            {
                if (smpTable[p1].baz.index == p + 1)
                {
                    if (p + 1 == Method.Subjects.Length + Method.Variables)
                    {
                        text += string.Format("{0} ", smpTable[p1].fr_mem);
                    }
                    else
                    {
                        text += string.Format("{0}; ", smpTable[p1].fr_mem);
                    }

                    break;
                }

                p2++;
            }

            if (p2 == Method.Subjects.Length + Method.Variables)
            {
                if (p + 1 == Method.Subjects.Length + Method.Variables)
                {
                    text += " 0 ";
                }
                else
                {
                    text += "0; ";
                }
            }
        }

        text += Environment.NewLine;
        text += Environment.NewLine;

        switch (result)
        {
            case 1:
                text += "Оптимальное решение найдено!";
                break;

            default:
                text += "Решение не допустимо!";
                break;
        }

        //File.AppendAllText(_log, text);

        var rr = string.Empty;

        for (int i = 0; i < Method.Variables; i++)
        {
            var e = smpTable.Where(x => x.baz.letter == 'x' && x.baz.index == (i + 1));

            if (i > 0)
            {
                rr += " + ";
            }

            if (e.Count() > 0)
            {
                rr += e.First().fr_mem.ToString() + "*" + Method.ObjectiveVariables[i].ToString("F1");
            }
            else
            {
                rr += "0*" + Method.ObjectiveVariables[i].ToString("F1");
            }
        }

        rr += " = " + smpTable[Method.Subjects.Length].fr_mem.ToString();

        log = text;
        return rr;
    }

    private string Maximaze2(out string log)
    {
        var cycle = 50;
        var result = 0;
        var text = string.Empty;

        var xs = Method.Variables + Method.Subjects.Length;

        /*var cnt = Method.Subjects.Count(x => x.Sign == Sign.GreaterThanEqual);
        xs += cnt;*/

        var tbl = new SimplexTable2()
        {
            Equations = new List<Equation>(),
            ObjFunctions = new Equation()
            {
                Basis = 0,
                Coefficients = new double[xs],
                Solution = 0
            }
        };

        // Введем выравнивающие переменные

        text += Environment.NewLine;
        text += "Введем выравнивающие и искусственные переменные:";
        text += Environment.NewLine;
        text += Environment.NewLine;

        var idx = 0;

        foreach (var s in Method.Subjects)
        {
            var inv = s.Sign == Condition.MoreOrEqual;

            var eq = new Equation()
            {
                Basis = idx + Method.Variables + 1,
                Coefficients = new double[xs],
                Solution = !inv ? s.Result : -s.Result
            };

            for (int i = 0; i < xs; i++)
            {
                if (i < s.Variables.Length)
                {
                    eq.Coefficients[i] = !inv ? s.Variables[i] : -s.Variables[i];

                    text += string.Format("{0}{1:F1}*x{2}",
                        i > 0 && eq.Coefficients[i] >= 0 ? "+" : string.Empty,
                        eq.Coefficients[i], i + 1);
                }
                else
                {
                    eq.Coefficients[i + idx] = 1;

                    text += string.Format("+{0:F1}*x{1}", eq.Coefficients[i + idx], i + idx + 1);

                    break;
                }
            }

            text += string.Format(" = {0:F1}", eq.Solution);
            text += Environment.NewLine;

            idx++;
            tbl.Equations.Add(eq);
        }

        /*text += Environment.NewLine;

        var Mmeth = Method.Subjects.Where(x => x.Sign == Sign.GreaterThanEqual);

        if (Mmeth.Count() > 0)
        {
            var c = 0;

            foreach (var y in Mmeth)
            {
                var Midx = Method.Subjects.IndexOf(y);

                var eq = tbl.Equations[Midx];

                eq.Basis = Method.Variables + Method.Subjects.Count + c + 1;
                eq.Coefficients = eq.Coefficients.Select(x => -x).ToArray();
                eq.Coefficients[Method.Variables + Method.Subjects.Count + c] = 1;
                eq.Solution = -eq.Solution;

                c++;
            }
        }*/

        text += Environment.NewLine;

        for (int i = 0; i < xs; i++)
        {
            if (i < Method.Variables)
            {
                tbl.ObjFunctions.Coefficients[i] = -Method.ObjectiveVariables[i];
            }

            text += string.Format("{0}{1:F1}*x{2}",
                        i > 0 && tbl.ObjFunctions.Coefficients[i] >= 0 ? "+" : string.Empty,
                        tbl.ObjFunctions.Coefficients[i], i + 1);
        }

        text += " → Max";
        text += Environment.NewLine;
        text += Environment.NewLine;


        // Решение задачи

        text += "Решение задачи:";

        text += Environment.NewLine;
        text += Environment.NewLine;

        //File.AppendAllText(_log, text);
        text = string.Empty;

        for (int i = 0; i < cycle; i++)
        {
            var t1 = tbl.ObjFunctions.Coefficients.Any(x => x < 0);
            var t2 = tbl.Equations.Any(x => x.Solution < 0);

            if (!(t1 || t2))
            {
                result = 1;
                break;
            }

            text += string.Format("Шаг №{0}", i);

            text += Environment.NewLine;
            text += Environment.NewLine;

            var row = -1;
            var column = -1;

            var maxIdx = tbl.ObjFunctions.Coefficients.Max(x => Math.Abs(x));
            column = tbl.ObjFunctions.Coefficients
                .Select(x => Math.Abs(x))
                .ToList()
                .IndexOf(maxIdx);

            var res = new double[tbl.Equations.Count];

            for (int j = 0; j < tbl.Equations.Count; j++)
            {
                res[j] = tbl.Equations[j].Coefficients[column] < 0
                    ? double.PositiveInfinity
                    : tbl.Equations[j].Solution / tbl.Equations[j].Coefficients[column];
            }

            var minIdx = res.Min();
            row = res.ToList().IndexOf(minIdx);

            text += PrintTable(tbl, column, row);

            var tmp = new SimplexTable2()
            {
                Equations = new List<Equation>(tbl.Equations.Select(x => new Equation()
                {
                    Basis = x.Basis,
                    Coefficients = x.Coefficients.ToArray(),
                    Solution = x.Solution
                })),
                ObjFunctions = new Equation()
                {
                    Basis = 0,
                    Coefficients = tbl.ObjFunctions.Coefficients.ToArray(),
                    Solution = tbl.ObjFunctions.Solution
                }
            };

            for (int j = 0; j < tbl.Equations.Count; j++)
            {
                if (j == row)
                {
                    tmp.Equations[j].Coefficients[column] = 1;
                }
                else
                {
                    tmp.Equations[j].Coefficients[column] = 0;
                }
            }

            for (int a = 0; a < tbl.Equations.Count; a++)
            {
                for (int b = 0; b < tbl.Equations[a].Coefficients.Length; b++)
                {
                    if (a == row)
                    {
                        tmp.Equations[a].Coefficients[b] =
                            tbl.Equations[a].Coefficients[b] / tbl.Equations[row].Coefficients[column];
                        tmp.Equations[a].Basis = column + 1;
                    }
                    else
                    {
                        tmp.Equations[a].Coefficients[b] =
                            tbl.Equations[a].Coefficients[b] - (tbl.Equations[row].Coefficients[b] *
                            tbl.Equations[a].Coefficients[column] / tbl.Equations[row].Coefficients[column]);
                    }
                }

                if (a == row)
                {
                    tmp.Equations[a].Solution =
                        tbl.Equations[a].Solution / tbl.Equations[row].Coefficients[column];
                }
                else
                {
                    tmp.Equations[a].Solution =
                        tbl.Equations[a].Solution - (tbl.Equations[row].Solution *
                        tbl.Equations[a].Coefficients[column] / tbl.Equations[row].Coefficients[column]);
                }
            }

            for (int a = 0; a < tbl.ObjFunctions.Coefficients.Length; a++)
            {
                if (a == column)
                {
                    tmp.ObjFunctions.Coefficients[a] = 0;
                }
                else
                {
                    tmp.ObjFunctions.Coefficients[a] =
                        tbl.ObjFunctions.Coefficients[a] - (tbl.Equations[row].Coefficients[a] *
                        tbl.ObjFunctions.Coefficients[column] / tbl.Equations[row].Coefficients[column]);
                }
            }

            tmp.ObjFunctions.Solution =
                tbl.ObjFunctions.Solution - (tbl.Equations[row].Solution *
                tbl.ObjFunctions.Coefficients[column] / tbl.Equations[row].Coefficients[column]);

            tbl = tmp;

            text += PrintTable(tbl, null, null);
        }

        text += string.Format("F(x) = {0:F2}", tbl.ObjFunctions.Solution);
        text += Environment.NewLine;

        text += string.Join(", ", tbl.Equations.Select(x => string.Format("x{0} = {1:F2}", x.Basis, x.Solution)));
        text += Environment.NewLine;

        switch (result)
        {
            case 1:
                text += "Оптимальное решение найдено!";
                break;

            default:
                text += "Решение не допустимо!";
                break;
        }

        //File.AppendAllText(_log, text);
        log = text;
        return string.Format("F(x) = {0:F2}; {1}", tbl.ObjFunctions.Solution,
            string.Join(", ", tbl.Equations.Select(x => string.Format("x{0} = {1:F2}", x.Basis, x.Solution))));
    }

    private string Minimaze(out string log)
    {
        var cycle = 50;
        var result = 0;
        var text = string.Empty;

        var xs = Method.Variables + Method.Subjects.Length;

        var tbl = new SimplexTable2()
        {
            Equations = new List<Equation>(),
            ObjFunctions = new Equation()
            {
                Basis = 0,
                Coefficients = new double[xs],
                Solution = 0
            }
        };

        // Введем выравнивающие переменные

        text += Environment.NewLine;
        text += "Введем выравнивающие и искусственные переменные:";
        text += Environment.NewLine;
        text += Environment.NewLine;

        var idx = 0;

        foreach (var s in Method.Subjects)
        {
            var inv = s.Sign == Condition.MoreOrEqual;

            var eq = new Equation()
            {
                Basis = idx + Method.Variables + 1,
                Coefficients = new double[xs],
                Solution = !inv ? s.Result : -s.Result
            };

            for (int i = 0; i < xs; i++)
            {
                if (i < s.Variables.Length)
                {
                    eq.Coefficients[i] = !inv ? s.Variables[i] : -s.Variables[i];

                    text += string.Format("{0}{1:F1}*x{2}",
                        i > 0 && eq.Coefficients[i] >= 0 ? "+" : string.Empty,
                        eq.Coefficients[i], i + 1);
                }
                else
                {
                    eq.Coefficients[i + idx] = 1;

                    text += string.Format("+{0:F1}*x{1}", eq.Coefficients[i + idx], i + idx + 1);

                    break;
                }
            }

            text += string.Format(" = {0:F1}", eq.Solution);
            text += Environment.NewLine;

            idx++;
            tbl.Equations.Add(eq);
        }

        text += Environment.NewLine;

        for (int i = 0; i < xs; i++)
        {
            if (i < Method.Variables)
            {
                tbl.ObjFunctions.Coefficients[i] = -Method.ObjectiveVariables[i];
            }

            text += string.Format("{0}{1:F1}*x{2}",
                        i > 0 && tbl.ObjFunctions.Coefficients[i] >= 0 ? "+" : string.Empty,
                        tbl.ObjFunctions.Coefficients[i], i + 1);
        }

        text += " → Min";
        text += Environment.NewLine;
        text += Environment.NewLine;


        // Решение задачи

        text += "Решение задачи:";

        text += Environment.NewLine;
        text += Environment.NewLine;

        //File.AppendAllText(_log, text);
        text = string.Empty;

        for (int i = 0; i < cycle; i++)
        {
            var t1 = tbl.ObjFunctions.Coefficients.Any(x => x > 0);
            var t2 = tbl.Equations.Any(x => x.Solution < 0);

            if (!(t1 || t2))
            {
                result = 1;
                break;
            }

            text += string.Format("Шаг №{0}", i);

            text += Environment.NewLine;
            text += Environment.NewLine;

            var row = -1;
            var column = -1;

            var maxIdx = tbl.ObjFunctions.Coefficients.Max(x => Math.Abs(x));
            column = tbl.ObjFunctions.Coefficients.ToList().IndexOf(maxIdx);

            var res = new double[tbl.Equations.Count];

            for (int j = 0; j < tbl.Equations.Count; j++)
            {
                res[j] = tbl.Equations[j].Coefficients[column] < 0
                    ? double.PositiveInfinity
                    : tbl.Equations[j].Solution / tbl.Equations[j].Coefficients[column];
            }

            var minIdx = res.Min();
            row = res.ToList().IndexOf(minIdx);

            text += PrintTable(tbl, column, row);

            var tmp = new SimplexTable2()
            {
                Equations = new List<Equation>(tbl.Equations.Select(x => new Equation()
                {
                    Basis = x.Basis,
                    Coefficients = x.Coefficients.ToArray(),
                    Solution = x.Solution
                })),
                ObjFunctions = new Equation()
                {
                    Basis = 0,
                    Coefficients = tbl.ObjFunctions.Coefficients.ToArray(),
                    Solution = tbl.ObjFunctions.Solution
                }
            };

            for (int j = 0; j < tbl.Equations.Count; j++)
            {
                if (j == row)
                {
                    tmp.Equations[j].Coefficients[column] = 1;
                }
                else
                {
                    tmp.Equations[j].Coefficients[column] = 0;
                }
            }

            for (int a = 0; a < tbl.Equations.Count; a++)
            {
                for (int b = 0; b < tbl.Equations[a].Coefficients.Length; b++)
                {
                    if (a == row)
                    {
                        tmp.Equations[a].Coefficients[b] =
                            tbl.Equations[a].Coefficients[b] / tbl.Equations[row].Coefficients[column];
                        tmp.Equations[a].Basis = column + 1;
                    }
                    else
                    {
                        tmp.Equations[a].Coefficients[b] =
                            tbl.Equations[a].Coefficients[b] - (tbl.Equations[row].Coefficients[b] *
                            tbl.Equations[a].Coefficients[column] / tbl.Equations[row].Coefficients[column]);
                    }
                }

                if (a == row)
                {
                    tmp.Equations[a].Solution =
                        tbl.Equations[a].Solution / tbl.Equations[row].Coefficients[column];
                }
                else
                {
                    tmp.Equations[a].Solution =
                        tbl.Equations[a].Solution - (tbl.Equations[row].Solution *
                        tbl.Equations[a].Coefficients[column] / tbl.Equations[row].Coefficients[column]);
                }
            }

            for (int a = 0; a < tbl.ObjFunctions.Coefficients.Length; a++)
            {
                if (a == column)
                {
                    tmp.ObjFunctions.Coefficients[a] = 0;
                }
                else
                {
                    tmp.ObjFunctions.Coefficients[a] =
                        tbl.ObjFunctions.Coefficients[a] - (tbl.Equations[row].Coefficients[a] *
                        tbl.ObjFunctions.Coefficients[column] / tbl.Equations[row].Coefficients[column]);
                }
            }

            tmp.ObjFunctions.Solution =
                tbl.ObjFunctions.Solution - (tbl.Equations[row].Solution *
                tbl.ObjFunctions.Coefficients[column] / tbl.Equations[row].Coefficients[column]);

            tbl = tmp;

            text += PrintTable(tbl, null, null);
        }

        text += string.Format("F(x) = {0:F2}", tbl.ObjFunctions.Solution);
        text += Environment.NewLine;

        text += string.Join(", ", tbl.Equations.Select(x => string.Format("x{0} = {1:F2}", x.Basis, x.Solution)));
        text += Environment.NewLine;

        switch (result)
        {
            case 1:
                text += "Оптимальное решение найдено!";
                break;

            default:
                text += "Решение не допустимо!";
                break;
        }

        //File.AppendAllText(_log, text);

        log = text;
        return string.Format("F(x) = {0:F2}; {1}", tbl.ObjFunctions.Solution,
            string.Join(", ", tbl.Equations.Select(x => string.Format("x{0} = {1:F2}", x.Basis, x.Solution))));
    }

    private string PrintTable(SimplexTable2 table, int? column, int? row)
    {
        var text = string.Empty;
        var idx = 0;
        var xs = Method.Variables + Method.Subjects.Length;

        text += "Базис\t";

        for (int i = 0; i < xs; i++)
        {
            text += string.Format("x{0}\t", i + 1);
        }

        text += "БР";
        text += Environment.NewLine;

        foreach (var s in table.Equations)
        {
            text += string.Format("x{0}\t", s.Basis);

            for (int i = 0; i < s.Coefficients.Length; i++)
            {
                text += string.Format("{0}{1:F2}\t",
                    i == column && idx == row ? ">" : string.Empty,
                    s.Coefficients[i]);
            }

            idx++;

            text += string.Format("{0:F1}", s.Solution);
            text += Environment.NewLine;
        }

        text += "F(x)\t";

        for (int i = 0; i < table.ObjFunctions.Coefficients.Length; i++)
        {
            text += string.Format("{0:F2}\t", table.ObjFunctions.Coefficients[i]);
        }

        text += string.Format("{0:F1}", table.ObjFunctions.Solution);

        text += Environment.NewLine;
        text += Environment.NewLine;

        return text;
    }

    private void PrintTable2(SimplexTable[] smpTable, int aRow, int aCol, bool mr)
    {
        var text = string.Empty;

        text += "Базис\tСв.чл.\t";

        for (int c = 0; c < Method.Variables + Method.Subjects.Length + Method.SynthVariables; c++)
        {
            if (c < Method.Variables + Method.Subjects.Length)
            {
                text += string.Format("x{0}\t", c + 1);
            }
            else
            {
                text += string.Format("y{0}\t", c + 1 - (Method.Variables + Method.Subjects.Length));
            }
        }

        if (mr)
        {
            text += "Оцен\t";
        }

        text += Environment.NewLine;

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            text += string.Format("{0}{1}\t", smpTable[r].baz.letter, smpTable[r].baz.index);
            text += string.Format("{0:F1}\t", smpTable[r].fr_mem);

            for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
            {
                if (c == aCol && r == aRow)
                {
                    text += string.Format(">{0:F1}\t", smpTable[r].vars[c]);
                }
                else
                {
                    text += string.Format("{0:F1}\t", smpTable[r].vars[c]);
                }
            }

            if (mr)
            {
                if (smpTable[r].ocenka == double.MaxValue)
                {
                    text += "∞\t";
                }
                else if (smpTable[r].ocenka == double.MaxValue)
                {
                    text += "-∞\t";
                }
                else
                {
                    text += string.Format("{0:F1}\t", smpTable[r].ocenka);
                }
            }

            text += Environment.NewLine;
        }

        text += "F(x)\t";
        text += string.Format("{0:F1}\t", smpTable[Method.Subjects.Length].fr_mem);

        for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
        {
            text += string.Format("{0:F1}\t", smpTable[Method.Subjects.Length].vars[c]);
        }

        if (mr && Method.Function == Extremum.Max)
        {
            text += "max\t";
        }
        if (mr && Method.Function == Extremum.Max)
        {
            text += "min\t";
        }

        text += Environment.NewLine;

        if (Method.SynthVariables > 0)
        {
            text += "M fn\t";
            text += string.Format("{0:F1}\t", smpTable[Method.Subjects.Length + 1].fr_mem);

            for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
            {
                text += string.Format("{0:F1}\t", smpTable[Method.Subjects.Length + 1].vars[c]);
            }

            if (mr && Method.Function == Extremum.Max)
            {
                text += "max\t";
            }
            if (mr && Method.Function == Extremum.Min)
            {
                text += "min\t";
            }

            text += Environment.NewLine;
        }

        text += Environment.NewLine;
        text += Environment.NewLine;

        //File.AppendAllText(_log, text);
    }

    private string Res(SimplexTable[] smpTable)
    {
        var text = string.Empty;

        text += string.Format("F(x) = {0:F1}", smpTable[Method.SynthVariables + Method.Subjects.Length].fr_mem);
        text += "X = (";

        for (int p = 0; p < Method.Subjects.Length + Method.Variables; p++)
        {
            int p2 = 0;

            for (int p1 = 0; p1 < Method.Subjects.Length + Method.Variables; p1++)
            {
                if (smpTable[p1].baz.index == p + 1)
                {
                    if (p + 1 == Method.Subjects.Length + Method.Variables)
                    {
                        text += string.Format("{0}) ", smpTable[p1].fr_mem);
                    }
                    else
                    {
                        text += string.Format("{0}; ", smpTable[p1].fr_mem);
                    }
                    break;
                }

                p2++;
            }
            if (p2 == Method.Subjects.Length + Method.Variables)
            {
                if (p + 1 == Method.Subjects.Length + Method.Variables)
                {
                    text += " 0) ";
                }
                else
                {
                    text += "0; ";
                }
            }
        }

        return text;
    }

    private void MStep(SimplexTable[] smpTable, ref int result, ref int tick)
    {
        //поиск минимума М-функции
        int IndMinR = 0;
        int IndMinC = 0;
        int sm = 0;

        if (Method.SynthVariables > 0)
        {
            sm = 1;
        }

        double min = smpTable[Method.Subjects.Length + sm].vars[0];

        for (int c = 0; c < Method.Variables + Method.Subjects.Length; c++)
        {
            if (smpTable[Method.Subjects.Length + sm].vars[c] < min)
            {
                min = smpTable[Method.Subjects.Length + sm].vars[c];
                IndMinC = c;
            }
        }

        //расчет оценок  и поиск мин оценки

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            if ((smpTable[r].fr_mem > 0 && smpTable[r].vars[IndMinC] < 0) ||
                (smpTable[r].fr_mem < 0 && smpTable[r].vars[IndMinC] > 0))
            {
                smpTable[r].ocenka = double.MaxValue;
            }
            else
            {
                if (smpTable[r].vars[IndMinC] != 0)
                {
                    smpTable[r].ocenka = smpTable[r].fr_mem / smpTable[r].vars[IndMinC];
                }
                else
                {
                    smpTable[r].ocenka = double.MaxValue;
                }
            }
        }

        min = smpTable[0].ocenka;

        for (int r = 0; r < Method.Subjects.Length; r++)
        {
            if (smpTable[r].ocenka < min)
            {
                min = smpTable[r].ocenka;
                IndMinR = r;
            }
        }

        PrintTable2(smpTable, IndMinR, IndMinC, true);

        SimplexTable[] smpTable2 = new SimplexTable[10];
        CopyTable(ref smpTable, ref smpTable2);

        // [cur_row][ cur_col] - [cur_row][aCol] * [aRow][cur_col] / [aRow][aCol]
        for (int r = 0; r < (Method.Subjects.Length + 2); r++)
        {
            if (smpTable[IndMinR].vars[IndMinC] != 0)
            {
                if (r != IndMinR)
                {
                    smpTable2[r].fr_mem = smpTable[r].fr_mem - smpTable[r].vars[IndMinC] * smpTable[IndMinR].fr_mem / smpTable[IndMinR].vars[IndMinC];
                }
                else
                {
                    smpTable2[r].fr_mem = smpTable[r].fr_mem / smpTable[IndMinR].vars[IndMinC];
                }

                for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
                {
                    if (r != IndMinR)
                    {
                        smpTable2[r].vars[c] = smpTable[r].vars[c] - smpTable[r].vars[IndMinC] * smpTable[IndMinR].vars[c] / smpTable[IndMinR].vars[IndMinC];
                    }
                    else
                    {
                        smpTable2[r].vars[c] = smpTable[r].vars[c] / smpTable[IndMinR].vars[IndMinC];
                    }
                }

                if (r == IndMinR)
                {
                    smpTable2[r].baz.index = IndMinC + 1;

                    if (IndMinC <= Method.Variables + Method.Subjects.Length)
                    {
                        smpTable2[r].baz.letter = 'x';
                    }
                    else
                    {
                        smpTable2[r].baz.letter = 'y';
                    }
                }
            }
            else
            { /* выч при разреш = нулю */ };
        }

        // N // кол-во пер-ных // Method.Variables
        // M // кол-во ур-ний  // Method.Subjects.Count


        if (Method.SynthVariables != 0)
        {
            PrintTable2(smpTable2, -1, -1, false);
        }

        double summ = 0;

        for (int c = 0; c < Method.Subjects.Length + Method.Variables + Method.SynthVariables; c++)
        {
            summ += Math.Abs(smpTable2[(Method.Subjects.Length + 1)].vars[c]);
        }

        if (summ == 0 && smpTable2[(Method.Subjects.Length + 1)].fr_mem == 0)
        {
            Method.SynthVariables = 0;
        }

        if (Method.SynthVariables != 0 && tick > 5)
        {
            result = -1;
        }

        summ = 0;

        CopyTable(ref smpTable2, ref smpTable);
    }

    private void CopyTable(ref SimplexTable[] from, ref SimplexTable[] to)
    {
        for (int r = 0; r < 10; r++)
        {
            to[r] = new SimplexTable()
            {
                fr_mem = from[r].fr_mem,
                ocenka = from[r].ocenka,
                baz = new Bazis()
                {
                    letter = from[r].baz.letter,
                    index = from[r].baz.index
                }
            };

            for (int c = 0; c < 16; c++)
            {
                to[r].vars[c] = from[r].vars[c];
            }
        }
    }
}
