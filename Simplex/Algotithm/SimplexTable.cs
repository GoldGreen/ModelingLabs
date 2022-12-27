namespace Simplex.Models;

public class SimplexTable
{
    public SimplexTable()
    {
        vars = new double[16];
        baz = new Bazis();
    }

    public Bazis baz;        // базис
    public double fr_mem;    // своб член
    public double[] vars;    // массив переменных
    public double ocenka;    // оценочное отношение
}

public struct Bazis
{
    public char letter;      // буква
    public int index;        // индекс перемен
};
