using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface ICartData
    {
        Task Clear(CartModel model);
        Task Delete(CartModel model);
        Task<int> GetCartCount(int user_id, int product_id);
        Task<List<CartModel>> GetList();
        Task<List<CartModel>> GetList(string[] args1, object[] args2);
        Task Save(CartModel model);
        Task<int> WaitingInCartCount();
    }

    public class CartData : ICartData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_cart";

        public CartData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<CartModel>> GetList()
        {
            string sql = string.Format("select * from [{0}]", tableName);

            return db.LoadData<CartModel, dynamic>(sql, new { });
        }

        public Task<List<CartModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select *, (select [name] from [tbl_products] where [id] = [tbl_cart].[product_id]) as [product_name], (select [img_path] from [tbl_products] where [id] = [tbl_cart].[product_id]) as [product_img_path], (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol], isnull([qty] * (select [unit_price] from [tbl_products] where [id] = [tbl_cart].[product_id]), 0) as [total_amount] from [{0}] where 1 = 1 {1}", tableName, where);

            return db.LoadData<CartModel, dynamic>(sql, parameters);
        }

        public Task Save(CartModel model)
        {
            string sql = string.Empty;
            if (model.id == 0)
            {
                string cols = string.Empty, vals = string.Empty;
                foreach (var prop in model.GetType().GetProperties())
                {
                    object[] keys = prop.GetCustomAttributes(typeof(KeyAttribute), false);
                    object[] editables = prop.GetCustomAttributes(typeof(EditableAttribute), false);
                    var value = prop.GetValue(model, null);
                    if (keys.Length == 0 && editables.Length == 0 && value != null)
                    {
                        cols += string.Format("[{0}], ", prop.Name);
                        vals += string.Format("@{0}, ", prop.Name);
                    }
                }
                sql = string.Format("insert into [{0}]({1}) values({2})", tableName, cols.Substring(0, cols.Length - 2), vals.Substring(0, vals.Length - 2));
            }
            else
            {
                string updatedFields = string.Empty;
                foreach (var prop in model.GetType().GetProperties())
                {
                    object[] keys = prop.GetCustomAttributes(typeof(KeyAttribute), false);
                    object[] editables = prop.GetCustomAttributes(typeof(EditableAttribute), false);
                    var value = prop.GetValue(model, null);
                    if (keys.Length == 0 && editables.Length == 0 && value != null)
                    {
                        updatedFields += string.Format("[{0}] = @{0}, ", prop.Name);
                    }
                }
                sql = string.Format("update [{0}] set {1} where [id] = @id", tableName, updatedFields.Substring(0, updatedFields.Length - 2));
            }
            return db.SaveData(sql, model);
        }

        public Task Delete(CartModel model)
        {
            string sql = string.Format("delete [{0}] where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public Task Clear(CartModel model)
        {
            string sql = string.Format("delete [{0}] where [user_id] = @user_id", tableName);

            return db.SaveData(sql, model);
        }

        public async Task<int> WaitingInCartCount()
        {
            int result = 0;
            string sql = string.Format("select [qty] from [tbl_cart]", tableName);

            var data = await db.LoadItem<CartModel, dynamic>(sql, new { });
            if (data != null) result = (int)data.qty;
            return result;
        }

        public async Task<int> GetCartCount(int user_id, int product_id)
        {
            int result = 0;
            string sql = string.Format("select [qty] from [tbl_cart] where [user_id] = @user_id and [product_id] = @product_id", tableName);

            var data = await db.LoadItem<CartModel, dynamic>(sql, new { product_id, user_id });
            if (data != null) result = (int)data.qty;
            return result;
        }
    }
}
