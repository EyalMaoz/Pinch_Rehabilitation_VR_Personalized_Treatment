using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PinchConstants
{
    private const string mainDir = @"/sdcard/PinchProject/";
    // Log Files
    public const string InformationLogFilePath = SystemDirectoryPath + @"InformationLog.txt";
    public const string ErrorLogFilePath = SystemDirectoryPath + @"ErrorLog.txt";
    // Directories
    public const string SystemDirectoryPath = mainDir + @"System/";
    public const string TherapistsDirectoryPath = mainDir + @"Therapists/";
    public const string PatientsDirectoryPath = mainDir + @"Patients/";
}

