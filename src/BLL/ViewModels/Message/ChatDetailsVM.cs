namespace BLL.ViewModels.Message;

public class ChatDetailsVM
{
    public Guid ContractId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public Guid InterlocutorId { get; set; }
    public string InterlocutorName { get; set; } = string.Empty;
    public string? InterlocutorAvatar { get; set; }
    public string ContractStatus { get; set; } = string.Empty;
    public bool IsInterlocutorOnline { get; set; }
    public string InterlocutorRole { get; set; } =  string.Empty;
}
