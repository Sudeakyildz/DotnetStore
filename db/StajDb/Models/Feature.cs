namespace StajDb.Models;

public class Feature
{
    public int Id { get; set; }//Her özelliğin benzersiz kimliği(1-renk,2-boyut)

    public string Name { get; set; } = null!;//Özelliğin adı(Renk,Boyut),null (Ben buna kesin değer vereceğim, null gelmeyecek)

    public FeatureDataType DataType { get; set; } = FeatureDataType.String;//Özelliğin veri tipi(String,Int,Decimal,Date,Bool)(Bu özellik ne tip veri tutuyor?)

    public bool IsDeleted { get; set; }//Özellik silinmiş mi? (true → silinmiş (DB’de durur))

    public int? CreatedByUserId { get; set; }//Özellik oluşturan kullanıcının kimliği(1-admin,2-user)(Kim oluşturdum?)
    public StoreUser? CreatedByUser { get; set; }//Özellik oluşturan kullanıcının bilgileri(Oluşturan kişinin detay bilgisi)

    public DateTime CreatedAt { get; set; }//Özellik oluşturulma tarihi(Ne zaman oluşturuldu?)

    public int? UpdatedByUserId { get; set; }//Özellik güncellenen kullanıcının kimliği(1-admin,2-user)(Kim güncelledi?)
    public StoreUser? UpdatedByUser { get; set; }//Özellik güncellenen kullanıcının bilgileri(Güncelleyen kişinin detay bilgisi)

    public DateTime UpdatedAt { get; set; }//Son güncelleme tarihi

    public ICollection<ProductFeatureValue> Values { get; set; } = new List<ProductFeatureValue>();//Bir özellik birden fazla ürün özelliği olabilir(1 → N ilişki)(color=siyah)
}
