namespace StajDb.Models;

public enum ProductStatus
{
    Active = 1,
    Inactive = 2,
    Draft = 3,//Taslak ürün tam hazır değil
}

public enum FeatureDataType
{
    None = 0,
    String = 1,
    Int = 2,
    Decimal = 3,
    Date = 4,
    Bool = 5,
}
