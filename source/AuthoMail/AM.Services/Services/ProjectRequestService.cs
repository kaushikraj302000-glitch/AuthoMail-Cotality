using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IProjectRequestService : IDisposable
    {
        List<ProjectRequestLookupForm> SchemaProjectRequestLookupForm();
    }
    public class ProjectRequestService : ServiceBase, IProjectRequestService
    {

        public List<ProjectRequestLookupForm> SchemaProjectRequestLookupForm()
        {
            var orderDictionary = new Dictionary<int, ProjectRequestLookupForm>();
            string query = @"select * from AM.ProjectRequestLookupForm p 
inner join AM.ProjectRequestLookupFormValue v on p.Id=v.ProjectRequestLookupFormId";
            var list = db.Query<ProjectRequestLookupForm, ProjectRequestLookupFormValue, ProjectRequestLookupForm>(
                query,
                (p, v) =>
                {
                    ProjectRequestLookupForm prlf;
                    if (!orderDictionary.TryGetValue(p.Id, out prlf))
                    {
                        prlf = p;
                        prlf.ProjectRequestLookupFormValueList = new List<ProjectRequestLookupFormValue>();
                        orderDictionary.Add(prlf.Id, prlf);
                    }
                    prlf.ProjectRequestLookupFormValueList.Add(v);
                    return prlf;
                }
                ).Distinct().ToList();
            return list;
        }
    }
}
