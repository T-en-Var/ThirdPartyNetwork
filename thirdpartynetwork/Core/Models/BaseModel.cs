namespace thirdpartynetwork.Core.Models;

public class BaseModel
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
}