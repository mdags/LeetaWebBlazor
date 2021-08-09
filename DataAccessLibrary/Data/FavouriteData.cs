using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface IFavouriteData
    {
        Task Delete(FavouriteModel model);
        Task<List<FavouriteModel>> GetList();
        Task<List<FavouriteModel>> GetList(string[] args1, object[] args2);
        Task<int> IsFavourite(int product_id, int user_id);
        Task Save(FavouriteModel model);
    }

    public class FavouriteData : IFavouriteData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_favourites";

        public FavouriteData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<FavouriteModel>> GetList()
        {
            string sql = string.Format("select * from [{0}]", tableName);

            return db.LoadData<FavouriteModel, dynamic>(sql, new { });
        }

        public Task<List<FavouriteModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select *, (select [name] from [tbl_products] where [id] = [tbl_favourites].[product_id]) as [product_name], (select [img_path] from [tbl_products] where [id] = [tbl_favourites].[product_id]) as [product_img_path], (select [unit_price] from [tbl_products] where [id] = [tbl_favourites].[product_id]) as [product_price], (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol] from [{0}] where 1 = 1 {1}", tableName, where);

            return db.LoadData<FavouriteModel, dynamic>(sql, parameters);
        }

        public Task Save(FavouriteModel model)
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

        public Task Delete(FavouriteModel model)
        {
            string sql = string.Format("delete [{0}] where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public async Task<int> IsFavourite(int product_id, int user_id)
        {
            int result = 0;
            string sql = string.Format("select [id] from [{0}] where [product_id] = @product_id and [user_id] = @user_id", tableName);

            var data = await db.LoadItem<FavouriteModel, dynamic>(sql, new { product_id, user_id });
            if (data != null) result = 1;
            return result;
        }
    }
}
