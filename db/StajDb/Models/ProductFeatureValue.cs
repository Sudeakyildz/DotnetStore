namespace StajDb.Models;

public class ProductFeatureValue
{
    public int Id { get; set; }//Her ürün özelliğinin benzersiz kimliği(1-siyah,2-kırmızı)

    public int ProductId { get; set; }//Ürün özelliğinin hangi ürüne ait olduğunun kimliği(1-telefon,2-bilgisayar)(Foreign Key (DB bağlantısı))
    public Product? Product { get; set; }//Navigation Property (objeyi getirir),N-1(Bir ürün özelliği bir ürüne ait) / 1-N(Bir ürün birden fazla özelliği olabilir) ilişkisi

    public int FeatureId { get; set; }//Ürün özelliğinin hangi özelliğe ait olduğunun kimliği(1-renk,2-boyut)(Foreign Key (DB bağlantısı))
    public Feature? Feature { get; set; }//Navigation Property (objeyi getirir),N-1(Bir ürün özelliği bir özelliğe ait) / 1-N(Bir özellik birden fazla ürün özelliği olabilir) ilişkisi

    public string Value { get; set; } = null!;//Ürün özelliğinin değeri(siyah,kırmızı),null (Ben buna kesin değer vereceğim, null gelmeyecek)

    public int? CreatedByUserId { get; set; }//Ürün özelliği oluşturan kullanıcının kimliği(1-admin,2-user)(Kim oluşturdum?)
    public StoreUser? CreatedByUser { get; set; }//Ürün özelliği oluşturan kullanıcının bilgileri(Oluşturan kişinin detay bilgisi)

    public DateTime CreatedAt { get; set; }//Ürün özelliği oluşturulma tarihi(Ne zaman oluşturuldu?)

    public int? UpdatedByUserId { get; set; }//Ürün özelliği güncellenen kullanıcının kimliği(1-admin,2-user)(Kim güncelledi?)
    public StoreUser? UpdatedByUser { get; set; }//Ürün özelliği güncellenen kullanıcının bilgileri(Güncelleyen kişinin detay bilgisi)

    public DateTime UpdatedAt { get; set; }//Son güncelleme tarihi
}
