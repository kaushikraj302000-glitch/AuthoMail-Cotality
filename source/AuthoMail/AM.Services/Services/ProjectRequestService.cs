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
        KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest);
        ProjectRequest ProjectRequestFindById(int Id);
        List<ProjectRequest> GetAllProjectRequest();
    }
    public class ProjectRequestService : ServiceBase, IProjectRequestService
    {

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
                        string query = @"INSERT INTO [AM].[ProjectRequest] ([ClientName] ,[SubmittedOn] ,[SubmittedBy] ,[UATStartOn] ,[ProdStartOn] ,[Description] ,[FileTransmittedVia] ,[NoOfEstSweepTimes] ,[ESTSweepTime] ,[TypeOfFiles] ,[TransmittedTime] ,[ServiceLevel] ,[EstimatedPagesPerPackage] ,[EstimatedPackagesDaily] ,[EstimatedPackagesMonthly] ,[EstimatedPackagesAnnually] ,[DailyPrintSchedule] ,[PaperPrintRequirements] ,[PackagesSentToCustomer] ,[IsSampleEnvelopeAttached] ,[Envelopes] ,[IsBRERequired] ,[BusinessReplyMailEnvelope] ,[IsCustomerSupplyEnvelope] ,[IsCustomerProvideArtworkEnvelope] ,[IsClientPostageAccount] ,[IsScanInAuthoMailDallas] ,[AdditionalInfo] ,[CreatedOn] ,[CreatedBy] ,[ModifiedOn] ,[ModifiedBy]) VALUES (@ClientName  ,@SubmittedOn  ,@SubmittedBy  ,@UATStartOn  ,@ProdStartOn  ,@Description  ,@FileTransmittedVia ,@NoOfEstSweepTimes ,@ESTSweepTime ,@TypeOfFiles  ,@TransmittedTime ,@ServiceLevel ,@EstimatedPagesPerPackage ,@EstimatedPackagesDaily  ,@EstimatedPackagesMonthly ,@EstimatedPackagesAnnually ,@DailyPrintSchedule ,@PaperPrintRequirements  ,@PackagesSentToCustomer  ,@IsSampleEnvelopeAttached ,@Envelopes    ,@IsBRERequired ,@BusinessReplyMailEnvelope ,@IsCustomerSupplyEnvelope ,@IsCustomerProvideArtworkEnvelope ,@IsClientPostageAccount  ,@IsScanInAuthoMailDallas ,@AdditionalInfo ,@CreatedOn    ,@CreatedBy  ,@ModifiedOn ,@ModifiedBy)";

                        db.Execute(query, projectRequest);
                    }
                    else
                    {
                        string query = @"UPDATE [AM].[ProjectRequest] SET [ClientName] = @ClientName ,[SubmittedOn] = @SubmittedOn ,[SubmittedBy] = @SubmittedBy ,[UATStartOn] = @UATStartOn ,[ProdStartOn] = @ProdStartOn ,[Description] = @Description ,[FileTransmittedVia] = @FileTransmittedVia ,[NoOfEstSweepTimes] = @NoOfEstSweepTimes ,[ESTSweepTime] = @ESTSweepTime ,[TypeOfFiles] = @TypeOfFiles ,[TransmittedTime] = @TransmittedTime ,[ServiceLevel] = @ServiceLevel ,[EstimatedPagesPerPackage] = @EstimatedPagesPerPackage ,[EstimatedPackagesDaily] = @EstimatedPackagesDaily ,[EstimatedPackagesMonthly] = @EstimatedPackagesMonthly ,[EstimatedPackagesAnnually] = @EstimatedPackagesAnnually ,[DailyPrintSchedule] = @DailyPrintSchedule ,[PaperPrintRequirements] = @PaperPrintRequirements ,[PackagesSentToCustomer] = @PackagesSentToCustomer ,[IsSampleEnvelopeAttached] = @IsSampleEnvelopeAttached ,[Envelopes] = @Envelopes ,[IsBRERequired] = @IsBRERequired ,[BusinessReplyMailEnvelope] = @BusinessReplyMailEnvelope ,[IsCustomerSupplyEnvelope] = @IsCustomerSupplyEnvelope ,[IsCustomerProvideArtworkEnvelope] = @IsCustomerProvideArtworkEnvelope ,[IsClientPostageAccount] = @IsClientPostageAccount ,[IsScanInAuthoMailDallas] = @IsScanInAuthoMailDallas ,[AdditionalInfo] = @AdditionalInfo ,[ModifiedOn] = @ModifiedOn ,[ModifiedBy] = @ModifiedBy  WHERE Id=@Id ";
                        db.Execute(query, projectRequest);
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
            return db.QuerySingle<ProjectRequest>("select * from AM.ProjectRequest where p.Id=@Id", new { Id = Id });
        }



        public List<ProjectRequest> GetAllProjectRequest()
        {
            return db.Query<ProjectRequest>("select * from AM.ProjectRequest").ToList();
        }
    }
}
