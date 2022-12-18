using System;
using System.Collections.Generic;
using System.Linq;

namespace Labs.Simplex;
internal class SimplexSolver3
{
    public const double Eps = 1e-9;

    public SimplexMethodResult Solve(double[,] A, double[] b, double[] c)
    {
        var n = A.GetLength(1);
        var m = A.GetLength(0);

        var ANew = new double[m, n + m];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                ANew[i, j] = A[i, j];
            }
        }
        A = ANew;
        var cNew = new double[n + m];
        for (int i = 0; i < n; i++)
        {
            cNew[i] = c[i];
        }
        c = cNew;
        n = n + m;

        var N = new HashSet<int>();
        for (int i = 0; i < n - m; i++)
        {
            N.Add(i);
        }
        var B = new HashSet<int>();
        var id = new int[n + 1];
        for (int i = n - m; i < n; i++)
        {
            B.Add(i);
            id[i] = i - (n - m);
        }

        int l;
        int e;
        double v = 0;
        if (!b.All(value => value > -Eps))
        {
            ANew = new double[m, n + 1];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    ANew[i, j] = A[i, j];
                }
                ANew[i, n] = -1;
            }
            A = ANew;

            cNew = new double[n + 1];
            cNew[n] = -1;

            N.Add(n);

            e = n;
            l = -1;
            foreach (var i in B)
            {
                if (b[id[i]] < -Eps && (l == -1 || b[id[l]] > b[id[i]]))
                {
                    l = i;
                }
            }

            v = 0;
            var result = Pivot(N, B, A, id, b, cNew, ref v, l, e);
            A = result.A;
            b = result.b;
            cNew = result.c;

            while (N.Any(i => cNew[i] > Eps))
            {
                e = -1;
                foreach (var i in N)
                {
                    if (cNew[i] > Eps)
                    {
                        e = i;
                        break;
                    }
                }

                l = -1;
                foreach (var i in B)
                {
                    if (A[id[i], e] > Eps && (l == -1 || b[id[l]] / A[id[l], e] > b[id[i]] / A[id[i], e]))
                    {
                        l = i;
                    }
                }

                if (l == -1)
                {
                    return new SimplexMethodResult { Status = SimplexMethodResultStatus.Unlimited };
                }
                result = Pivot(N, B, A, id, b, cNew, ref v, l, e);
                A = result.A;
                b = result.b;
                cNew = result.c;
            }

            if (v < -Eps)
            {
                return new SimplexMethodResult { Status = SimplexMethodResultStatus.NoSolution };
            }

            if (B.Contains(n))
            {
                B.Remove(n);
                e = -1;
                foreach (var j in N)
                {
                    if (Math.Abs(A[id[n], j]) > Eps)
                    {
                        e = j;
                        break;
                    }
                }
                var d = A[id[n], e];
                for (int j = 0; j < n; j++)
                {
                    A[id[n], j] /= d;
                }
                A[id[n], e] = 0;
                foreach (var i in B)
                {
                    d = -A[id[i], e];
                    for (int j = 0; j < n; j++)
                    {
                        A[id[i], j] += d * A[id[n], j];
                    }
                    A[id[i], e] = 0;
                }
                N.Remove(e);
                B.Add(e);
                id[e] = id[n];
            }
            else if (N.Contains(n))
            {
                N.Remove(n);
            }

            cNew = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (!B.Contains(i))
                {
                    cNew[i] += c[i];
                }
                else
                {
                    for (int j = 0; j < n; j++)
                    {
                        cNew[j] -= c[i] * A[id[i], j];
                    }
                    v += c[i] * b[id[i]];
                }
            }
            c = cNew;

            ANew = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    ANew[i, j] = A[i, j];
                }
            }
            A = ANew;
        }

        while (N.Any(i => c[i] > Eps))
        {
            e = -1;
            foreach (var i in N)
            {
                if (c[i] > Eps)
                {
                    e = i;
                    break;
                }
            }

            l = -1;
            foreach (var i in B)
            {
                if (A[id[i], e] > Eps && (l == -1 || b[id[l]] / A[id[l], e] > b[id[i]] / A[id[i], e]))
                {
                    l = i;
                }
            }

            if (l == -1)
            {
                return new SimplexMethodResult { Status = SimplexMethodResultStatus.Unlimited };
            }
            var result = Pivot(N, B, A, id, b, c, ref v, l, e);
            A = result.A;
            b = result.b;
            c = result.c;
        }

        var x = new double[n - m];
        for (int i = 0; i < n - m; i++)
        {
            if (B.Contains(i))
            {
                x[i] = b[id[i]];
            }
        }

        return new SimplexMethodResult { Status = SimplexMethodResultStatus.Ok, Value = v, Solution = x };
    }

    private PivotResult Pivot(ISet<int> N, ISet<int> B, double[,] A, IList<int> id, IList<double> b, IList<double> c, ref double v, int l, int e)
    {
        var ANew = new double[A.GetLength(0), A.GetLength(1)];
        var bNew = new double[b.Count];
        var cNew = new double[c.Count];

        id[e] = id[l];

        bNew[id[e]] = b[id[l]] / A[id[l], e];

        foreach (var j in N)
        {
            if (j != e)
            {
                ANew[id[e], j] = A[id[l], j] / A[id[l], e];
            }
        }
        ANew[id[e], l] = 1 / A[id[l], e];

        foreach (var i in B)
        {
            if (i != l)
            {
                bNew[id[i]] = b[id[i]] - A[id[i], e] * bNew[id[e]];
                foreach (var j in N)
                {
                    if (j != e)
                    {
                        ANew[id[i], j] = A[id[i], j] - A[id[i], e] * ANew[id[e], j];
                    }
                }
                ANew[id[i], l] = -A[id[i], e] * ANew[id[e], l];
            }
        }

        v += c[e] * bNew[id[e]];
        foreach (var j in N)
        {
            if (j != e)
            {
                cNew[j] = c[j] - c[e] * ANew[id[e], j];
            }
        }
        cNew[l] = -c[e] * ANew[id[e], l];

        N.Remove(e);
        N.Add(l);
        B.Remove(l);
        B.Add(e);

        return new PivotResult { A = ANew, c = cNew, b = bNew };
    }
}

public class SimplexMethodResult
{
    public SimplexMethodResultStatus Status { get; set; }

    public double Value { get; set; }

    public double[] Solution { get; set; }
}

public enum SimplexMethodResultStatus
{
    Ok,
    NoSolution,
    Unlimited
}

internal class PivotResult
{
    public double[,] A { get; set; }

    public double[] c { get; set; }

    public double[] b { get; set; }
}
