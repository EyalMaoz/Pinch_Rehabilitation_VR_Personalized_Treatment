using Assets.Scripts;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using static MainController;

public class MailsSystemManager
{
    private const string SystemEmail = "PinchProject2020@gmail.com";
    private const string SystemPass = "PinchProject123";

    private const string ReportExtention = ".doc";

    private static SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com")
    {
        Port = 587,
        Credentials = new System.Net.NetworkCredential("PinchProject2020", SystemPass),
        EnableSsl = true
    };

    public static void SendAllReportsInMail(Therapist therapist)
    {
        Thread thread = new Thread(() => SendAllReportsInMailForThread(therapist));
        thread.Start();
    }

    public static void SendPatientReportInMail(Therapist therapist, Patient patient)
    {
        Thread thread = new Thread(() => SendPatientReportInMailForThread(therapist, patient));
        thread.Start();
    }

    public static void SendInformationLogInMail(Therapist therapist)
    {
        Thread thread = new Thread(() => SendInformationLogInMailForThread(therapist));
        thread.Start();
    }

    public static void SendErrorLogInMail(Therapist therapist)
    {
        Thread thread = new Thread(() => SendErrorLogInMailForThread(therapist));
        thread.Start();
    }

    private static void SendAllReportsInMailForThread(Therapist therapist)
    {
        try
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(SystemEmail);
            mail.To.Add(therapist.Email);
            mail.Subject = therapist.FirstName + " " + therapist.LastName + ": All Patients Reports";
            mail.Body = "All patients reports are attached as requested.\nIf the file is not displaying correctly, please open it via Google Docs or Notpad.";

            string[] patientsFiles = Directory.GetFiles(PinchConstants.PatientsDirectoryPath);

            foreach (string file in patientsFiles)
            {
                Patient patient = QuestFileManager.GetPatientFromFile(file);

                Attachment attachment = new Attachment(ReportsManager.GetPatientReport(patient), patient.Id + ReportExtention);
                mail.Attachments.Add(attachment);
            }

            SmtpServer.Send(mail);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), LogType.Error);
        }
    }

    private static void SendPatientReportInMailForThread(Therapist therapist, Patient patient)
    {
        try
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(SystemEmail);
            mail.To.Add(therapist.Email);
            mail.Subject = therapist.FirstName + " " + therapist.LastName + ": Patient Report";
            mail.Body = "Report of patient: " + patient.FullName + ", id: " + patient.Id + ", is attached as requested.\nIf the file is not displaying correctly, please open it via Google Docs or Notpad.";

            Attachment attachment = new Attachment(ReportsManager.GetPatientReport(patient), patient.Id + ReportExtention);
            mail.Attachments.Add(attachment);

            SmtpServer.Send(mail);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), LogType.Error);
        }
    }

    private static void SendInformationLogInMailForThread(Therapist therapist)
    {
        try
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(SystemEmail);
            mail.To.Add(therapist.Email);
            mail.Subject = therapist.FirstName + " " + therapist.LastName + ": Information Log";
            if (File.Exists(PinchConstants.InformationLogFilePath))
            {
                mail.Body = "Information log attached as requested.\nIf the file is not displaying correctly, please open it via Google Docs or Notpad.";
                Attachment attachment = new Attachment(PinchConstants.InformationLogFilePath);
                mail.Attachments.Add(attachment);
            }
            else
            {
                mail.Body = "Information log is empty.";
            }

            SmtpServer.Send(mail);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), LogType.Error);
        }
    }

    private static void SendErrorLogInMailForThread(Therapist therapist)
    {
        try
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(SystemEmail);
            mail.To.Add(therapist.Email);
            mail.Subject = therapist.FirstName + " " + therapist.LastName + ": Error Log";
            if (File.Exists(PinchConstants.ErrorLogFilePath))
            {
                mail.Body = "Error log attached as requested.\nIf the file is not displaying correctly, please open it via Google Docs or Notpad.";
                Attachment attachment = new Attachment(PinchConstants.ErrorLogFilePath);
                mail.Attachments.Add(attachment);
            }
            else
            {
                mail.Body = "Error log is empty.";
            }

            SmtpServer.Send(mail);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), LogType.Error);
        }
    }
}
