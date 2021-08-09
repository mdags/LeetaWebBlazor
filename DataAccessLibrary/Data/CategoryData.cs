using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface ICategoryData
    {
        Task<List<CategoryModel>> GetList();
        Task Save(CategoryModel category);
        Task Delete(CategoryModel category);
    }
    public class CategoryData : ICategoryData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_categories";

        public CategoryData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<CategoryModel>> GetList()
        {
            string sql = string.Format("select * from [{0}]", tableName);

            return db.LoadData<CategoryModel, dynamic>(sql, new { });
        }

        public Task<List<CategoryModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select * from [{0}] where 1 = 1 {1}", tableName, where);

            return db.LoadData<CategoryModel, dynamic>(sql, parameters);
        }

        public Task Save(CategoryModel model)
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

        public Task Delete(CategoryModel model)
        {
            string sql = string.Format("delete [{0}] where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }
    }
}
