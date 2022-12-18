using System;

namespace Labs.Simplex;
internal class SimplexSolver2
{
    private static double EPSILON = 1.0E-8;

    private double[,] a;





    private int m;
    private int n;
    private int[] basis;

    public SimplexSolver2(double[,] A, double[] b, double[] c)
    {
        m = b.Length;
        n = c.Length;
        a = new double[m + 2, n + m + m + 1];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                a[i, j] = A[i, j];
        for (int i = 0; i < m; i++)
            a[i, n + i] = 1.0;
        for (int i = 0; i < m; i++)
            a[i, n + m + m] = b[i];
        for (int j = 0; j < n; j++)
            a[m, j] = c[j];

        for (int i = 0; i < m; i++)
        {
            if (b[i] < 0)
            {
                for (int j = 0; j <= n + m + m; j++)
                    a[i, j] = -a[i, j];
            }
        }

        for (int i = 0; i < m; i++)
            a[i, n + m + i] = 1.0;
        for (int i = 0; i < m; i++)
            a[m + 1, n + m + i] = -1.0;
        for (int i = 0; i < m; i++)
            pivot(i, n + m + i);

        basis = new int[m];
        for (int i = 0; i < m; i++)
            basis[i] = n + m + i;

        phase1();
        phase2();
    }

    private void phase1()
    {
        while (true)
        {

            int q = bland1();
            if (q == -1)
                break;
            int p = minRatioRule(q);

            pivot(p, q);

            basis[p] = q;
        }
        if (a[m + 1, n + m + m] > EPSILON)
            throw new ArithmeticException("Linear program is infeasible");
    }

    private void phase2()
    {
        while (true)
        {

            int q = bland2();
            if (q == -1)
                break;

            int p = minRatioRule(q);
            if (p == -1)
                throw new ArithmeticException("Linear program is unbounded");

            pivot(p, q);

            basis[p] = q;
        }
    }

    private int bland1()
    {
        for (int j = 0; j < n + m; j++)
            if (a[m + 1, j] > EPSILON)
                return j;
        return -1;
    }

    private int bland2()
    {
        for (int j = 0; j < n + m; j++)
            if (a[m, j] > EPSILON)
                return j;
        return -1;
    }


    private int minRatioRule(int q)
    {
        int p = -1;
        for (int i = 0; i < m; i++)
        {
            if (a[i, q] <= EPSILON)
                continue;
            else if (p == -1)
                p = i;
            else if ((a[i, n + m + m] / a[i, q]) < (a[p, n + m + m] / a[p, q]))
                p = i;
        }
        return p;
    }

    private void pivot(int p, int q)
    {

        for (int i = 0; i <= m + 1; i++)
            for (int j = 0; j <= n + m + m; j++)
                if (i != p && j != q)
                    a[i, j] -= a[p, j] * a[i, q] / a[p, q];

        for (int i = 0; i <= m + 1; i++)
            if (i != p)
                a[i, q] = 0.0;

        for (int j = 0; j <= n + m + m; j++)
            if (j != q)
                a[p, j] /= a[p, q];
        a[p, q] = 1.0;
    }

    public double Value()
    {
        return -a[m, n + m + m];
    }

    public double[] Primal()
    {
        double[] x = new double[n];
        for (int i = 0; i < m; i++)
            if (basis[i] < n)
                x[basis[i]] = a[i, n + m + m];
        return x;
    }

    public double[] Dual()
    {
        double[] y = new double[m];
        for (int i = 0; i < m; i++)
            y[i] = -a[m, n + i];
        return y;
    }


    private bool isPrimalFeasible(double[,] A, double[] b)
    {
        double[] x = Primal();

        for (int j = 0; j < x.Length; j++)
        {
            if (x[j] < 0.0)
            {
                return false;
            }
        }

        for (int i = 0; i < m; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
            {
                sum += A[i, j] * x[j];
            }
            if (sum > b[i] + EPSILON)
            {
                return false;
            }
        }
        return true;
    }

    private bool isDualFeasible(double[,] A, double[] c)
    {
        double[] y = Dual();

        for (int i = 0; i < y.Length; i++)
        {
            if (y[i] < 0.0)
            {
                return false;
            }
        }

        for (int j = 0; j < n; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < m; i++)
            {
                sum += A[i, j] * y[i];
            }
            if (sum < c[j] - EPSILON)
            {
                return false;
            }
        }
        return true;
    }

    private bool isOptimal(double[] b, double[] c)
    {
        double[] x = Primal();
        double[] y = Dual();
        double value = Value();

        double value1 = 0.0;
        for (int j = 0; j < x.Length; j++)
            value1 += c[j] * x[j];
        double value2 = 0.0;
        for (int i = 0; i < y.Length; i++)
            value2 += y[i] * b[i];
        if (Math.Abs(value - value1) > EPSILON || Math.Abs(value - value2) > EPSILON)
        {
            return false;
        }

        return true;
    }

    private bool check(double[,] A, double[] b, double[] c)
    {
        return isPrimalFeasible(A, b) && isDualFeasible(A, c) && isOptimal(b, c);
    }
}
