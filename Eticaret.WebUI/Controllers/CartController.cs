using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Eticaret.WebUI.ExtensionMethods;
using Eticaret.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Eticaret.WebUI.Controllers
{
	public class CartController : Controller
	{
		private readonly IService<Product> _serviceProduct;
		private readonly IService<AppUser> _serviceAppUser;
		private readonly IService<Core.Entities.Address> _serviceAddress;
		private readonly IService<Order> _serviceOrder;
		private readonly IConfiguration _configuration;
		public CartController(IService<Product> serviceProduct, IService<Core.Entities.Address> serviceAddress, IService<AppUser> serviceAppUser, IService<Order> serviceOrder, IConfiguration configuration)
		{
			_serviceProduct = serviceProduct;
			_serviceAddress = serviceAddress;
			_serviceAppUser = serviceAppUser;
			_serviceOrder = serviceOrder;
			_configuration = configuration;
		}
		public IActionResult Index()
		{
			var cart = GetCart();
			var model = new CartViewModel()
			{
				CartLines = cart.CartLines,
				TotalPrice = cart.TotalPrice()
			};
			return View(model);
		}
		public IActionResult Add(int ProductId, int quantity = 1)
		{
			var product = _serviceProduct.Find(ProductId);
			if (product != null)
			{
				var cart = GetCart();
				cart.AddProduct(product, quantity);
				HttpContext.Session.SetJson("Cart", cart);
				return Redirect(Request.Headers["Referer"].ToString());//Buraya gelmeden bir önceki sayfaya yönlendirir.
			}
			return RedirectToAction("Index");
		}
		public IActionResult Update(int ProductId, int quantity = 1)
		{
			var product = _serviceProduct.Find(ProductId);
			if (product != null)
			{
				var cart = GetCart();
				cart.UpdateProduct(product, quantity);
				HttpContext.Session.SetJson("Cart", cart);
			}
			return RedirectToAction("Index");
		}
		public IActionResult Remove(int ProductId)
		{
			var product = _serviceProduct.Find(ProductId);
			if (product != null)
			{
				var cart = GetCart();
				cart.RemoveProduct(product);
				HttpContext.Session.SetJson("Cart", cart);
			}
			return RedirectToAction("Index");
		}
		[Authorize]
		public async Task<IActionResult> Checkout()
		{
			var cart = GetCart();
			var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
			if (appUser == null)
			{
				return RedirectToAction("SignIn", "Account");
			}
			var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
			var model = new CheckoutViewModel()
			{
				CartProducts = cart.CartLines,
				TotalPrice = cart.TotalPrice(),
				Addresses = addresses
			};

			return View(model);
		}
		[Authorize, HttpPost]
		public async Task<IActionResult> Checkout(string CardNumber, string CardNameSurname, string CardMonth, string CardYear, string CVV,
			string DeliveryAddress, string BillingAddress)
		{
			var cart = GetCart();
			var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
			if (appUser == null)
			{
				return RedirectToAction("SignIn", "Account");
			}
			var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
			var model = new CheckoutViewModel()
			{
				CartProducts = cart.CartLines,
				TotalPrice = cart.TotalPrice(),
				Addresses = addresses
			};
			if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CardMonth) || string.IsNullOrWhiteSpace(CardYear)
				|| string.IsNullOrWhiteSpace(CVV) || string.IsNullOrWhiteSpace(DeliveryAddress) || string.IsNullOrWhiteSpace(BillingAddress))
			{
				return View(model);
			}
			var faturaAdresi = addresses.FirstOrDefault(a => a.AddressGuid.ToString() == BillingAddress);
			var teslimatAdresi = addresses.FirstOrDefault(a => a.AddressGuid.ToString() == DeliveryAddress);

			//Ödeme çekme

			var siparis = new Order()
			{
				AppUserId = appUser.Id,
				BillingAddress = $"{faturaAdresi.OpenAddress} {faturaAdresi.District} {faturaAdresi.City}", //BillingAddress,
				CustomerId = appUser.UserGuid.ToString(),
				DeliveryAddress = $"{faturaAdresi.OpenAddress} {faturaAdresi.District} {faturaAdresi.City}",//DeliveryAddress,
				TotalPrice = cart.TotalPrice(),
				OrderNumber = Guid.NewGuid().ToString(),
				OrderDate = DateTime.Now,
				OrderLines = []
			};

			#region OdemeIslemi
			Options options = new Options();
			options.ApiKey = _configuration["IyzicOptions:ApiKey"];
			options.SecretKey = _configuration["IyzicOptions:SecretKey"];
			options.BaseUrl = _configuration["IyzicOptions:BaseUrl"];

			CreatePaymentRequest request = new CreatePaymentRequest();
			request.Locale = Locale.TR.ToString();
			request.ConversationId = HttpContext.Session.Id;
			request.Price = siparis.TotalPrice.ToString().Replace(",", ".");
			request.PaidPrice = siparis.TotalPrice.ToString().Replace(",", ".");
			request.Currency = Currency.TRY.ToString();
			request.Installment = 1;
			request.BasketId = "B" + HttpContext.Session.Id;
			request.PaymentChannel = PaymentChannel.WEB.ToString();
			request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

			PaymentCard paymentCard = new PaymentCard();
			paymentCard.CardHolderName = CardNameSurname; //"John Doe";
			paymentCard.CardNumber = CardNumber; //"5528790000000008";
			paymentCard.ExpireMonth = CardMonth; //"12";
			paymentCard.ExpireYear = CardYear; //"2030";
			paymentCard.Cvc = CVV; //"123";
			paymentCard.RegisterCard = 0;
			request.PaymentCard = paymentCard;

			Buyer buyer = new Buyer();
			buyer.Id = "BY" + appUser.Id;
			buyer.Name = appUser.Name;
			buyer.Surname = appUser.Surname;
			buyer.GsmNumber = appUser.Phone;
			buyer.Email = appUser.Email;
			buyer.IdentityNumber = "11111111111";
			buyer.LastLoginDate = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss"); //"2015-10-05 12:43:35";
			buyer.RegistrationDate = appUser.CreateDate.ToString("yyyy-mm-dd hh:mm:ss");// "2013-04-21 15:12:09";
			buyer.RegistrationAddress = siparis.DeliveryAddress;
			buyer.Ip = HttpContext.Connection.RemoteIpAddress?.ToString();
			buyer.City = teslimatAdresi.City;
			buyer.Country = "Turkey";
			buyer.ZipCode = "34732";
			request.Buyer = buyer;

			var shippingAddress = new Iyzipay.Model.Address();
			shippingAddress.ContactName = appUser.Name + " " + appUser.Surname;
			shippingAddress.City = teslimatAdresi.City;
			shippingAddress.Country = "Turkey";
			shippingAddress.Description = teslimatAdresi.OpenAddress;
			shippingAddress.ZipCode = "34742";
			request.ShippingAddress = shippingAddress;

			var billingAddress = new Iyzipay.Model.Address();
			billingAddress.ContactName = appUser.Name + " " + appUser.Surname;
			billingAddress.City = teslimatAdresi.City;
			billingAddress.Country = "Turkey";
			billingAddress.Description = teslimatAdresi.OpenAddress;
			billingAddress.ZipCode = "34742";
			request.BillingAddress = billingAddress;

			List<BasketItem> basketItems = new List<BasketItem>();

			foreach (var item in cart.CartLines)
			{
				siparis.OrderLines.Add(new OrderLine
				{
					ProductId = item.Product.Id,
					OrderId = siparis.Id,
					Quantity = item.Quantity,
					UnitPrice = item.Product.Price,
				});
				basketItems.Add(new BasketItem
				{
					Id = item.Product.Id.ToString(),
					Name = item.Product.Name,
					Category1 = "Collectibles",
					ItemType = BasketItemType.PHYSICAL.ToString(),
					Price = (item.Product.Price * item.Quantity).ToString().Replace(",", ".")
				});
			}

			if (siparis.TotalPrice<999)
			{
				basketItems.Add(new BasketItem
				{
					Id = "Kargo",
					Name ="Kargo Ücreti",
					Category1 = "Kargo Ücreti",
					ItemType = BasketItemType.VIRTUAL.ToString(),
					Price = "99"
				});
				siparis.TotalPrice += 99;
				request.Price = siparis.TotalPrice.ToString().Replace(",", ".");
				request.PaidPrice = siparis.TotalPrice.ToString().Replace(",", ".");
			}

			request.BasketItems = basketItems;

			Payment payment = await Payment.Create(request, options);

			#endregion
			try
			{
				if (payment.Status == "success")
				{
					await _serviceOrder.AddAsync(siparis);
					var sonuc = await _serviceOrder.SaveChangesAsync();
					if (sonuc > 0)
					{
						HttpContext.Session.Remove("Cart");
						return RedirectToAction("Thanks");
					}
				}
				else
				{
					TempData["Message"] = "<div class='alert alert-danger'>Siparişiniz alınamadı. Lütfen tekrar deneyin!</div>" +
						$"({payment.ErrorMessage})";
				}

			}
			catch (Exception)
			{

				TempData["Message"] = "<div class='alert alert-danger'>Bir hata oluştu. Lütfen tekrar deneyin.</div>";
			}
			return View(model);
		}
		public IActionResult Thanks()
		{
			return View();
		}
		private CartService GetCart()
		{
			return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
		}

	}
}
