using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AM.Services;
using ExcelReader;
using System.Data;
using AM.USPSLib;
using System.Linq;
using System.Collections.Generic;

namespace AM.UnitTests
{
    [TestClass]
    public class USPSUnitTest
    {
        [TestMethod]
        public void USPSVerifyAdress()
        {
            XLS excel = new XLS(@"E:\Projects\CoreLogic\AuthoMail\source\AuthoMail\AM.UnitTests\bin\Release\USPSTestData\TestUspsAdd.xlsx");

            var group = excel.GetData().AsEnumerable().Select((x, index) => new { x, index }).GroupBy(x => x.index / 5, y => y.x).Select(s => s.Select(item => new AddressRequest()
            {
                Id = "SVC" + item["Id"].ToString(),
                StreetAddress = item["Address"].ToString(),
                City = item["City"].ToString(),
                State = item["State"].ToString(),
                Zip5 = item["Zip"].ToString()
            })).ToList();

            var sendingGrp = group;
            List<VerifierAddressResponse> vAddResList = new List<VerifierAddressResponse>();

            do
            {
                var vAddRes = ValidateAddress(sendingGrp);
                vAddResList.AddRange(vAddRes);
                sendingGrp = sendingGrp.Skip(vAddRes.Count).ToList();

            } while (sendingGrp.Count > 0);
        }

        private static List<VerifierAddressResponse> ValidateAddress(List<IEnumerable<AddressRequest>> group)
        {
            Random rdm = new Random();
            List<VerifierAddressResponse> res = new List<VerifierAddressResponse>();

            try
            {
                //var svc = RestService.For<IUSPS>(new HttpClient(new AuthenticatedHttpClientHandler(uid, pass)) { BaseAddress = new Uri(endpoint) });

                var svc = new USPSService();

                foreach (var grp in group)
                {
                    VerifierAddressResponse result = svc.VerifyAddressAsync("UnitTest", new VerifierAddressRequest()
                    {
                        AddressRequests = grp.ToList()
                    }).Result;

                    res.Add(result);

                    //Thread.Sleep(rdm.Next(1, 6) * 1000);
                }
            }
            catch (Exception ex)
            {
            }

            return res;
        }
    }
}
