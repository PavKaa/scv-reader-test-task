using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Annotation
{
	internal class DateTimeConverter : DefaultTypeConverter
	{
		public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
		{
			return DateTime.ParseExact(text, "yyyy-MM-dd HH:mm:ss UTC", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}
	}
}
