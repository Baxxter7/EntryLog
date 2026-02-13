namespace EntryLog.Entities.POCOEntities;

public class FaceID
{
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime RegisterDate {  get; set; }
    public List<float> Descriptor { get; set; } = [];
    public bool Active { get; set; }
}
