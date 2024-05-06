using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Entity
{
	internal class EventStatistics
	{
		public decimal TotalRevenue { get; set; }

		public string MostPopularBrand { get; set; }

		public long MostPopularCategoryId { get; set; }

		public string MostPopularCategoryCode { get; set; }

		public long MostPopularProductId { get; set; }
	}
}
