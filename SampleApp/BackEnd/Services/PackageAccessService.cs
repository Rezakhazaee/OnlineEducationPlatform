using BackEnd.Data;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PackageAccessService
{
    private readonly ApplicationDbContext _context;

    public PackageAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetPackageLevelAsync()
    {
        return await _context.OrganizationSettings
            .Select(x => (int?)x.PackageLevel)
            .FirstOrDefaultAsync() ?? 1;
    }

    public async Task<bool> HasPackageAsync(int requiredPackageLevel)
    {
        var packageLevel = await GetPackageLevelAsync();

        return packageLevel >= requiredPackageLevel;
    }
}