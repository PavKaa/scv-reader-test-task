using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Entity
{
	internal class EventRecord
	{
		[TypeConverter(typeof(Annotation.DateTimeConverter))]
		[Name("event_time")]
		public DateTime EventTime { get; set; }

		[Name("event_type")]
        public string EventType { get; set; }

		[Name("product_id")]
        public long ProductId { get; set; }

		[Name("category_id")]
        public long CategoryId { get; set; }

		[Name("category_code")]
        public string? CategoryCode { get; set; }

		[Name("brand")]
        public string? Brand { get; set; }

		[Name("price")]
        public decimal Price { get; set; }

		[Name("user_id")]
        public long UserId { get; set; }

		[Name("user_session")]
        public string UserSession { get; set; }
    }
}
