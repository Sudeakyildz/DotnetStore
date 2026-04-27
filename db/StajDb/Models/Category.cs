namespace StajDb.Models;

public class Category
{
    public int Id { get; set; }//Her kategorinin benzersiz kimliği(1-elektronik,2-giyim))

    public string Name { get; set; } = null!;//Kategori adı(Elektronik,Giyim),null (Ben buna kesin değer vereceğim, null gelmeyecek)

    public string? Description { get; set; }//Kategori açıklaması(Elektronik:Akıllı saat,bilgisayar ve aksesuar),? = nullable (boş olabilir)

    public string? ImageUrl { get; set; }//Kategori resmi url'si(/images/electronics.png)

    public string? Slug { get; set; }//URL dostu isim(electronics,home-appliances)(site.com/categories/electronics)

    public bool IsDeleted { get; set; }//Kategori silinmiş mi? (true-silinmiş ama veritabanında duruyor(Buna soft delete denir),false-silinmemiş)

    public int? CreatedByUserId { get; set; }//Kategori oluşturan kullanıcının kimliği(1-admin,2-user)
    public StoreUser? CreatedByUser { get; set; }//Kategori oluşturan kullanıcının bilgileri(Bu ID’nin kendisini getirir,Yani sadece ID değil, kullanıcı bilgisi de gelir)(Ahmet oluşturdu → Ahmet’in bilgisi burada)

    public DateTime CreatedAt { get; set; }//Kategori oluşturulma tarihi(Ne zaman oluşturuldu?)

    public int? UpdatedByUserId { get; set; }//Kategori güncellenen kullanıcının kimliği(1-admin,2-user)(Kim güncelledi?)
    public StoreUser? UpdatedByUser { get; set; }//Kategori güncellenen kullanıcının bilgileri(Güncelleyen kişinin detay bilgisi)

    public DateTime UpdatedAt { get; set; }//Son güncelleme tarihi

    public ICollection<Product> Products { get; set; } = new List<Product>();//Bir kategori içinde birden fazla ürün olabilir(1 → N ilişki)
}
