namespace BLL.ViewModels.Bid;

public class BidVM
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FreelancerId { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}