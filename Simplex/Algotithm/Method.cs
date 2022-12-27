namespace Labs.Simplex.Algotithm;

public class Method
{
    public int Variables { get; set; }

    public int SynthVariables { get; set; }

    public Extremum Function { get; set; }

    public Subject[] Subjects { get; set; }

    public double[] ObjectiveVariables { get; set; }
}
