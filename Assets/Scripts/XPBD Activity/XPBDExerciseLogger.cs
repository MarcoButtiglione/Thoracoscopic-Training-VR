using System.Globalization;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class XPBDExerciseLogger : Singleton<XPBDExerciseLogger>
{
    private string _summaryLogPath;
    private string _zoneSpecificLogPath;
    private string _graspableSelectedLogPath;
    private string _grabbedVerticesLogPath;
    private CultureInfo _cultureInfo;
    public CultureInfo cultureInfo
    {
        get { return _cultureInfo; }
    }

    private void Awake()
    {
        _cultureInfo = new CultureInfo("en-GB");
        _summaryLogPath = Application.persistentDataPath + "/XPBD_summaryLogs.csv";
        _zoneSpecificLogPath = Application.persistentDataPath + "/XPBD_zoneSpecificLogs.csv";
        _graspableSelectedLogPath = Application.persistentDataPath + "/XPBD_graspableSelectedLogs.csv";
        _grabbedVerticesLogPath = Application.persistentDataPath + "/XPBD_grabbedVerticesLogs.csv";

        // These are done only the first time the application is opened in a device.
        if (!File.Exists(_summaryLogPath))
        {
            string summaryLogsColumnNames = "SessionId;LogTimestamp;ExerciseName;WasExerciseCompleted;ExecutionTime(s);GrabErrors;NumberOfLimitsTouched;TimeSpentTouchingLimits(s);MostUsedZone;MostUsedZoneTime(s);ZoneWithMostLimitsTouched;NumberOfLimitsTouchedInZoneWithMostLimitsTouched";
            using (StreamWriter writer = File.AppendText(_summaryLogPath))
            {
                writer.WriteLine(summaryLogsColumnNames);
            };
        }

        if (!File.Exists(_zoneSpecificLogPath))
        {
            string zoneSpecificLogsColumnNames = "SessionIdReference;ZoneName;ZoneUsageTime(s);NumberOfTimesLimitsOfTheZoneWereTouched;AmountOfTimeLimitsOfTheZoneWereTouched(s)";
            using (StreamWriter writer = File.AppendText(_zoneSpecificLogPath))
            {
                writer.WriteLine(zoneSpecificLogsColumnNames);
            };
        }

        if (!File.Exists(_graspableSelectedLogPath))
        {
            string graspableSpecificLogsColumnNames = "SessionIdReference;SelectedVertexIndex;SelectedStartedTimestamp;SelectedEndedTimestamp";
            using (StreamWriter writer = File.AppendText(_graspableSelectedLogPath))
            {
                writer.WriteLine(graspableSpecificLogsColumnNames);
            };
        }
        
        if (!File.Exists(_grabbedVerticesLogPath))
        {
            string graspableCorrectPlacementLogsColumnNames = "SessionIdReference;SelectedVertexIndex;GrabbedVertexIndex;IsAccepted;DistanceBetweenVertices;Timestamp";
            using (StreamWriter writer = File.AppendText(_grabbedVerticesLogPath))
            {
                writer.WriteLine(graspableCorrectPlacementLogsColumnNames);
            };
        }
    }

    public void LogSessionData(string endOfSessionTimestamp, string minigameName, string wasExerciseCompleted,
         float executionTime, int puzzleErrors, int numberOfLimitsTouched, float timeSpentTouchingLimits,
         string mostUsedZone, float mostUsedZoneTime, string zoneWithMostLimitsTouched, int numberOfLimitsTouchedInZoneWithMostLimitsTouched,
         List<Tuple<string, float, int, float>> zonesInfo, List<SelectedVertexInfo> selectedVertexInfos, List<GrabbedVertexInfo> grabbedVertexInfos)
    {
        string sessionId = minigameName + " " + endOfSessionTimestamp;
        string sessionLogValues = sessionId + ";" + endOfSessionTimestamp + ";" + minigameName + ";" + wasExerciseCompleted + ";" + executionTime + ";" +
                                  puzzleErrors + ";" + numberOfLimitsTouched + ";" + timeSpentTouchingLimits + ";" + mostUsedZone + ";" + mostUsedZoneTime + ";" +
                                  zoneWithMostLimitsTouched + ";" + numberOfLimitsTouchedInZoneWithMostLimitsTouched;

        using (StreamWriter writer = File.AppendText(_summaryLogPath))
        {
            writer.WriteLine(sessionLogValues);
        };

        using (StreamWriter writer = File.AppendText(_zoneSpecificLogPath))
        {
            foreach (var tuple in zonesInfo)
            {
                string zoneInfoEntry = sessionId + ";" + tuple.Item1 + ";" + tuple.Item2 + ";" + tuple.Item3 + ";" + tuple.Item4;
                writer.WriteLine(zoneInfoEntry);
            }
        };

        using (StreamWriter writer = File.AppendText(_graspableSelectedLogPath))
        {
            foreach (var tuple in selectedVertexInfos)
            {
                string graspableInfoEntry = sessionId + ";" + tuple.vertexIndex + ";" + tuple.selectedStartedTimestamp + ";" + tuple.selectedEndedTimestamp;
                writer.WriteLine(graspableInfoEntry);
            }
        };

        using (StreamWriter writer = File.AppendText(_grabbedVerticesLogPath))
        {
            foreach (var tuple in grabbedVertexInfos)
            {
                string graspableCorrectPlacementInfoEntry = sessionId + ";" + tuple.selectedVertexIndex + ";" + tuple.grabbedVertexIndex + ";" + tuple.isAccepted + ";" + tuple.distanceBetweenVertices + ";" + tuple.timestamp;
                writer.WriteLine(graspableCorrectPlacementInfoEntry);
            }
        };
    }
}
