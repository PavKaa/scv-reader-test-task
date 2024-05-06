using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Entity
{
	internal class PersonInfoRecord
	{
		[Name("phoneNumber")]
		public string? PhoneNumber { get; set; }

		[Name("email")]
		public string Email { get; set; }

		[Name("name")]
		public string Name { get; set; }

		[Name("address")]
		public string Address { get; set; }

		[Name("userAgent")]
		public string UserAgent { get; set; }

		[Name("hexcolor")]
		public string Hexcolor { get; set; }
    }
}
