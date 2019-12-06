using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Net;
using Refit;
using System.Net.Http;
using AM.UnitTests.AM_API;
using System.IO;
using AM.Entities;

namespace AM.UnitTests
{
    /// <summary>
    /// Summary description for API_UnitTest
    /// </summary>
    [TestClass]
    public class API_UnitTest
    {
        public API_UnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void MRAPI_Success()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "ELLIEMAE",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual(mailPiece.Id, response.UniqueId);
            Assert.AreEqual("Your request is queued for processing.", response.Message);            
        }
        [TestMethod]
        public void MRAPI_InvalidClientCode()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "US",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("InvalidClientCode", response.ErrorCode);
        }
        [TestMethod]
        public void MRAPI_MissingDocument()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "USAA",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    //Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    //IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_PDF_File", response.ErrorCode);
            Assert.AreEqual("Pdf file  is mandatory.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingPdfName()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "ELLIEMAE",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Initial Disclosures",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Missing_PDF_File", response.ErrorCode);
            Assert.AreEqual("Pdf name  is mandatory.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingDocumentPacakgeType()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "ELLIEMAE",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }

            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Invalid_PackageType", response.ErrorCode);
            Assert.AreEqual("Document or package type is not specified.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingRecAddressInfo()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "USAA",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = ""
                    }
                },
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Recipient address is mandatory.", response.ErrorDesc);
        }
        [TestMethod]
        public void MRAPI_MissingRetRecAddressInfo()
        {
            MailPiece mailPiece = new MailPiece()
            {
                Id = Guid.NewGuid(),
                ClientCode = "USAA",
                BusinessFriendlyIdName = "Loan Number",
                BusinessFriendlyIdValue = "123456789",
                DocumentOrPackageType = "Appraisal Report Waiver",
                DocumentOrPackage = new DocumentOrPackage()
                {
                    Name = "202600000030598.pdf",
                    Base64String = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\akshar.vala.mtc\Desktop\03232017\202600000030598.pdf")).ToString(),
                    IncludePatternBaseString = true
                },
                RecipientInfo = new RecipientInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "1363 Henery Street",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                },
                ReturnInfo = new ReturnInfo()
                {
                    Recipient = new Recipient()
                    {
                        FirstName = "Mark",
                        LastName = "Varney"
                    },
                    Address = new Address()
                    {
                        Street = "",
                        City = "Wichita",
                        State = "KS",
                        Zip = "67203"
                    }
                }
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual("Error", response.MailingStatus);
            Assert.AreEqual("Mailing_Address_Missing_Or_Invalid", response.ErrorCode);
            Assert.AreEqual("Return recipient address is mandatory.", response.ErrorDesc);
        }
        
        [TestMethod]
        public void MRAPI_InvalidRequest()
        {
            MailPiece mailPiece = new MailPiece()
            {                
            };
            var response = CreateMailRequest(mailPiece).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void MailResponseAPI_ValidRequest()
        {
            string[] ids = { "ECC7429B-C69D-4ADE-B418-B43E1F53C6BF", "DE03C595-5C6F-4DB0-B484-F22C9AD7C478", "7EAC64B7-2831-4FC2-AEC7-623B3B1BB7FD", "4CE84B05-EBCA-449E-ABD0-3731AD08861C", "deb7ad5f-71d9-46bd-a92f-f3f3366223c1" };

            List<string> mailPieceIds = new List<string>(ids);
            List<MailAPIResponse> response = GenerateMailResponse(mailPieceIds).Result;
            Assert.AreEqual("Error", response[0].MailingStatus);
            Assert.AreEqual("Shipped", response[1].MailingStatus);
            Assert.AreEqual("PulledBack", response[2].MailingStatus);
            Assert.AreEqual("eDelivered", response[3].MailingStatus);
            Assert.AreEqual("NotFound", response[4].MailingStatus);
            Assert.AreEqual("The specified package does not exist.", response[4].Message);
        }

        [TestMethod]
        public Task<MailAPIResponse> CreateMailRequest(MailPiece request)
        {
            string endpoint = "http://localhost:17005";//"https://authomailuat.corelogic.com";
            //
            // TODO: Add test logic here
            //
            if (endpoint.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            string uid = "admin@milestonetechnoconsulting.com";
            string pass = "Admin@123";

            var svc = RestService.For<IMailRequestService>(new HttpClient(new AuthenticatedHttpClientHandler(uid, pass)) { BaseAddress = new Uri(endpoint) });
            return svc.CreateMailRequest(request);
        }

        [TestMethod]
        public Task<List<MailAPIResponse>> GenerateMailResponse(List<string> mailPieceIds)
        {
            string endpoint = "http://localhost:17005";//"https://authomailuat.corelogic.com";
            //
            // TODO: Add test logic here
            //
            if (endpoint.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            string uid = "admin@milestonetechnoconsulting.com";
            string pass = "Admin@123";

            var svc = RestService.For<IMailRequestService>(new HttpClient(new AuthenticatedHttpClientHandler(uid, pass)) { BaseAddress = new Uri(endpoint) });
            return svc.MailAPIResponse(mailPieceIds);
        }
    }
}
