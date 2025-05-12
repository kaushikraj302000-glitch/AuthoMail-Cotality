using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

public class Program
{
    static async Task Main()
    {
        var url = "https://corelogicxb002.sendproenterprise.com/Logistics%20Server/xmlservice.aspx?apikey=0RHS0IPMRRGA9MHYLQBXEFYVG";

        //Test xml
        string xmlPayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<PierbridgeShipRequest>
    <Carrier>106</Carrier>
    <ServiceType>981</ServiceType>
    <Receiver>
        <CompanyName>The White House</CompanyName>
        <Street>1600 Pennsylvania Avenue NW</Street>
        <City>Washington</City>
        <Region>DC</Region>
        <PostalCode>20500</PostalCode>
        <Country>US</Country>
    </Receiver>
    <Packages>
        <Package>
            <ShipperReference>MyRef</ShipperReference>
            <ReceiverName>Mr President</ReceiverName>
            <ReceiverPhone>202-456-1414</ReceiverPhone>
            <PackageType>656</PackageType>
            <Weight>10.5</Weight>
            <Length>7</Length>
            <Width>8</Width>
            <Height>9</Height>
            <ContentDescription>A gift</ContentDescription>
        </Package>
    </Packages>
    <UserName>blackbox</UserName>
</PierbridgeShipRequest>
";

        //Need to use this code while integrating the dll.
        //Console.WriteLine("Enter XML payload (end with an empty line):");

        //StringBuilder sb = new StringBuilder();
        //string line;
        //while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
        //{
        //    sb.AppendLine(line);
        //}

        //string xmlPayload = sb.ToString();



        using (var client = new HttpClient())
        {
            var content = new StringContent(xmlPayload, Encoding.UTF8, "application/xml");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Response Status: " + response.StatusCode);

                // Parse XML
                var xDoc = XDocument.Parse(responseContent);
                var contentElement = xDoc.Root.Element("Content");

                if (contentElement != null)
                {
                    string base64String = contentElement.Value;

                    // Convert Base64 to byte[]
                    byte[] pdfBytes = Convert.FromBase64String(base64String);

                    // Save to file
                    string filePath = "output.pdf";
                    File.WriteAllBytes(filePath, pdfBytes);

                    Console.WriteLine("PDF saved to: " + Path.GetFullPath(filePath));
                }
                else
                {
                    Console.WriteLine("Content element not found in response.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }
    }
}
