using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

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
                    var docType = docTypeSvc.GetDocTypeById(item.DocTypeId.Value);
                    DateTime estEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(docType, item.ReceivedOn);
                    var dueDate = commonSvc.GetDueDate(docType, estEffectiveReceivedOn);
                    rSvc.UpdateDueDateAndEftRecOnEST(item.Id, dueDate, estEffectiveReceivedOn);
                }
            }
        }

        public static void UpdateErrorEffectiveReceivedOn()
        {
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
                    }
                    else
                    {
                        var docType = docTypeSvc.GetDocTypeById(item.DocTypeId.Value);
                        DateTime estEffectiveReceivedOn = commonSvc.GetEffectiveEstReceivedOn(docType, item.ReceivedOn);
                        rSvc.UpdateEftRecOnEST(item.Id, estEffectiveReceivedOn);
                    }
                }
            }
        }
    }
}
