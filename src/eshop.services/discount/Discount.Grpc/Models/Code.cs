using System.Text.Json;

namespace Discount.Grpc.Models;

public class Code
{
    public int Id { get; set; }

    public double Amount { get; set; }

    public double Percentage { get; set; }
}