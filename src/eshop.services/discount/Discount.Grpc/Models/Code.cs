namespace Discount.Grpc.Models;

public class Code
{
    public int Id { get; set; }
    
    public string Rules { get; set; } = string.Empty;
    
    public double Amount { get; set; }
    
    public double Percentage { get; set; }
}
