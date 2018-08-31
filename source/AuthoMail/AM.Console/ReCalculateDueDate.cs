using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    DateTime estEffectiveReceivedOn = Channel.GetEffectiveEstReceivedOn(docType, Convert.ToDateTime(item.ReceivedOn));
                    var dueDate = Channel.GetDueDate(docType, estEffectiveReceivedOn);
                    DateTime d = dueDate;
                    rSvc.UpdateDueDateAndEftRecOnEST(item.Id, dueDate,estEffectiveReceivedOn);
                }
            }
        }
    }
}
