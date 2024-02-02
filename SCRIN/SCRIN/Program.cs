using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source = (localdb)\\mssqllocaldb;Database=SCRIN;Trusted_Connection=True;MultipleActiveResultSets=true";

        Console.Write("Введите путь к файлу XML: ");
        string filePath = Console.ReadLine();
        XmlDocument doc = new XmlDocument();

        try
        {
            doc.Load(filePath);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (XmlNode orderNode in doc.SelectNodes("/orders/order"))
                {
                    // Извлечение данных о заказе
                    int orderNo = int.Parse(orderNode.SelectSingleNode("no").InnerText);
                    DateTime regDate = DateTime.Parse(orderNode.SelectSingleNode("reg_date").InnerText);
                    decimal orderSum = decimal.Parse(ConvertToCommaSeparated(orderNode.SelectSingleNode("sum").InnerText));

                    // Извлечение данных о пользователе
                    string fio = orderNode.SelectSingleNode("user/fio").InnerText;
                    string email = orderNode.SelectSingleNode("user/email").InnerText;

                    // Вставка данных о пользователе в таблицу "User"
                    int userId = InsertUser(connection, fio, email);

                    // Вставка данных о заказе в таблицу "Order"
                    InsertOrder(connection, orderNo, userId, regDate, orderSum);

                    // Обработка каждого продукта в заказе
                    foreach (XmlNode productNode in orderNode.SelectNodes("product"))
                    {
                        // Извлечение данных о продукте
                        int quantity = int.Parse(productNode.SelectSingleNode("quantity").InnerText);
                        string productName = productNode.SelectSingleNode("name").InnerText;
                        decimal price = decimal.Parse(ConvertToCommaSeparated(productNode.SelectSingleNode("price").InnerText));

                        // Вставка данных о продукте в таблицу "product"
                        int productId = InsertProduct(connection, productName, price);

                        // Вставка данных о продукте в таблицу "orderlist"
                        InsertOrderList(connection, orderNo, productId, quantity);
                    }
                }

                connection.Close();
            }

            Console.WriteLine("Данные успешно загружены в базу данных.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке файла XML: {ex.Message}");
        }
    }

    static int InsertUser(SqlConnection connection, string fio, string email)
    {
        int userId = GetUserByFioAndEmail(connection, fio, email);

        if (userId == -1)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO [User] (username, [mail]) VALUES (@username, @mail) SELECT SCOPE_IDENTITY()", connection))
            {
                cmd.Parameters.AddWithValue("@username", fio);
                cmd.Parameters.AddWithValue("@mail", email);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        return userId;
    }

    static void InsertOrder(SqlConnection connection, int orderNo, int userId, DateTime regDate, decimal orderSum)
    {
        using (SqlCommand cmd = new SqlCommand("INSERT INTO [Order] (orderId, user_userId, OrderDate, [value]) VALUES (@orderNo, @userId, @regDate, @orderSum) SELECT SCOPE_IDENTITY()", connection))
        {
            cmd.Parameters.AddWithValue("@orderNo", orderNo);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@regDate", regDate);
            cmd.Parameters.AddWithValue("@orderSum", orderSum);

            cmd.ExecuteNonQuery();
        }
    }

    static int InsertProduct(SqlConnection connection, string productName, decimal price)
    {
        int productId = GetProductByName(connection, productName);

        if (productId == -1)
        {
            using (SqlCommand cmd = new SqlCommand("INSERT INTO product (productName, [price]) VALUES (@productName, @price) SELECT SCOPE_IDENTITY()", connection))
            {
                cmd.Parameters.AddWithValue("@productName", productName);
                cmd.Parameters.AddWithValue("@price", price);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        return productId;
    }

    static void InsertOrderList(SqlConnection connection, int orderId, int productId, int quantity)
    {
        using (SqlCommand cmd = new SqlCommand("INSERT INTO orderlist (order_orderId, product_productId, [count]) VALUES (@orderId, @productId, @quantity)", connection))
        {
            cmd.Parameters.AddWithValue("@orderId", orderId);
            cmd.Parameters.AddWithValue("@productId", productId);
            cmd.Parameters.AddWithValue("@quantity", quantity);

            cmd.ExecuteNonQuery();
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

    static int GetUserByFioAndEmail(SqlConnection connection, string fio, string email)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT userid FROM [User] WHERE username = @username AND [mail] = @mail", connection))
        {
            cmd.Parameters.AddWithValue("@username", fio);
            cmd.Parameters.AddWithValue("@mail", email);

            object result = cmd.ExecuteScalar();

            return (result != null) ? Convert.ToInt32(result) : -1;
        }
    }

    static int GetProductByName(SqlConnection connection, string productName)
    {
        using (SqlCommand cmd = new SqlCommand("SELECT productid FROM product WHERE productname = @productName", connection))
        {
            cmd.Parameters.AddWithValue("@productName", productName);

            object result = cmd.ExecuteScalar();

            return (result != null) ? Convert.ToInt32(result) : -1;
        }
    }
}
