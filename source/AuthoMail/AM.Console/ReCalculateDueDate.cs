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
            {
                var docTypeList = rSvc.GetDocTypeTest();
                foreach (var item in docTypeList)
                {
                    var docType = docTypeSvc.GetDocTypeById(item.DocTypeId);
                    DateTime estEffectiveReceivedOn = Channel.GetEffectiveEstReceivedOn(docType, item.ReceivedOn);
                    var dueDate = Channel.GetDueDate(docType, estEffectiveReceivedOn);                    
                    rSvc.UpdateDueDateAndEftRecOnEST(item.Id, dueDate,estEffectiveReceivedOn.EstToUtc());
                }
            }
        }
    }
}
