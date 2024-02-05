using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=(localdb)\\mssqllocaldb;Database=SCRIN;Trusted_Connection=True;MultipleActiveResultSets=true";

        Console.Write("Введите путь к файлу XML: ");
        string filePath = Console.ReadLine();
        XmlDocument doc = new XmlDocument();


        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                doc.Load(filePath);

                foreach (XmlNode orderNode in doc.SelectNodes("/orders/order"))
                {
                    int orderNo = int.Parse(orderNode.SelectSingleNode("no").InnerText);
                    DateTime regDate = DateTime.Parse(orderNode.SelectSingleNode("reg_date").InnerText);
                    decimal orderSum = decimal.Parse(ConvertToCommaSeparated(orderNode.SelectSingleNode("sum").InnerText));

                    string fio = orderNode.SelectSingleNode("user/fio").InnerText;
                    string email = orderNode.SelectSingleNode("user/email").InnerText;

                    int userId = InsertUser(connection, fio, email, transaction, orderNo);

                    InsertOrder(connection, orderNo, userId, regDate, orderSum, transaction);

                    if (orderNode.SelectNodes("product").Count > 0)
                    {
                        foreach (XmlNode productNode in orderNode.SelectNodes("product"))
                        {
                            int quantity = int.Parse(productNode.SelectSingleNode("quantity").InnerText);
                            string productName = productNode.SelectSingleNode("name").InnerText;
                            decimal price = decimal.Parse(ConvertToCommaSeparated(productNode.SelectSingleNode("price").InnerText));

                            int productId = InsertProduct(connection, productName, price, transaction, orderNo);
                            
                            InsertOrderList(connection, orderNo, productId, quantity, transaction);
                        }
                    }
                    else
                    {
                        throw new Exception($"В заказе под номером {orderNo} отсутсвуют товары");
                    }
                }

                transaction.Commit();
                Console.WriteLine("Данные успешно загружены в базу данных.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
    //Метод для добавления пользователей
    static int InsertUser(SqlConnection connection, string fio, string email, SqlTransaction transaction, int orderNo)
    {
        int userId = GetUserByFioAndEmail(connection, fio, email, transaction);

        if (userId == -1)
        {
            if (!(string.IsNullOrEmpty(fio) || string.IsNullOrEmpty(email)))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO [User] (username, [mail]) VALUES (@username, @mail) SELECT SCOPE_IDENTITY()", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@username", fio);
                    cmd.Parameters.AddWithValue("@mail", email);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            else
            {
                throw new Exception($"Ошибка при добавлении пользователя в заказе номер {orderNo}");
            }
        }
        else
        {
            return userId;
        }
    }

    //Метод для добавления заказа
    static void InsertOrder(SqlConnection connection, int orderNo, int userId, DateTime regDate, decimal orderSum, SqlTransaction transaction)
    {
        try
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO [Order] (orderId, user_userId, OrderDate, [value]) VALUES (@orderNo, @userId, @regDate, @orderSum) SELECT SCOPE_IDENTITY()", connection, transaction))
            {
                cmd.Parameters.AddWithValue("@orderNo", orderNo);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@regDate", regDate);
                cmd.Parameters.AddWithValue("@orderSum", orderSum);

                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {

            throw new Exception($"Ошибка добаления заказа номер {orderNo}: {ex.Message}");
        }
    }
    //Метод для добавления товаров
    static int InsertProduct(SqlConnection connection, string productName, decimal price, SqlTransaction transaction, int orderNo)
    {
        int productId = GetProductByName(connection, productName, transaction);

        if (productId == -1)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO product (productName, [price]) VALUES (@productName, @price) SELECT SCOPE_IDENTITY()", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@price", price);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            else
            {
                throw new Exception($"Ошибка при добавлении товара в заказе под номером {orderNo}");
            }
        }

        return productId;
    }
    //Метод для вставки товаров в заказ
    static void InsertOrderList(SqlConnection connection, int orderId, int productId, int quantity, SqlTransaction transaction)
    {
        try
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO orderlist (order_orderId, product_productId, [count]) VALUES (@orderId, @productId, @quantity)", connection, transaction))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@quantity", quantity);

                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {

            throw new Exception($" Ошибка при добавлении товаров в заказ {ex.Message}");
        }
        
    }

    static string ConvertToCommaSeparated(string number)
    {
        if (number.Contains("."))
        {
            number = number.Replace(".", ",");
        }

        return number;
    }
    //Метод для проверки существования пользователя по имени и email'у
    static int GetUserByFioAndEmail(SqlConnection connection, string fio, string email, SqlTransaction transaction)
    {
        try
        {
            using (SqlCommand cmd = new SqlCommand("SELECT userid FROM [User] WHERE username = @username AND [mail] = @mail", connection, transaction))
            {
                cmd.Parameters.AddWithValue("@username", fio);
                cmd.Parameters.AddWithValue("@mail", email);

                object result = cmd.ExecuteScalar();

                return (result != null) ? Convert.ToInt32(result) : -1;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при обработке пользователя: {ex.Message}");
        }
    }

    //Метод для проверки существования товара по названию
    static int GetProductByName(SqlConnection connection, string productName, SqlTransaction transaction)
    {
        try
        {
            using (SqlCommand cmd = new SqlCommand("SELECT productid FROM product WHERE productname = @productName", connection, transaction))
            {
                cmd.Parameters.AddWithValue("@productName", productName);

                object result = cmd.ExecuteScalar();

                return (result != null) ? Convert.ToInt32(result) : -1;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при обработке товара: {ex.Message}");
        }
    }
    //Метод для проверки существования заказа. Не используется в связи с тем, что
    //при существовании заказа база данных сама выкинет исключение
    static bool OrderExists(SqlConnection connection, int orderNo, SqlTransaction transaction)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [Order] WHERE orderId = @orderNo", connection, transaction))
        {
            cmd.Parameters.AddWithValue("@orderNo", orderNo);
            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}
