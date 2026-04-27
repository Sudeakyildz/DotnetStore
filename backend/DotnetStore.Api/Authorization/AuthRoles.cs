using StajDb.Models;

namespace DotnetStore.Api.Authorization;

public static class AuthRoles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string StaffPrices = nameof(UserRole.StaffPrices);
    public const string StaffFeatures = nameof(UserRole.StaffFeatures);
    public const string StaffCategories = nameof(UserRole.StaffCategories);
    public const string Musteri = nameof(UserRole.Musteri);

    public const string AdminOrStaffPrices = Admin + "," + StaffPrices;
    public const string AdminOrStaffFeatures = Admin + "," + StaffFeatures;
    public const string AdminOrStaffCategories = Admin + "," + StaffCategories;

    public const string CatalogReaders =
        Admin + "," + StaffPrices + "," + StaffFeatures + "," + StaffCategories;
}
