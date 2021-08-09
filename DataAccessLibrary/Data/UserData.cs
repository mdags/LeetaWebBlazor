using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface IUserData
    {
        Task Delete(UserModel model);
        Task<List<UserModel>> GetList();
        Task<List<UserModel>> Login(UserModel model);
        Task Save(UserModel model);
    }
    public class UserData : IUserData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "core_users";

        public UserData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<UserModel>> GetList()
        {
            string sql = string.Format("select * from [{0}] where [status] = 1", tableName);

            return db.LoadData<UserModel, dynamic>(sql, new { });
        }

        public Task<List<UserModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select * from [{0}] where [status] = 1 {1}", tableName, where);

            return db.LoadData<UserModel, dynamic>(sql, parameters);
        }

        public Task<UserModel> GetItem(int id)
        {
            string sql = string.Format("select * from [{0}] where [id] = @id", tableName);

            return db.LoadItem<UserModel, dynamic>(sql, new { id });
        }

        public Task Save(UserModel model)
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

        public Task Delete(UserModel model)
        {
            string sql = string.Format("update [{0}] set [status] = 0 where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public Task<List<UserModel>> Login(UserModel model)
        {
            string sql = string.Format("select * from [{0}] where [user_name] = @user_name and [user_password] = @user_password", tableName);

            return db.LoadData<UserModel, dynamic>(sql, model);
        }
    }
}
