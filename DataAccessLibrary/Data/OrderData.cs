using Dapper;
using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface IOrderData
    {
        Task CompleteOrder(OrderModel model);
        Task Delete(OrderModel model);
        Task<List<OrderModel>> GetList();
        Task<List<OrderModel>> GetList(string[] args1, object[] args2);
        Task Save(OrderModel model);
        Task SetStatusAcceptAll(OrderModel model);
        Task SetStatusArrivingAll(OrderModel model);
        Task SetStatusDeliveredAll(OrderModel model);
        Task SetStatusPreparingAll(OrderModel model);
        Task SetStatusRejectAll(OrderModel model);
        Task<double> TotalOrderEarning();
    }

    public class OrderData : IOrderData
    {
        private readonly ISqlDataAccess db;
        private const string tableName = "tbl_orders";

        public OrderData(ISqlDataAccess db)
        {
            this.db = db;
        }

        public Task<List<OrderModel>> GetList()
        {
            string sql = string.Format("select *, (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol] from [{0}]", tableName);

            return db.LoadData<OrderModel, dynamic>(sql, new { });
        }

        public Task<List<OrderModel>> GetList(string[] args1, object[] args2)
        {
            string where = string.Empty;
            var parameters = new DynamicParameters();
            for (int i = 0; i < args1.Length; i++)
            {
                parameters.Add(args1[i], args2[i]);
                where += string.Format(" and [{0}] = @{0}", args1[i]);
            }
            string sql = string.Format("select *, (select top 1 [currency_symbol] from [core_backend_config]) as [currency_symbol] from [{0}] where 1 = 1 {1}", tableName, where);

            return db.LoadData<OrderModel, dynamic>(sql, parameters);
        }

        public Task Save(OrderModel model)
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

        public Task Delete(OrderModel model)
        {
            string sql = string.Format("delete [{0}] where [id] = @id", tableName);

            return db.SaveData(sql, model);
        }

        public Task CompleteOrder(OrderModel model)
        {
            string sql = @"BEGIN TRAN
                    declare @order_id int 

                    insert into[tbl_orders]([user_id], [sub_total_amount], [discount_amount], [tax_amount], [tax_percent], [shipping_amount], [total_amount], [total_item_count], [contact_name], [contact_phone], [contact_email], [user_address_id], [contact_address], [delivery_type], [payment_method], [order_note], [created_user_id], [updated_user_id]) values(@user_id, 0, 0, 0, 0, @shipping_amount, 0, 0, (select[user_name] from [tbl_members] where [id] = @user_id), (select[user_phone] from [tbl_members] where [id] = @user_id), (select[user_email] from [tbl_members] where [id] = @user_id), @user_address_id, (select [full_address] from [tbl_member_address] where [id] = @user_address_id), @delivery_type, @payment_method, @order_note, @user_id, @user_id)
	                select @order_id = SCOPE_IDENTITY() 

                    insert into[tbl_order_details]([order_id], [order_status_id], [product_id], [qty], [unit_price], [discount_amount], [discount_percent], [total_amount], [created_user_id], [updated_user_id]) select @order_id, 1, [product_id], [qty], (select[unit_price] from[tbl_products] where[id] = [tbl_cart].[product_id]), 0, 0, [qty] *(select[unit_price] from[tbl_products] where[id] = [tbl_cart].[product_id]), @user_id, @user_id from[tbl_cart] where[user_id] = @user_id 

                    update[tbl_orders] set[sub_total_amount] = (select sum([total_amount]) from[tbl_order_details] where[order_id] = @order_id), [total_amount] = [shipping_amount] + (select sum([total_amount]) from[tbl_order_details] where[order_id] = @order_id), [total_item_count] = (select count([id]) from[tbl_order_details] where[order_id] = @order_id) where[id] = @order_id 

                    insert into[tbl_order_logs]([order_detail_id], [order_status_id], [created_user_id], [updated_user_id]) select[id], 1, @user_id, @user_id from[tbl_order_details] where[order_id] = @order_id 

                    delete[tbl_cart] where[user_id] = @user_id
                IF @@ROWCOUNT = 1
                  COMMIT TRAN
                ELSE
                 ROLLBACK TRAN";

            return db.SaveData(sql, model);
        }

        public Task SetStatusAcceptAll(OrderModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 2 where [order_id] = @id" +
                        "insert into [tbl_order_logs]([order_detail_id], [order_status_id]) select [id], 2 from [tbl_order_details] where [order_id] = @id";

            return db.SaveData(sql, model);
        }

        public Task SetStatusPreparingAll(OrderModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 3 where [order_id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) select [id], 3 from [tbl_order_details] where [order_id] = @id";

            return db.SaveData(sql, model);
        }

        public Task SetStatusArrivingAll(OrderModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 4 where [order_id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) select [id], 4 from [tbl_order_details] where [order_id] = @id";

            return db.SaveData(sql, model);
        }

        public Task SetStatusDeliveredAll(OrderModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 5 where [order_id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) select [id], 5 from [tbl_order_details] where [order_id] = @id";

            return db.SaveData(sql, model);
        }

        public Task SetStatusRejectAll(OrderModel model)
        {
            string sql = "update [tbl_order_details] set [order_status_id] = 6 where [order_id] = @id" +
                        " insert into [tbl_order_logs]([order_detail_id], [order_status_id]) select [id], 6 from [tbl_order_details] where [order_id] = @id";

            return db.SaveData(sql, model);
        }

        public async Task<double> TotalOrderEarning()
        {
            double result = 0;
            string sql = string.Format("select sum([sub_total_amount]) as [sub_total_amount] from [{0}]", tableName);

            var data = await db.LoadItem<OrderModel, dynamic>(sql, new { });
            if (data != null) result = data.sub_total_amount;
            return result;
        }
    }
}
