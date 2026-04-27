namespace StajDb.Models;

//Ürünün durumunu belirler
public enum ProductStatus
{
    Active = 1,
    Inactive = 2,
    Draft = 3,//Taslak ürün tam hazır değil
}

//Özelliğin veri tipini belirler
public enum FeatureDataType
{
    None = 0,
    String = 1,
    Int = 2,
    Decimal = 3,
    Date = 4,
    Bool = 5,
}

public enum UserRole
{
    Admin = 1,
    StaffPrices = 2,
    StaffFeatures = 3,
    StaffCategories = 4,
    /// <summary>
    /// Mağaza müşterisi (gelecekteki müşteri uygulaması; yalnız bu rol tüketici ekranlarına bağlanır).
    /// </summary>
    Musteri = 5,
}

/// <summary>
/// Müşteriye (User) bağlı satın alma / sepet hattı durumu; admin panelden güncellenir.
/// </summary>
public enum OrderStatus
{
    BegeniyeEklendi = 1,
    Sepette = 2,
    SiparisAlindi = 3,
    KargoyaVerildi = 4,
    TeslimEdildi = 5,
    IptalEdildi = 6,
}
