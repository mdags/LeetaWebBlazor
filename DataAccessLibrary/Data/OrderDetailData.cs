using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface IOrderDetailData
    {
        Task Delete(OrderDetailModel model);
        Task<List<OrderDetailModel>> GetList(int id);
        Task<List<OrderDetailModel>> GetList(string[] args1, object[] args2);
        Task Save(OrderDetailModel model);
        Task SetStatusAccept(OrderDetailModel model);
        Task SetStatusArriving(OrderDetailModel model);
        Task SetStatusDelivered(OrderDetailModel model);
        Task SetStatusPreparing(OrderDetailModel model);
        Task SetStatusReject(OrderDetailModel model);
        Task<int> TotalOrderCount();
        Task<int> TotalPendingOrderCount();
    }

    public class OrderDetailData : IOrderDetailData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_order_details";

        public OrderDetailData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<OrderDetailModel>> GetList(int id)
        {
            string sql = string.Format("select *, (select [name] from [tbl_products] where [id] = [tbl_order_details].[product_id]) as [product_name], (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol], (select [name] from [tbl_orders_status] where [id] = [tbl_order_details].[order_status_id]) as [order_status_name] from [{0}] where [order_id] = @id", tableName);

            return db.LoadData<OrderDetailModel, dynamic>(sql, new { id });
        }

        public Task<List<OrderDetailModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select *, (select [name] from [tbl_products] where [id] = [{0}].[product_id]) as [product_name], (select [img_path] from [tbl_products] where [id] = [{0}].[product_id]) as [product_img_path], (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol], (select [name] from [tbl_orders_status] where [id] = [{0}].[order_status_id]) as [order_status_name], (select top 1 [created_date] from [tbl_order_logs] where [order_detail_id] = [{0}].[id] and [order_status_id] = 2) as [accepted_date], (select top 1 [created_date] from [tbl_order_logs] where [order_detail_id] = [{0}].[id] and [order_status_id] = 3) as [preparing_date], (select top 1 [created_date] from [tbl_order_logs] where [order_detail_id] = [{0}].[id] and [order_status_id] = 4) as [arriving_date], (select top 1 [created_date] from [tbl_order_logs] where [order_detail_id] = [{0}].[id] and [order_status_id] = 5) as [delivered_date], (select top 1 [created_date] from [tbl_order_logs] where [order_detail_id] = [{0}].[id] and [order_status_id] = 6) as [cancelled_date] from [{0}] where 1 = 1 {1}", tableName, where);

            return db.LoadData<OrderDetailModel, dynamic>(sql, parameters);
        }

        public Task Save(OrderDetailModel model)
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

        public Task Delete(OrderDetailModel model)
        {
            string sql = string.Format("delete [{0}] where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public Task SetStatusAccept(OrderDetailModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 2 where [id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) values(@id, 2)";

            return db.SaveData(sql, model);
        }

        public Task SetStatusPreparing(OrderDetailModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 3 where [id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) values(@id, 3)";

            return db.SaveData(sql, model);
        }

        public Task SetStatusArriving(OrderDetailModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 4 where [id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) values(@id, 4)";

            return db.SaveData(sql, model);
        }

        public Task SetStatusDelivered(OrderDetailModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 5 where [id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) values(@id, 5)";

            return db.SaveData(sql, model);
        }

        public Task SetStatusReject(OrderDetailModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 6 where [id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) values(@id, 6)";

            return db.SaveData(sql, model);
        }

        public async Task<int> TotalOrderCount()
        {
            int result = 0;
            string sql = string.Format("select [id] from [{0}]", tableName);

            var data = await db.LoadData<OrderDetailModel, dynamic>(sql, new { });
            if (data != null) result = data.Count;
            return result;
        }

        public async Task<int> TotalPendingOrderCount()
        {
            int result = 0;
            string sql = string.Format("select [id] from [{0}] where [order_status_id] = 1", tableName);

            var data = await db.LoadData<OrderDetailModel, dynamic>(sql, new { });
            if (data != null) result = data.Count;
            return result;
        }
    }
}
