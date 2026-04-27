namespace StajDb.Models;

public class ProductPrice
{
    public int Id { get; set; }//Her ürün fiyatının benzersiz kimliği(1-100,2-200)

    public int ProductId { get; set; }//Ürün fiyatının hangi ürüne ait olduğunun kimliği(1-telefon,2-bilgisayar)(Foreign Key (DB bağlantısı))
    public Product? Product { get; set; }//Navigation Property (objeyi getirir),N-1(Bir ürün fiyatı bir ürüne ait) / 1-N(Bir ürün birden fazla fiyatı olabilir) ilişkisi

    public decimal Price { get; set; }//Ürün fiyatı(100,200),decimal (Virgülden sonra 2 hane)

    public bool IsDiscount { get; set; }//Ürün fiyatı indirimli mi? (true-indirimli,false-normal)

    public DateTime StartDate { get; set; }//Ürün fiyatının geçerli olacağı tarih(Ne zaman geçerli olacak?)
    public DateTime? EndDate { get; set; }//Ürün fiyatının geçerli olmayacağı tarih(Ne zaman geçerli olmayacak?),? = nullable (boş olabilir)

    public int? CreatedByUserId { get; set; }//Ürün fiyatı oluşturan kullanıcının kimliği(1-admin,2-user)(Kim oluşturdum?)
    public StoreUser? CreatedByUser { get; set; }//Ürün fiyatı oluşturan kullanıcının bilgileri(Oluşturan kişinin detay bilgisi)

    public DateTime CreatedAt { get; set; }//Ürün fiyatı oluşturulma tarihi(Ne zaman oluşturuldu?)

    public int? UpdatedByUserId { get; set; }//Ürün fiyatı güncellenen kullanıcının kimliği(1-admin,2-user)(Kim güncelledi?)
    public StoreUser? UpdatedByUser { get; set; }//Ürün fiyatı güncellenen kullanıcının bilgileri(Güncelleyen kişinin detay bilgisi)

    public DateTime UpdatedAt { get; set; }//Son güncelleme tarihi
}
