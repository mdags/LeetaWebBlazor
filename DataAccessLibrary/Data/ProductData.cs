using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface IProductData
    {
        Task Delete(ProductModel model);
        Task<List<ProductModel>> GetList();
        Task Save(ProductModel model);
        Task SetFavouriteCount(ProductModel model, string type);
    }

    public class ProductData : IProductData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_products";

        public ProductData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<ProductModel>> GetList()
        {
            string sql = string.Format("select *, (select [name] from [tbl_categories] where [id] = [tbl_products].[cat_id]) as [cat_name], (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol] from [{0}] where [status] = 1", tableName);

            return db.LoadData<ProductModel, dynamic>(sql, new { });
        }

        public Task<List<ProductModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select *, (select [name] from [tbl_categories] where [id] = [tbl_products].[cat_id]) as [cat_name], (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol] from [{0}] where [status] = 1 {1}", tableName, where);

            return db.LoadData<ProductModel, dynamic>(sql, parameters);
        }

        public Task Save(ProductModel model)
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

        public Task Delete(ProductModel model)
        {
            string sql = string.Format("update [{0}] set [status] = 0 where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public Task SetFavouriteCount(ProductModel model, string type)
        {
            string sql = string.Format("update [{0}] set [favourite_count] = [favourite_count] + 1 where [id] = @id", tableName);
            if (type == "delete") sql = string.Format("update [{0}] set [favourite_count] = [favourite_count] - 1 where [id] = @id", tableName);
            return db.SaveData(sql, model);
        }
    }
}
