using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public interface IDiversityPatchService
    {
        string Patch(string diversityJson, DiversityPatch diversityPatch);
    }
}
