using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Assets.Game.Scripts.TherapyData;
using static MainController;

namespace Assets.Scripts
{
    public class ReportsManager
    {

        public static Stream GetPatientReport(Patient patient)
        {
            string report = string.Empty;
            int numOf2Pad = 0;
            int numOf2Tip = 0;
            int numOf3Pad = 0;
            int numOf3Tip = 0;
            report += "Personal Information:";
            report += "\n   Id: " + patient.Id;
            report += "\n   Full Name: " + patient.FullName;
            report += "\n   Height: " + patient.Height;
            report += "\n   Weight: " + patient.Weight;
            report += "\n   Hand: " + patient.Hand;
            report += "\n\nPinch Motion Range from: " + patient.MotionRange.CreationTime;
            report += "\n   2 Pad: " + (int)(patient.MotionRange.Pad2 * 100) + "%";
            report += "\n   2 Tip: " + (int)(patient.MotionRange.Tip2 * 100) + "%";
            report += "\n   3 Pad: " + (int)(patient.MotionRange.Pad3 * 100) + "%";
            report += "\n   3 Tip: " + (int)(patient.MotionRange.Tip3 * 100) + "%";
            report += "\n";
            if (patient.MotionRangeHistory != null && patient.MotionRangeHistory.Count > 0)
            {
                foreach (MotionRange mr in patient.MotionRangeHistory)
                {
                    report += "\n\nHistory of Motion Range";
                    report += "\nFrom: " + mr.CreationTime;
                    report += "\n   2 Pad: " + (int)(mr.Pad2 * 100) + "%";
                    report += "\n   2 Tip: " + (int)(mr.Tip2 * 100) + "%";
                    report += "\n   3 Pad: " + (int)(mr.Pad3 * 100) + "%";
                    report += "\n   3 Tip: " + (int)(mr.Tip3 * 100) + "%";
                    report += "\n";
                }
            }
            
            if (patient.CurrentTreatment != null && patient.CurrentTreatment.Plan.Count != 0)
            {
                report += "\nCurrent Treatment:";
                report += "\n   Level Number: " + patient.CurrentTreatment.TreatmentNumber;
                report += "\n   Creation Time: " + patient.CurrentTreatment.CreationTime;
                report += "\n\n   Actions:";
                foreach (Challenge challenge in patient.CurrentTreatment.Plan)
                {
                    foreach (PinchAction action in challenge.ActionsList)
                    {
                        switch (action.Type)
                        {
                            case PinchType.Pad2:
                                numOf2Pad++;
                                break;
                            case PinchType.Pad3:
                                numOf3Pad++;
                                break;
                            case PinchType.Tip2:
                                numOf2Tip++;
                                break;
                            case PinchType.Tip3:
                                numOf3Tip++;
                                break;
                        }
                    }
                }
                report += "\n       2 Pad: " + numOf2Pad + " times.";
                report += "\n       2 Tip: " + numOf2Tip + " times.";
                report += "\n       3 Pad: " + numOf3Pad + " times.";
                report += "\n       3 Tip: " + numOf3Tip + " times.";
                report += "\n\nHistory:";

                if (patient.TreatmentsHistory != null && patient.TreatmentsHistory.Count != 0)
                {
                    foreach (TreatmentPlan tp in patient.TreatmentsHistory)
                    {
                        report += "\n";
                        numOf2Pad = 0;
                        numOf2Tip = 0;
                        numOf3Pad = 0;
                        numOf3Tip = 0;
                        report += "\n   Level Number: " + tp.TreatmentNumber;
                        report += "\n   Creation Time: " + tp.CreationTime;
                        report += "\n\n   Actions:";
                        foreach (Challenge challenge in tp.Plan)
                        {
                            foreach (PinchAction action in challenge.ActionsList)
                            {
                                switch (action.Type)
                                {
                                    case PinchType.Pad2:
                                        numOf2Pad++;
                                        break;
                                    case PinchType.Pad3:
                                        numOf3Pad++;
                                        break;
                                    case PinchType.Tip2:
                                        numOf2Tip++;
                                        break;
                                    case PinchType.Tip3:
                                        numOf3Tip++;
                                        break;
                                }
                            }
                        }
                        report += "\n       2 Pad: " + numOf2Pad + " times.";
                        report += "\n       2 Tip: " + numOf2Tip + " times.";
                        report += "\n       3 Pad: " + numOf3Pad + " times.";
                        report += "\n       3 Tip: " + numOf3Tip + " times.";
                    }
                }
            }
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(report);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
