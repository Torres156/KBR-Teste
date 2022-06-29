namespace KBR.Models
{
    public class SimulateModel 
    {   
        public int          QuotaCount { get; set; }
        public double       Value      { get; set; }
        public double       Percentage { get; set; }
        public List<double> QuotaValue { get; set; } = new List<double>();
    }
}
