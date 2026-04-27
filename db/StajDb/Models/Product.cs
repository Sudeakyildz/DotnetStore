namespace StajDb.Models;

public class Product
{
    public int Id { get; set; }//Her ürünün benzersiz kimliği(1-telefon,2-bilgisayar)

    public int CategoryId { get; set; }//Ürünün hangi kategoriye ait olduğunun kimliği(1-elektronik,2-giyim)(Foreign Key (DB bağlantısı))
    public Category? Category { get; set; }//Navigation Property (objeyi getirir),N-1(Bir ürün bir kategoriye ait) / 1-N(Bir kategori birden fazla ürün içerebilir) ilişkisi

    public string Name { get; set; } = null!;//Ürün adı zorunlu

    public string? Description { get; set; }//Ürün açıklaması,null olabilir

    public int Stock { get; set; }//Ürün stok miktarı

    public ProductStatus Status { get; set; } = ProductStatus.Active;//Ürün durumu(Aktif,Pasif,Silinmiş)

    public string? ImageUrl { get; set; }//Ürün resmi url'si(/images/phone.png)

    public bool IsDeleted { get; set; }//Ürün silinmiş mi? (true → silinmiş (DB’de durur))

    public int? CreatedByUserId { get; set; }//Ürün oluşturan kullanıcının kimliği(1-admin,2-user)(Kim oluşturdum?)
    public StoreUser? CreatedByUser { get; set; }//Ürün oluşturan kullanıcının bilgileri(Oluşturan kişinin detay bilgisi)

    public DateTime CreatedAt { get; set; }//Ürün oluşturulma tarihi(Ne zaman oluşturuldu?)

    public int? UpdatedByUserId { get; set; }//Ürün güncellenen kullanıcının kimliği(1-admin,2-user)(Kim güncelledi?)
    public StoreUser? UpdatedByUser { get; set; }//Ürün güncellenen kullanıcının bilgileri(Güncelleyen kişinin detay bilgisi)

    public DateTime UpdatedAt { get; set; }//Son güncelleme tarihi

    public ICollection<ProductFeatureValue> FeatureValues { get; set; } = new List<ProductFeatureValue>();//Bir ürün birden fazla özellik değeri olabilir(1 → N ilişki)
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();//Bir ürün birden fazla fiyatı olabilir(1 → N ilişki)
}
