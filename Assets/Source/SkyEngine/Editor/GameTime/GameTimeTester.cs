using UnityEditor;
using UnityEngine;
using SkySoft.GameTime;
using System.Collections.Generic;
using System.Linq;

public class GameTimeTester : EditorWindow
{
    internal class DateVariables
    {
        private bool IsRealTime = false;
        public int Day, Month, Year;
        public int MonthLength
        {
            get
            {
                if (IsRealTime)
                    return System.DateTime.DaysInMonth(Year, Month);
                else
                    return Calendar.DaysInMonth;
            }
        }
        public string[] MonthNames
        {
            get
            {
                if (!IsRealTime)
                    return Calendar.Months;
                else
                    return new string[]
                    {
                        "February",
                        "January",
                        "March",
                        "April",
                        "May",
                        "June",
                        "July",
                        "August",
                        "September",
                        "October",
                        "November",
                        "December"
                    };
            }
        }

        public string[] WeekDays
        {
            get
            {
                if (!IsRealTime)
                    return Calendar.Weekdays;
                else
                    return new string[]
                    {
                        "Monday",
                        "Tuesday",
                        "Wednesday",
                        "Thursday",
                        "Friday",
                        "Saturday",
                        "Sunday"
                    };
            }
        }

        public DateVariables(DateTime Base)
        {
            Day = Base.DayOfMonth;
            Month = Base.Month;
            Year = Base.Year;
        }

        public DateVariables(System.DateTime Base)
        {
            IsRealTime = true;
            Day = Base.Day;
            Month = Base.Month;
            Year = Base.Year;
        }
    }

    [MenuItem("SkyEngine/Test/Game Time")]
    public static void CreateGameTimeWindow()
    {
        GameTimeTester Tester = GetWindow<GameTimeTester>("Game Time Tester");
    }

    internal DateVariables Vars = new DateVariables(System.DateTime.Now);

    private Vector2 Scroll = Vector2.zero;

    bool DoneFirstCalculation = false;
    System.DateTime WorldTimeOutput;
    DateTime GameTimeOutput;

    private void DisplayLabels(params GUIContent[] Labels)
    {
        EditorGUILayout.BeginHorizontal();
        foreach (GUIContent Label in Labels)
        {
            EditorGUILayout.LabelField(Label);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnGUI()
    {
        string[] _Days = Enumerable.Range(1, Vars.MonthLength).Select(I => I.ToString()).ToArray();
        string[] _Months = Enumerable.Range(1, 12).Select(I => System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(I)).ToArray();

        Vars.Day = Mathf.Clamp(Vars.Day, 1, _Days.Length);
        Vars.Month = Mathf.Clamp(Vars.Month, 1, _Months.Length);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            GUILayout.Label("Input Date:");

            EditorGUILayout.BeginHorizontal();
            {
                Vars.Day = EditorGUILayout.Popup(Vars.Day - 1, _Days) + 1;
                Vars.Month = EditorGUILayout.Popup(Vars.Month - 1, _Months) + 1;
                Vars.Year = EditorGUILayout.IntField(Vars.Year);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        Scroll = EditorGUILayout.BeginScrollView(Scroll, GUILayout.ExpandHeight(true));
        {
            EditorGUILayout.LabelField("Time Conversion Table");

            if (DoneFirstCalculation)
            {
                DisplayLabels(new GUIContent("Property"), new GUIContent("World Time"), new GUIContent("Game Time"));
                DisplayLabels(new GUIContent("Year"), new GUIContent(WorldTimeOutput.Year.ToString()), new GUIContent(GameTimeOutput.Year.ToString()));
                DisplayLabels(new GUIContent("Day of Week"), new GUIContent(WorldTimeOutput.ToString("dddd")), new GUIContent(GameTimeOutput.DayOfWeekName));
                DisplayLabels(new GUIContent("Day of Month"), new GUIContent(WorldTimeOutput.Day.ToString()), new GUIContent(GameTimeOutput.DayOfMonth.ToString()));
                DisplayLabels(new GUIContent("Month"), new GUIContent(_Months[WorldTimeOutput.Month - 1]), new GUIContent(GameTimeOutput.MonthName));
                DisplayLabels(new GUIContent("Week"), new GUIContent(((WorldTimeOutput.DayOfYear - 1) / 7 + 1).ToString()), new GUIContent(GameTimeOutput.Week.ToString()));
                EditorGUILayout.Space();
                DisplayLabels(new GUIContent("Earth Time"), new GUIContent(WorldTimeOutput.ToString("D")));
                DisplayLabels(new GUIContent("Erinheim Time"), new GUIContent(GameTimeOutput.ToString("D")));
            }
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Convert"))
        {
            WorldTimeOutput = new System.DateTime(Vars.Year, Vars.Month, Vars.Day);
            GameTimeOutput = new DateTime(WorldTimeOutput);
            DoneFirstCalculation = true;
        }
    }
}
