namespace StajDb.Models;

public class StoreUser
{
    public int Id { get; set; }//Kullanıcının benzersiz kimliği(1-admin,2-user)

    public string UserName { get; set; } = null!;//Kullanıcı adı(admin,user),null (Ben buna kesin değer vereceğim, null gelmeyecek)

    public string PasswordHash { get; set; } = null!;//Kullanıcı şifresinin hash değeri(123456),null (Ben buna kesin değer vereceğim, null gelmeyecek)

    public DateTime CreatedAt { get; set; }//Kullanıcının oluşturulma tarihi(Ne zaman oluşturuldu?)
    public DateTime UpdatedAt { get; set; } //Kullanıcının güncellenme tarihi(Ne zaman güncellendi?)

    public string Email { get; set; } = null!;

    public UserRole Role { get; set; }

    public DateTime? FirstLoginAt { get; set; }

    public DateTime? LastLoginAt { get; set; }
}
