using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.IO;

namespace AM.Console
{
    public class ReCalculateDueDate
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Start()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            {
                var docTypeList = rSvc.GetDocTypeTest();
                foreach (var item in docTypeList)
                {
                    var docType = docTypeSvc.GetDocTypeById(item.DocTypeId.Value, 0);
                    DateTime estEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(docType, item.ReceivedOn);
                    var dueDate = commonSvc.GetDueDate(docType, estEffectiveReceivedOn);
                    rSvc.UpdateDueDateAndEftRecOnEST(item.Id, dueDate, estEffectiveReceivedOn);
                }
            }
        }

        public static void UpdateErrorEffectiveReceivedOn()
        {
            //StringBuilder stringBuilder = new StringBuilder(string.Join(",", "Id", "ReceivedOn", "DueDate", "EffectiveReceivedOn", "DocTypeId", "ClientId"));
            //stringBuilder.AppendLine();
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            {
                var docTypeList = rSvc.GetDocTypeTestWithErrorMP();
                foreach (var item in docTypeList)
                {
                    if (!item.DocTypeId.HasValue)
                    {
                        //NOTE : If System didn't find any doctypeid for particular mail piece then it will take default max cut-off time of that particular client for calculating effective received on date.                        
                        DateTime estMaxEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(docTypeSvc.GetDocTypeWithMaximumCutOffTimeByClientId(item.ClientId), item.ReceivedOn);
                        rSvc.UpdateEftRecOnEST(item.Id, estMaxEffectiveReceivedOn);
                        //item.EffectiveReceivedOnEst = estMaxEffectiveReceivedOn;
                    }
                    else
                    {
                        var docType = docTypeSvc.GetDocTypeById(item.DocTypeId.Value, 0);
                        DateTime estEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(docType, item.ReceivedOn);
                        rSvc.UpdateEftRecOnEST(item.Id, estEffectiveReceivedOn);
                        //item.EffectiveReceivedOnEst = estEffectiveReceivedOn;
                    }
                    //stringBuilder.AppendLine(String.Join(",", item.Id, item.ReceivedOn, item.DueDate, item.EffectiveReceivedOnEst, item.DocTypeId, item.ClientId));
                }

                //System.IO.File.WriteAllText(@"C:\Users\sagar.g\Desktop\Reports MC\Calculation.csv", stringBuilder.ToString());
            }
        }

        public static void UpdatePnSConfigDetailId()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            {

                var listofMpToUpdate = rSvc.GetMPsToUpdatePnSConfigDetailId();
                var columnName = String.Join(",", "MPID", "DocTypeID", "PnSConfigDetailId");
                var errcolumnName = String.Join(",", "MPID", "ClientId", "ClientDocTypeId", "DocTypeID", "ShippingCarrier", "ShippingServiceType", "TotalPages");

                StringBuilder deliveryReport = new StringBuilder(columnName);
                StringBuilder errorReport = new StringBuilder(errcolumnName);

                deliveryReport.AppendLine();
                errorReport.AppendLine();
                foreach (var mp in listofMpToUpdate)
                {
                    DocTypeInfo doctype = new DocTypeInfo(), docTypeInfo = new DocTypeInfo();
                    try
                    {
                        doctype = rSvc.GetDocTypeById(mp.DocTypeId.Value);

                        docTypeInfo = rSvc.GetPnSConfigDetailId(mp.ClientId, mp.DocTypeId.Value, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages == 0 ? 1 : mp.TotalPages, mp.EffectiveReceivedOnEst.Value);

                        deliveryReport.AppendLine(String.Join(",", mp.Id, mp.DocTypeId, docTypeInfo.PnSConfigDetailId));

                        //rSvc.UpdatePnSConfigDetailId(mp.Id, docTypeInfo.PnSConfigDetailId);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured in ClientId MailPieceId :{0} For DocumentType - {1} - Shipping Carrier {2} - ShippingService {3} - Total Pages {4} With Error {5}".Fill(mp.Id, doctype.Id, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages, ex));
                        errorReport.AppendLine(String.Join(",", mp.Id, mp.ClientId, mp.ClientDocTypeId, mp.DocTypeId, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages));
                    }
                }
                File.WriteAllText(@"D:\PnSIds.csv", deliveryReport.ToString());
                File.WriteAllText(@"D:\ErrorPnSIds.csv", errorReport.ToString());

            }
        }
    }
}
