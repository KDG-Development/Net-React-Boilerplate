namespace KDG.Boilerplate.Models.DTO;

public class CalculationRequest
{
    public double Number1 { get; set; }
    public double Number2 { get; set; }
}

public class ComplexCalculationRequest
{
    public double[] Numbers { get; set; } = [];
    public string Operation { get; set; } = string.Empty;
    public int Iterations { get; set; }
}

public class CalculationHistory
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public double Input1 { get; set; }
    public double Input2 { get; set; }
    public double Result { get; set; }
    public DateTime Timestamp { get; set; }
}
