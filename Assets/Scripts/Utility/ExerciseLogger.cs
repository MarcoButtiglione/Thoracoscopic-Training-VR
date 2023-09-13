using System.Globalization;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseLogger : Singleton<ExerciseLogger>
{
    private string _summaryLogPath;
    private string _zoneSpecificLogPath;
    private string _graspableSpecificLogPath;
    private string _graspableCorrectPlacementLogPath;
    private CultureInfo _cultureInfo;
    public CultureInfo cultureInfo
    {
        get { return _cultureInfo; }
    }

    private void Awake()
    {
        _cultureInfo = new CultureInfo("en-GB");
        _summaryLogPath = Application.persistentDataPath + "/summaryLogs.csv";
        _zoneSpecificLogPath = Application.persistentDataPath + "/zoneSpecificLogs.csv";
        _graspableSpecificLogPath = Application.persistentDataPath + "/graspableSpecificLogs.csv";
        _graspableCorrectPlacementLogPath = Application.persistentDataPath + "/graspableCorrectPlacementTimestamps.csv";

        // These are done only the first time the application is opened in a device.
        if (!File.Exists(_summaryLogPath))
        {
            string summaryLogsColumnNames = "SessionId;LogTimestamp;ExerciseName;WasExerciseCompleted;ExecutionTime(s);PuzzleErrors;NumberOfLimitsTouched;TimeSpentTouchingLimits(s);MostUsedZone;MostUsedZoneTime(s);ZoneWithMostLimitsTouched;NumberOfLimitsTouchedInZoneWithMostLimitsTouched";
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

        if (!File.Exists(_graspableSpecificLogPath))
        {
            string graspableSpecificLogsColumnNames = "SessionIdReference;GraspableName;GraspStartedTimestamp;GraspEndedTimestamp";
            using (StreamWriter writer = File.AppendText(_graspableSpecificLogPath))
            {
                writer.WriteLine(graspableSpecificLogsColumnNames);
            };
        }

        if (!File.Exists(_graspableCorrectPlacementLogPath))
        {
            string graspableCorrectPlacementLogsColumnNames = "SessionIdReference;GraspableName;GraspCorrectPlacementTimestamp";
            using (StreamWriter writer = File.AppendText(_graspableCorrectPlacementLogPath))
            {
                writer.WriteLine(graspableCorrectPlacementLogsColumnNames);
            };
        }
    }

    public void LogSessionData(string endOfSessionTimestamp, string minigameName, string wasExerciseCompleted,
         float executionTime, int puzzleErrors, int numberOfLimitsTouched, float timeSpentTouchingLimits,
         string mostUsedZone, float mostUsedZoneTime, string zoneWithMostLimitsTouched, int numberOfLimitsTouchedInZoneWithMostLimitsTouched,
         List<Tuple<string, float, int, float>> zonesInfo, List<Tuple<string, string, string>> graspablesInfo, List<Tuple<string, string>> graspablesCorrectPlacementInfo)
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

        using (StreamWriter writer = File.AppendText(_graspableSpecificLogPath))
        {
            foreach (var tuple in graspablesInfo)
            {
                string graspableInfoEntry = sessionId + ";" + tuple.Item1 + ";" + tuple.Item2 + ";" + tuple.Item3;
                writer.WriteLine(graspableInfoEntry);
            }
        };

        using (StreamWriter writer = File.AppendText(_graspableCorrectPlacementLogPath))
        {
            foreach (var tuple in graspablesCorrectPlacementInfo)
            {
                string graspableCorrectPlacementInfoEntry = sessionId + ";" + tuple.Item1 + ";" + tuple.Item2;
                writer.WriteLine(graspableCorrectPlacementInfoEntry);
            }
        };
    }
}
