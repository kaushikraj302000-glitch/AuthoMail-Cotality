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
        KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest, List<ProjectRequestValueMapping> values);
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


        public KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest, List<ProjectRequestValueMapping> values)
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

                        values.ForEach(v => v.ProjectRequestId = id);

                        string sql = "INSERT INTO AM.ProjectRequestValueMapping (ProjectRequestId ,ProjectRequestLookupFormValueId ,Value) VALUES (@ProjectRequestId ,@ProjectRequestLookupFormValueId ,@Value)";
                        db.Execute(sql, values);
                    }
                    else
                    {
                        string query = @"UPDATE AM.ProjectRequest SET ClientName = @ClientName ,SubmittedOn = @SubmittedOn ,SubmittedBy = @SubmittedBy ,UATStartOn = @UATStartOn ,ProdStartOn = @ProdStartOn ,Description = @Description ,ESTSweepTime = @ESTSweepTime ,TransmittedTime = @TransmittedTime ,EstimatedPagesPerPackage = @EstimatedPagesPerPackage ,EstimatedPackagesDaily = @EstimatedPackagesDaily ,EstimatedPackagesMonthly = @EstimatedPackagesMonthly ,EstimatedPackagesAnnually = @EstimatedPackagesAnnually ,IsBRERequired = @IsBRERequired ,IsCustomerSupplyEnvelope = @IsCustomerSupplyEnvelope ,IsCustomerProvideArtworkEnvelope = @IsCustomerProvideArtworkEnvelope ,IsClientPostageAccount = @IsClientPostageAccount ,IsScanInAuthoMailDallas = @IsScanInAuthoMailDallas ,AdditionalInfo = @AdditionalInfo WHERE Id=@Id";
                        db.Execute(query, projectRequest);

                        db.Execute("delete from AM.ProjectRequestValueMapping where ProjectRequestId=@ProjectRequestId", new { ProjectRequestId = projectRequest.Id });
                        values.ForEach(v => v.ProjectRequestId = projectRequest.Id);

                        string sql = "INSERT INTO AM.ProjectRequestValueMapping (ProjectRequestId ,ProjectRequestLookupFormValueId ,Value) VALUES (@ProjectRequestId ,@ProjectRequestLookupFormValueId ,@Value)";
                        db.Execute(sql, values);
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
    }
}
