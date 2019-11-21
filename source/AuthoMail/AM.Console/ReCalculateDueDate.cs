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

                StringBuilder deliveryReport = new StringBuilder(columnName);

                deliveryReport.AppendLine();
                foreach (var mp in listofMpToUpdate)
                {
                    var doctype = rSvc.GetDocTypeById(mp.DocTypeId.Value);

                    var docTypeInfo = rSvc.GetPnSConfigDetailId(mp.ClientId, mp.DocTypeId.Value, doctype.ShippingCarrier, doctype.ShippingServiceType, mp.TotalPages == 0 ? 1 : mp.TotalPages);
                    
                    deliveryReport.AppendLine(String.Join(",", mp.Id, mp.DocTypeId, docTypeInfo.PnSConfigDetailId));

                    rSvc.UpdatePnSConfigDetailId(mp.Id, docTypeInfo.PnSConfigDetailId);
                }
                File.WriteAllText(@"E:\Git\PnSIds.csv", deliveryReport.ToString());
            }
        }
    }
}
