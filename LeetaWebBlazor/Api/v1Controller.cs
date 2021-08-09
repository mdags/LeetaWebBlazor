using DataAccessLibrary;
using DataAccessLibrary.Models;
using LeetaWebBlazor.Editors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LeetaWebBlazor.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class v1Controller : ControllerBase
    {
        private readonly ISqlDataAccess _sqlDataAccess;
        private readonly AppSettings _appSettings;
        public v1Controller(ISqlDataAccess sqlDataAccess, IOptions<AppSettings> appSettings)
        {
            _sqlDataAccess = sqlDataAccess;
            _appSettings = appSettings.Value;
        }

        public async Task<IActionResult> MemberLogin(string username, string password)
        {
            var db = new MemberData(_sqlDataAccess);
            var model = new MemberModel() { user_phone = username, user_password = password };
            var data = await db.Login(model);
            if (data != null)
            {
                data.img_path = _appSettings.DocURL + data.img_path;
                return Ok(data);
            }
            else
            {
                return StatusCode(500, new { error = "err:Username or password is wrong." });
            }
        }

        public async Task<IActionResult> GetMemberProfile(string userId)
        {
            var db = new MemberData(_sqlDataAccess);
            var data = await db.GetItem(Convert.ToInt32(userId));
            data.img_path = _appSettings.DocURL + data.img_path;
            return Ok(data);
        }

        public async Task<IActionResult> UpdateMemberProfile(MemberModel model)
        {
            try
            {
                var db = new MemberData(_sqlDataAccess);
                await db.Save(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> ChangeMemberPassword(string userId, string oldPassword, string newPassword)
        {
            try
            {
                var db = new MemberData(_sqlDataAccess);
                var model = new MemberModel() { id = Convert.ToInt32(userId), user_name = oldPassword, user_password = newPassword };
                await db.ChangePassword(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> UpdateProfilePhoto(string userId)
        {
            try
            {
                var file = Request.Form.Files[0];
                var path = Path.GetFullPath("wwwroot\\uploads\\") + file.Name;
                using (var stream = System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }

                var db = new MemberData(_sqlDataAccess);
                var model = new MemberModel() { id = Convert.ToInt32(userId), img_path = file.Name };
                await db.Save(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> GetAddress(string userId)
        {
            AddressData db = new AddressData(_sqlDataAccess);
            var data = await db.GetList(new string[] { "user_id" }, new object[] { Convert.ToInt32(userId) });
            return Ok(data);
        }

        public async Task<IActionResult> AddAddress(string userId, string adrName, string fullAddress)
        {
            try
            {
                AddressData db = new AddressData(_sqlDataAccess);
                AddressModel model = new AddressModel()
                {
                    user_id = Convert.ToInt32(userId),
                    adr_name = adrName,
                    full_address = fullAddress
                };
                await db.Save(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> DelAddress(string id)
        {
            try
            {
                AddressData db = new AddressData(_sqlDataAccess);
                AddressModel model = new AddressModel()
                {
                    id = Convert.ToInt32(id)
                };
                await db.Delete(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> GetCategories()
        {
            CategoryData db = new CategoryData(_sqlDataAccess);
            var data = await db.GetList();
            foreach (var item in data)
            {
                item.img_path = _appSettings.DocURL + item.img_path;
            }
            return Ok(data);
        }

        public async Task<IActionResult> GetProducts(string userId, string catId)
        {
            ProductData db = new ProductData(_sqlDataAccess);
            FavouriteData favDb = new FavouriteData(_sqlDataAccess);
            CartData cartDb = new CartData(_sqlDataAccess);
            var data = await db.GetList(new string[] { "cat_id" }, new object[] { Convert.ToInt32(catId) });
            foreach (var item in data)
            {
                item.img_path = _appSettings.DocURL + item.img_path;
                item.is_favourite = await favDb.IsFavourite(item.id, Convert.ToInt32(userId));
                item.cart_count = await cartDb.GetCartCount(Convert.ToInt32(userId), item.id);
            }
            return Ok(data);
        }

        public async Task<IActionResult> GetProductById(string userId, string id)
        {
            ProductData db = new ProductData(_sqlDataAccess);
            FavouriteData favDb = new FavouriteData(_sqlDataAccess);
            CartData cartDb = new CartData(_sqlDataAccess);
            var data = await db.GetList(new string[] { "id" }, new object[] { Convert.ToInt32(id) });
            foreach (var item in data)
            {
                item.img_path = _appSettings.DocURL + item.img_path;
                item.is_favourite = await favDb.IsFavourite(item.id, Convert.ToInt32(userId));
                item.cart_count = await cartDb.GetCartCount(Convert.ToInt32(userId), item.id);
            }
            return Ok(data);
        }

        public async Task<IActionResult> GetProductForSearch(string userId)
        {
            ProductData db = new ProductData(_sqlDataAccess);
            FavouriteData favDb = new FavouriteData(_sqlDataAccess);
            CartData cartDb = new CartData(_sqlDataAccess);
            var data = await db.GetList();
            foreach (var item in data)
            {
                item.img_path = _appSettings.DocURL + item.img_path;
                item.is_favourite = await favDb.IsFavourite(item.id, Convert.ToInt32(userId));
                item.cart_count = await cartDb.GetCartCount(Convert.ToInt32(userId), item.id);
            }
            return Ok(data);
        }

        public async Task<IActionResult> GetFavourites(string userId)
        {
            FavouriteData db = new FavouriteData(_sqlDataAccess);
            var data = await db.GetList(new string[] { "user_id" }, new object[] { Convert.ToInt32(userId) });
            foreach (var item in data)
            {
                item.product_img_path = _appSettings.DocURL + item.product_img_path;
            }
            return Ok(data);
        }

        public async Task<IActionResult> SetFavourite(string userId, string productId)
        {
            FavouriteData db = new FavouriteData(_sqlDataAccess);
            ProductData productDb = new ProductData(_sqlDataAccess);

            var data = await db.GetList(new string[] { "product_id", "user_id" }, new object[] { Convert.ToInt32(productId), Convert.ToInt32(userId) });
            if (data.Count == 0)
            {
                var model = new FavouriteModel() { product_id = Convert.ToInt32(productId), user_id = Convert.ToInt32(userId), created_user_id = Convert.ToInt32(userId), updated_user_id = Convert.ToInt32(userId) };
                await db.Save(model);
                await productDb.SetFavouriteCount(new ProductModel() { id = Convert.ToInt32(productId) }, "insert");
            }
            else
            {
                await db.Delete(new FavouriteModel() { id = data[0].id });
                await productDb.SetFavouriteCount(new ProductModel() { id = Convert.ToInt32(productId) }, "delete");
            }
            return Ok();
        }

        public async Task<IActionResult> GetCartItems(string userId)
        {
            CartData db = new CartData(_sqlDataAccess);
            var data = await db.GetList(new string[] { "user_id" }, new object[] { Convert.ToInt32(userId) });
            foreach (var item in data)
            {
                item.product_img_path = _appSettings.DocURL + item.product_img_path;
            }
            return Ok(data);
        }

        public async Task<IActionResult> SetCart(string userId, string productId, string qty)
        {
            try
            {
                CartData db = new CartData(_sqlDataAccess);
                var data = await db.GetList(new string[] { "user_id", "product_id" }, new object[] { Convert.ToInt32(userId), Convert.ToInt32(productId) });
                if (data.Count == 0)
                {
                    CartModel model = new CartModel()
                    {
                        user_id = Convert.ToInt32(userId),
                        product_id = Convert.ToInt32(productId),
                        qty = Convert.ToInt32(qty)
                    };
                    await db.Save(model);
                }
                else
                {
                    CartModel model = new CartModel()
                    {
                        id = data[0].id,
                        qty = Convert.ToInt32(qty),
                        updated_date = DateTime.Now.ToString(),
                        updated_user_id = Convert.ToInt32(userId)
                    };
                    if (Convert.ToInt32(qty) > 0)
                    {
                        await db.Save(model);
                    }
                    else
                    {
                        await db.Delete(model);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> RemoveFromCart(string id)
        {
            try
            {
                CartData db = new CartData(_sqlDataAccess);
                CartModel model = new CartModel()
                {
                    id = Convert.ToInt32(id)
                };
                await db.Delete(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> ClearCart(string userId)
        {
            try
            {
                CartData db = new CartData(_sqlDataAccess);
                CartModel model = new CartModel()
                {
                    user_id = Convert.ToInt32(userId)
                };
                await db.Clear(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }

        public async Task<IActionResult> GetOrders(string userId)
        {
            OrderData db = new OrderData(_sqlDataAccess);
            OrderDetailData detailDb = new OrderDetailData(_sqlDataAccess);

            var data = await db.GetList(new string[] { "user_id" }, new object[] { Convert.ToInt32(userId) });
            foreach (var order in data)
            {
                order.order_details = await detailDb.GetList(new string[] { "order_id" }, new object[] { order.id });
                foreach (var item in order.order_details)
                {
                    item.product_img_path = _appSettings.DocURL + item.product_img_path;
                }
            }

            return Ok(data);
        }

        public async Task<IActionResult> CompleteOrder(OrderModel model)
        {
            try
            {
                OrderData db = new OrderData(_sqlDataAccess);
                await db.CompleteOrder(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "err:" + ex.Message });
            }
        }
    }
}
