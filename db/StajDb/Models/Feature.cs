namespace StajDb.Models;

public class Feature
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public FeatureDataType DataType { get; set; } = FeatureDataType.String;

    public bool IsDeleted { get; set; }

    public int? CreatedByUserId { get; set; }
    public StoreUser? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }
    public StoreUser? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductFeatureValue> Values { get; set; } = new List<ProductFeatureValue>();
}
