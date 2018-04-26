using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Services;
using System.Net.Mail;
using System.Configuration;
using System.IO;

namespace AM.Monitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            MonitorBatchesService monitorSvc = new MonitorBatchesService();
            var pendingBatches = monitorSvc.GetPendingBatches();

            if (pendingBatches.Count > 0)
            {
                try
                {
                    using (SmtpClient smtpClient = new SmtpClient())
                    {
                        using (MailMessage msg = new MailMessage())
                        {
                            msg.IsBodyHtml = true;
                            msg.To.Add(ConfigurationManager.AppSettings["MonitorEmail_To"]);
                            msg.CC.Add(ConfigurationManager.AppSettings["MonitorEmail_Cc"]);
                            msg.Subject = ConfigurationManager.AppSettings["MonitorEmail_Subject"];
                            msg.Priority = MailPriority.High;

                            string monitor_PrintBatch = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string monitor_AppCode = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string monitor_JobName = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string monitor_Pipelinestatus = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string monitor_ReceivedOn = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string monitor_DownloadedOn = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string monitor_PreppedOn = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            StringBuilder sb = new StringBuilder(File.ReadAllText(Path.GetFullPath(ConfigurationManager.AppSettings["MonitorEmailTemplate"])));
                            StringBuilder batchData = new StringBuilder();
                            foreach (var pendingBatch in pendingBatches)
                            {
                                batchData.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", monitor_PrintBatch, pendingBatch.PrintBatch.ToString()));
                                batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_AppCode, pendingBatch.AppCode.ToString()));
                                batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_JobName, pendingBatch.JobName.ToString()));
                                batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_Pipelinestatus, pendingBatch.Pipelinestatus.ToString()));
                                batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_ReceivedOn, pendingBatch.ReceivedOn.ToString()));
                                batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_DownloadedOn, !string.IsNullOrEmpty(pendingBatch.DownloadedOn.ToString()) ? pendingBatch.DownloadedOn.ToString() : string.Empty));
                                batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td> </tr>", monitor_PreppedOn, !string.IsNullOrEmpty(pendingBatch.PreppedOn) ? pendingBatch.PreppedOn.ToString() : string.Empty));
                            }
                            sb = sb.Replace("<%trdata%>", batchData.ToString());
                            msg.Body = sb.ToString();
                            smtpClient.Send(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
