namespace SkySoft.GameTime
{
    public struct DateTime
    {
        private int _DayOfYear;
        public int DayOfYear { get => _DayOfYear; private set => _DayOfYear = value; }
        public int Month => (DayOfYear - 1) / Calendar.DaysInMonth + 1;
        public string MonthName => Calendar.Months[Month - 1];
        public int DayOfMonth => (DayOfYear - 1) % Calendar.DaysInMonth + 1;
        public int Week
        {
            get
            {
                int DaysOffset = ((int)new DateTime(Year, 1, 1).DayOfWeek + Calendar.DaysInWeek - 1) % Calendar.DaysInWeek;
                int AdjustedDayOfYear = DayOfYear + DaysOffset;
                return (AdjustedDayOfYear - 1) / Calendar.DaysInWeek + 1;
            }
        }
        public int DayOfWeek => ((DayOfYear - 1) % Calendar.DaysInWeek) + 1;
        public string DayOfWeekName => Calendar.Weekdays[DayOfWeek - 1];
        public int Year { get; private set; }
        public int Season => ((Month - 1) / (Calendar.MonthsInYear / Calendar.Seasons.Length)) + 1;
        public string SeasonName => Calendar.Seasons[Season - 1];

        public DateTime(int Year)
        {
            _DayOfYear = 1;
            this.Year = Year;
        }

        public DateTime(int DayOfYear, int Year)
        {
            _DayOfYear = DayOfYear;
            this.Year = Year;
        }

        public DateTime(int Day, int Month, int Year)
        {
            _DayOfYear = (Month - 1) * (Calendar.WeeksInMonth * Calendar.DaysInWeek) + (Day - 1);
            this.Year = Year;
        }

        public DateTime(System.DateTime Base)
        {
            int DaysInYear = System.DateTime.IsLeapYear(Base.Year) ? 366 : 365;
            _DayOfYear = (Base.DayOfYear - 1) * Calendar.DaysInYear / DaysInYear + 1;
            Year = Base.Year;
        }

        public static int DaysBetween(DateTime Start, DateTime End)
        {
            int DaysInStartYear = Calendar.DaysInYear - Start.DayOfYear;
            int DaysInEndYear = Calendar.DaysInYear - End.DayOfYear;

            int DaysBetweenYears = 0;
            for (int Year = Start.Year + 1; Year < End.Year; Year++)
            {
                DaysBetweenYears += Calendar.DaysInYear;
            }

            return DaysInStartYear + DaysBetweenYears + DaysInEndYear;
        }

        public static DateTime operator +(DateTime A, int B)
        {
            int TotalDays = A.DayOfYear + B;
            int Year = A.Year;

            while (TotalDays > Calendar.DaysInYear)
            {
                TotalDays -= Calendar.DaysInYear;
                Year++;
            }

            return new DateTime(TotalDays, Year);
        }

        public static DateTime operator -(DateTime A, int B)
        {
            int TotalDays = A.DayOfYear - B;
            int Year = A.Year;

            while (TotalDays < 1)
            {
                Year--;
                TotalDays += Calendar.DaysInYear;
            }

            return new DateTime(TotalDays, Year);
        }

        public static bool operator ==(DateTime A, DateTime B)
        {
            return A.DayOfYear == B.DayOfYear && A.Year == B.Year;
        }

        public static bool operator !=(DateTime A, DateTime B)
        {
            return A.DayOfYear != B.DayOfYear || A.Year != B.Year;
        }

        public static bool operator <(DateTime A, DateTime B)
        {
            if (A.Year < B.Year)
                return true;
            if (A.Year > B.Year)
                return false;
            return A.DayOfYear < B.DayOfYear;
        }

        public static bool operator >(DateTime A, DateTime B)
        {
            if (A.Year > B.Year)
                return true;
            if (A.Year < B.Year)
                return false;
            return A.DayOfYear > B.DayOfYear;
        }

        public static bool operator <=(DateTime A, DateTime B)
        {
            return A < B || A == B;
        }

        public static bool operator >=(DateTime A, DateTime B)
        {
            return A > B || A == B;
        }

        public override bool Equals(object? Other)
        {
            if (Other is DateTime OtherDateTime)
            {
                return DayOfYear == OtherDateTime.DayOfYear && Year == OtherDateTime.Year;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int Hash = 17;
            Hash = Hash * 23 + DayOfYear.GetHashCode();
            Hash = Hash * 23 + Year.GetHashCode();
            return Hash;
        }

        public static DateTime Now
        {
            get
            {
                return new DateTime(System.DateTime.Now);
            }
        }

        public override string ToString()
        {
            return ToString("D");
        }
        public string ToString(string Format)
        {
            if (string.IsNullOrEmpty(Format))
                Format = "D";

            string Result = Format;
            Result = Result.Replace("D", $"dddd, d MMMM yyyy");
            Result = Result.Replace("dddd", DayOfWeekName);
            Result = Result.Replace("dd", DayOfMonth.ToString("00"));
            Result = Result.Replace("d", DayOfMonth.ToString());
            Result = Result.Replace("MMMM", MonthName);
            Result = Result.Replace("MMM", MonthName.Substring(0, 3));
            Result = Result.Replace("MM", Month.ToString("00"));
            Result = Result.Replace("M", Month.ToString());
            Result = Result.Replace("yyyy", Year.ToString());
            Result = Result.Replace("yy", Year.ToString().Substring(2));

            return Result;
        }
    }

    public class Calendar
    {
        public static int DaysInMonth => WeeksInMonth * DaysInWeek;
        public static int DaysInYear => DaysInMonth * MonthsInYear;

        /// <summary>
        /// The Length in Days a week lasts
        /// </summary>
        public const int DaysInWeek = 7;
        /// <summary>
        /// Ths Length in Weeks a month lasts
        /// </summary>
        public const int WeeksInMonth = 9;
        /// <summary>
        /// The Length in Months a year lasts
        /// </summary>
        public const int MonthsInYear = 12;

        private static string[] _Months = new string[] { };
        public static string[] Months
        {
            get
            {
                if (_Months.Length != MonthsInYear)
                    _Months = new string[12]
                    {
                        "Undra",
                        "Lumina",
                        "Verdantia",
                        "Solstice",
                        "Myrnshadow",
                        "Emberfall",
                        "Frostbloom",
                        "Harvestend",
                        "Starshroud",
                        "Whisperwind",
                        "Shadowveil",
                        "Celestia"
                    };

                return _Months;
            }
        }

        private static string[] _Weekdays = new string[] { };
        public static string[] Weekdays
        {
            get
            {
                if (_Weekdays.Length != DaysInWeek)
                    _Weekdays = new string[7]
                    {
                        "Dawnas",
                        "Fronas",
                        "Byrnas",
                        "Stornas",
                        "Leinas",
                        "Onnas",
                        "Ednas"
                    };

                return _Weekdays;
            }
        }

        private static string[] _Seasons = new string[] { };
        public static string[] Seasons
        {
            get
            {
                if (_Seasons.Length != 4)
                    _Seasons = new string[4]
                    {
                        "Harvest",
                        "Raise",
                        "Gather",
                        "Stow"
                    };

                return _Seasons;
            }
        }
    }
}
