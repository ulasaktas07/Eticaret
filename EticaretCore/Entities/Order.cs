﻿using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Entities
{
	public class Order:IEntity
	{
		public int Id { get; set; }
		[Display(Name = "Sipariş No"),StringLength(50)]
		public string OrderNumber { get; set; }
		[Display(Name = "Sipariş Toplamı")]
		public decimal TotalPrice { get; set; }
		[Display(Name = "Müşteri No")]
		public int AppUserId { get; set; }
		[Display(Name = "Müşteri"), StringLength(50)]
		public string CustomerId { get; set; }
		[Display(Name = "Fatura Adresi"), StringLength(50)]
		public string BillingAddress { get; set; }
		[Display(Name = "Teslimat Adresi"), StringLength(100)]
		public string DeliveryAddress { get; set; }
		[Display(Name = "Sipariş Tarihi")]
		public DateTime OrderDate { get; set; } = DateTime.Now;
		public List<OrderLine>? OrderLines { get; set; }
	}
}
