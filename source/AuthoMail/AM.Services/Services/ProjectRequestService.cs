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
        KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest);
        ProjectRequest ProjectRequestFindById(int Id);
    }
    public class ProjectRequestService : ServiceBase, IProjectRequestService
    {

        public List<ProjectRequestLookupForm> SchemaProjectRequestLookupForm()
        {
            var orderDictionary = new Dictionary<int, ProjectRequestLookupForm>();
            string query = @"select * from AM.ProjectRequestLookupForm p 
  inner join AM.ProjectRequestFormHeaders h on p.ProjectRequestFormHeadersHeaderCode=h.HeaderCode
inner join AM.ProjectRequestLookupFormValue v on p.Id=v.ProjectRequestLookupFormId";
            var list = db.Query<ProjectRequestLookupForm, ProjectRequestFormHeaders, ProjectRequestLookupFormValue, ProjectRequestLookupForm>(
                query,
                (p, h, v) =>
                {
                    p.ProjectRequestFormHeaders = h;
                    ProjectRequestLookupForm prlf;
                    if (!orderDictionary.TryGetValue(p.Id, out prlf))
                    {
                        prlf = p;
                        prlf.ProjectRequestLookupFormValueList = new List<ProjectRequestLookupFormValue>();
                        orderDictionary.Add(prlf.Id, prlf);
                    }
                    prlf.ProjectRequestLookupFormValueList.Add(v);
                    return prlf;
                }, splitOn: "ProjectRequestFormHeadersHeaderCode,Id"
                ).Distinct().OrderBy(h => h.ProjectRequestFormHeaders.AlphaSeqCode).ToList();
            return list;
        }


        public KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest)
        {
            //Validation
            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    if (projectRequest.Id != 0)
                    {
                        string query = @"INSERT INTO AM.ProjectRequest (ClientName ,SubmittedOn ,SubmittedBy ,UATStartOn ,ProdStartOn ,Description ,ESTSweepTime ,TransmittedTime ,EstimatedPagesPerPackage ,EstimatedPackagesDaily ,EstimatedPackagesMonthly ,EstimatedPackagesAnnually ,IsBRERequired ,IsCustomerSupplyEnvelope ,IsCustomerProvideArtworkEnvelope ,IsClientPostageAccount ,IsScanInAuthoMailDallas ,AdditionalInfo)      VALUES (@ClientName ,@SubmittedOn,@SubmittedBy,@UATStartOn,@ProdStartOn,@Description,@ESTSweepTime,@TransmittedTime,@EstimatedPagesPerPackage,@EstimatedPackagesDaily,@EstimatedPackagesMonthly,@EstimatedPackagesAnnually,@IsBRERequired ,@IsCustomerSupplyEnvelope,@IsCustomerProvideArtworkEnvelope,@IsClientPostageAccount,@IsScanInAuthoMailDallas ,@AdditionalInfo);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                        int id = db.Query<int>(query, projectRequest).Single();
                        if (projectRequest.ProjectRequestValueMappingList != null)
                        {
                            projectRequest.ProjectRequestValueMappingList.ForEach(v => v.ProjectRequestId = id);

                            string sql = "INSERT INTO AM.ProjectRequestValueMapping (ProjectRequestId ,ProjectRequestLookupFormValueId ,Value) VALUES (@ProjectRequestId ,@ProjectRequestLookupFormValueId ,@Value)";
                            db.Execute(sql, projectRequest.ProjectRequestValueMappingList);
                        }
                    }
                    else
                    {
                        string query = @"UPDATE AM.ProjectRequest SET ClientName = @ClientName ,SubmittedOn = @SubmittedOn ,SubmittedBy = @SubmittedBy ,UATStartOn = @UATStartOn ,ProdStartOn = @ProdStartOn ,Description = @Description ,ESTSweepTime = @ESTSweepTime ,TransmittedTime = @TransmittedTime ,EstimatedPagesPerPackage = @EstimatedPagesPerPackage ,EstimatedPackagesDaily = @EstimatedPackagesDaily ,EstimatedPackagesMonthly = @EstimatedPackagesMonthly ,EstimatedPackagesAnnually = @EstimatedPackagesAnnually ,IsBRERequired = @IsBRERequired ,IsCustomerSupplyEnvelope = @IsCustomerSupplyEnvelope ,IsCustomerProvideArtworkEnvelope = @IsCustomerProvideArtworkEnvelope ,IsClientPostageAccount = @IsClientPostageAccount ,IsScanInAuthoMailDallas = @IsScanInAuthoMailDallas ,AdditionalInfo = @AdditionalInfo WHERE Id=@Id";
                        db.Execute(query, projectRequest);
                        if (projectRequest.ProjectRequestValueMappingList != null)
                        {
                            db.Execute("delete from AM.ProjectRequestValueMapping where ProjectRequestId=@ProjectRequestId", new { ProjectRequestId = projectRequest.Id });

                            projectRequest.ProjectRequestValueMappingList.ForEach(v => v.ProjectRequestId = projectRequest.Id);

                            string sql = "INSERT INTO AM.ProjectRequestValueMapping (ProjectRequestId ,ProjectRequestLookupFormValueId ,Value) VALUES (@ProjectRequestId ,@ProjectRequestLookupFormValueId ,@Value)";
                            db.Execute(sql, projectRequest.ProjectRequestValueMappingList);
                        }
                    }
                    t.Commit();
                    return new KeyValuePair<bool, string>(true, "Saved successful.");
                }
                catch (Exception ex)
                {
                    t.Rollback();
                    return new KeyValuePair<bool, string>(false, ex.Message);
                }
            }
        }


        public ProjectRequest ProjectRequestFindById(int Id)
        {
            string query = @"select * from AM.ProjectRequest p
left outer join AM.ProjectRequestValueMapping v on p.Id=v.ProjectRequestId
where p.Id=@Id";
            var orderDictionary = new Dictionary<int, ProjectRequest>();

            var projectRequest = db.Query<ProjectRequest, ProjectRequestValueMapping, ProjectRequest>(
                query, (p, v) =>
                {
                    ProjectRequest pr;
                    if (!orderDictionary.TryGetValue(p.Id, out pr))
                    {
                        pr = p;
                        pr.ProjectRequestValueMappingList = new List<ProjectRequestValueMapping>();
                        orderDictionary.Add(pr.Id, pr);
                    }
                    pr.ProjectRequestValueMappingList.Add(v);
                    return pr;
                }
                ).Distinct().Single();
            return projectRequest;
        }
    }
}
