using System.ComponentModel;
using AdaptiveRag.Data;
using Microsoft.EntityFrameworkCore;

namespace AdaptiveRag.Api.Ai;

public record MaterialInfoResult(
    bool Found,
    int MaterialCode,
    string? Title,
    string? SubjectArea,
    string? IndexingStatus,
    int TestsCount);

public class PlatformTools(AdaptiveRagDbContext db)
{
    public List<string> Calls { get; } = new();

    [Description("Returns title, subject area, indexing status, and number of tests for a study material by its numeric ID.")]
    public async Task<MaterialInfoResult> GetMaterialInfo(
        [Description("Positive integer ID of the study material, e.g. 101")] int materialId)
    {
        Calls.Add($"get_material_info({materialId})");
        if (materialId <= 0)
            return new(false, materialId, null, null, null, 0);

        var materials = await db.CourseMaterials
            .AsNoTracking()
            .Include(m => m.Tests)
            .OrderBy(m => m.Title)
            .ToListAsync();

        int index = materialId - 101;
        if (index < 0 || index >= materials.Count)
            return new(false, materialId, null, null, null, 0);

        var item = materials[index];
        return new(true, materialId, item.Title, item.SubjectArea, item.IndexingStatus, item.Tests.Count);
    }
}