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
            var pendingBatchesToSubmit = monitorSvc.GetPendingBatchesToSubmit();
            var pendingBatchesToReportBack = monitorSvc.GetPendingBatchesToReportBack();
            string monitor_PrintBatch = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            string monitor_AppCode = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            string monitor_JobName = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            string monitor_Pipelinestatus = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            string monitor_ReceivedOn = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            string monitor_DownloadedOn = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            string monitor_PreppedOn = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
            
            if (pendingBatchesToSubmit.Count > 0)
            {
                PrepEmailBody_PendingBatchesToSubmit(monitorSvc, pendingBatchesToSubmit, monitor_PrintBatch, monitor_AppCode, monitor_JobName, monitor_Pipelinestatus, monitor_ReceivedOn, monitor_DownloadedOn, monitor_PreppedOn,new StringBuilder(File.ReadAllText(Path.GetFullPath(ConfigurationManager.AppSettings["MonitorEmailTemplate"]))));
            }

            if (pendingBatchesToReportBack.Count > 0)
            {
                PrepEmailBody_PendingBatchesToReportBack(monitorSvc, pendingBatchesToReportBack, monitor_PrintBatch, monitor_AppCode, monitor_JobName, monitor_Pipelinestatus, monitor_ReceivedOn, monitor_DownloadedOn, monitor_PreppedOn, new StringBuilder(File.ReadAllText(Path.GetFullPath(ConfigurationManager.AppSettings["MonitorEmailTemplate"]))));
            }
        }

        private static void PrepEmailBody_PendingBatchesToSubmit(MonitorBatchesService monitorSvc, List<dynamic> pendingBatchesToSubmit, string monitor_PrintBatch, string monitor_AppCode, string monitor_JobName, string monitor_Pipelinestatus, string monitor_ReceivedOn, string monitor_DownloadedOn, string monitor_PreppedOn, StringBuilder sb)
        {
            try
            {
                StringBuilder batchData = new StringBuilder();
                foreach (var pendingBatch in pendingBatchesToSubmit)
                {
                    int isPrintBatchExist = monitorSvc.CheckPrintBatchIdExist(int.Parse(pendingBatch.PrintBatch.ToString()), true);
                    if (isPrintBatchExist > 0)
                    {
                        int lastEmailSentHours = monitorSvc.CheckLastEmailSentTime(int.Parse(pendingBatch.PrintBatch.ToString()), true);
                        if (lastEmailSentHours > 5)
                        {
                            batchData.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", monitor_PrintBatch, pendingBatch.PrintBatch.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_AppCode, pendingBatch.AppCode.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_JobName, pendingBatch.JobName.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_Pipelinestatus, pendingBatch.Pipelinestatus.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_ReceivedOn, pendingBatch.ReceivedOn.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_DownloadedOn, pendingBatch.DownloadedOn != null ? pendingBatch.DownloadedOn : string.Empty));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td> </tr>", monitor_PreppedOn, pendingBatch.PreppedOn != null ? pendingBatch.PreppedOn : string.Empty));
                            monitorSvc.UpdateEmailTimeStamp(int.Parse(pendingBatch.PrintBatch.ToString()), true);
                        }
                    }
                    else
                    {
                        batchData.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", monitor_PrintBatch, pendingBatch.PrintBatch.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_AppCode, pendingBatch.AppCode.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_JobName, pendingBatch.JobName.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_Pipelinestatus, pendingBatch.Pipelinestatus.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_ReceivedOn, pendingBatch.ReceivedOn.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_DownloadedOn, pendingBatch.DownloadedOn != null ? pendingBatch.DownloadedOn : string.Empty));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td> </tr>", monitor_PreppedOn, pendingBatch.PreppedOn != null ? pendingBatch.PreppedOn : string.Empty));
                        monitorSvc.SaveEmailSentTimeStamp(int.Parse(pendingBatch.PrintBatch.ToString()), true);
                    }

                }
                if (batchData.Length > 0)
                {
                    SendEmail(sb, batchData, ConfigurationManager.AppSettings["MonitorSubmitHeading"].ToString(), ConfigurationManager.AppSettings["Monitor_Submit_Email_To"].ToString(), ConfigurationManager.AppSettings["Monitor_Submit_Email_Cc"].ToString(), ConfigurationManager.AppSettings["Monitor_Submit_Email_Subject"].ToString()); 
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString(),true);
            }
        }

        private static void PrepEmailBody_PendingBatchesToReportBack(MonitorBatchesService monitorSvc, List<dynamic> pendingBatchesToReportBack, string monitor_PrintBatch, string monitor_AppCode, string monitor_JobName, string monitor_Pipelinestatus, string monitor_ReceivedOn, string monitor_DownloadedOn, string monitor_PreppedOn, StringBuilder sb)
        {
            try
            {
                StringBuilder batchData = new StringBuilder();
                foreach (var pendingBatch in pendingBatchesToReportBack)
                {
                    int isPrintBatchExist = monitorSvc.CheckPrintBatchIdExist(int.Parse(pendingBatch.PrintBatch.ToString()), false);
                    if (isPrintBatchExist > 0)
                    {
                        int lastEmailSentHours = monitorSvc.CheckLastEmailSentTime(int.Parse(pendingBatch.PrintBatch.ToString()), false);
                        if (lastEmailSentHours > 5)
                        {
                            batchData.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", monitor_PrintBatch, pendingBatch.PrintBatch.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_AppCode, pendingBatch.AppCode.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_JobName, pendingBatch.JobName.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_Pipelinestatus, pendingBatch.Pipelinestatus.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_ReceivedOn, pendingBatch.ReceivedOn.ToString()));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_DownloadedOn, pendingBatch.DownloadedOn != null ? pendingBatch.DownloadedOn : string.Empty));
                            batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td> </tr>", monitor_PreppedOn, pendingBatch.PreppedOn != null ? pendingBatch.PreppedOn : string.Empty));
                            monitorSvc.UpdateEmailTimeStamp(int.Parse(pendingBatch.PrintBatch.ToString()), false);
                        }
                    }
                    else
                    {
                        batchData.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", monitor_PrintBatch, pendingBatch.PrintBatch.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_AppCode, pendingBatch.AppCode.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_JobName, pendingBatch.JobName.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", monitor_Pipelinestatus, pendingBatch.Pipelinestatus.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_ReceivedOn, pendingBatch.ReceivedOn.ToString()));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", monitor_DownloadedOn, pendingBatch.DownloadedOn != null ? pendingBatch.DownloadedOn : string.Empty));
                        batchData.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td> </tr>", monitor_PreppedOn, pendingBatch.PreppedOn != null ? pendingBatch.PreppedOn : string.Empty));
                        monitorSvc.SaveEmailSentTimeStamp(int.Parse(pendingBatch.PrintBatch.ToString()), false);
                    }

                }
                if (batchData.Length > 0)
                {
                    SendEmail(sb, batchData, ConfigurationManager.AppSettings["MonitorReportbackHeading"].ToString(), ConfigurationManager.AppSettings["Monitor_ReportedBack_Email_To"].ToString(), ConfigurationManager.AppSettings["Monitor_ReportedBack_Email_Cc"].ToString(), ConfigurationManager.AppSettings["Monitor_ReportedBack_Email_Subject"].ToString()); 
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString(), true);
            }
        }

        private static void SendEmail(StringBuilder templateBody, StringBuilder batchData, string heading, string email_To, string email_Cc, string email_Subject)
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(email_To);
                    msg.CC.Add(email_Cc);
                    msg.Subject = email_Subject;
                    msg.Priority = MailPriority.High;
                    templateBody = templateBody.Replace("<%heading%>", heading);
                    templateBody = templateBody.Replace("<%trdata%>", batchData.ToString());
                    msg.Body = templateBody.ToString();
                    smtpClient.Send(msg);
                }
            }
        }

        public static void LogError(string message, bool sendEmail = false)
        {
            Console.WriteLine(message);
            File.AppendAllText(ConfigurationManager.AppSettings["LogFilePath"], string.Format("{0} : {1}{2}", DateTime.Now.ToString("MMddyyyy HHmmssfff"), message, Environment.NewLine));

            if (sendEmail)
            {
                using (SmtpClient smtpClient = new SmtpClient())
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = false;
                    msg.To.Add(ConfigurationManager.AppSettings["LogErr_To"]);
                    //msg.CC.Add("sandpatel@corelogic.com"); //Uncomment during production
                    msg.Subject = "Exception occurred in AuthoMail monitor batch Job.";
                    msg.Body = message;
                    msg.Priority = MailPriority.High;
                    smtpClient.Send(msg);
                }
            }

        }
    }
}
