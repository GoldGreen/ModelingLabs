using System.Collections.Generic;

namespace Simplex.Models;

public class SimplexTable2
{
    public List<Equation> Equations;
    public Equation ObjFunctions;
}

public class Equation
{
    public int Basis;               // Базис x{0}
    public double[] Coefficients;   // Коэффициенты x1, x2, ... , x{n}
    public double Solution;         // Базисное решение
}
