using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface IMemberData
    {
        Task Delete(MemberModel model);
        Task<List<MemberModel>> GetList();
        Task<MemberModel> GetItem(int id);
        Task<MemberModel> Login(MemberModel model);
        Task Save(MemberModel model);
        Task ChangePassword(MemberModel model);
    }

    public class MemberData : IMemberData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_members";

        public MemberData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<MemberModel>> GetList()
        {
            string sql = string.Format("select * from [{0}] where [status] = 1", tableName);

            return db.LoadData<MemberModel, dynamic>(sql, new { });
        }

        public Task<List<MemberModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select * from [{0}] where [status] = 1 {1}", tableName, where);

            return db.LoadData<MemberModel, dynamic>(sql, parameters);
        }

        public Task<MemberModel> GetItem(int id)
        {
            string sql = string.Format("select * from [{0}] where [id] = @id", tableName);

            return db.LoadItem<MemberModel, dynamic>(sql, new { id });
        }

        public Task Save(MemberModel model)
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

        public Task Delete(MemberModel model)
        {
            string sql = string.Format("update [{0}] set [status] = 0 where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public Task<MemberModel> Login(MemberModel model)
        {
            string sql = string.Format("select * from [{0}] where [user_phone] = @user_phone and [user_password] = @user_password", tableName);

            return db.LoadItem<MemberModel, dynamic>(sql, model);
        }

        public Task ChangePassword(MemberModel model)
        {
            string sql = string.Format("if exists(select [id] from [{0}] where [id] = 1 and [user_password] = @user_name) begin update [{0}] set [user_password] = @user_password where [id] = @id end else begin RAISERROR('Old password is wrong.', 16, 1) end", tableName);

            return db.SaveData(sql, model);
        }
    }
}
