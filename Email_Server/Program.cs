using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Email_Server
{
    class Program
    {

        static SqlConnection sqlConnection;
        static Socket handler;

        static void Main(string[] args)
        {

            //Veri tabanı bağlantısını aç.
            DB_Connect();

            while (true)
            {
                //Soket dinle.
                string sql = Listen();

                //Gelen komut yazma mı? Yoksa çekme mi?
                if (sql.Contains("Insert"))
                {
                    //Veri tabanına yaz.
                    DB_Insert(sql);

                    //İşlem tamamlandı mesajı gönder.
                    SendReply("Completed.");
                }
                else if (sql.Contains("Select"))
                {
                    //Veri tabanından çek.
                    string sonuc = DB_Select(sql);

                    //Sonuçu soketten geri gönder.
                    SendReply(sonuc);
                }
            }
        }

        private static void DB_Connect()
        {
            string connectionString;

            connectionString = @"Data Source = ERDOGAN\SQLEXPRESS; Initial Catalog = DB_Email; Integrated Security = True";
            sqlConnection = new SqlConnection(connectionString);

            sqlConnection.Open();
            Console.WriteLine("Connection Open.");
        }

        private static void DB_Insert(string sql)
        {
            SqlCommand command;
            SqlDataAdapter adapter = new SqlDataAdapter();

            //Komut stringi
            //sql = "Insert into PublicKeys (MailAdress, PublicKey) values('" + "mail2" + "', '" + "Key2" + "')";
            
            //String'ten komut oluştur.
            command = new SqlCommand(sql, sqlConnection);

            //Komutu çalıştır.
            adapter.InsertCommand = command;
            int sonuc = adapter.InsertCommand.ExecuteNonQuery();

            //Komutu temizle.
            command.Dispose();
        }

        private static string DB_Select(string sql)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            string output = "";

            //Komut stringi
            //sql = "Select  MailAdress, PublicKey from PublicKeys where convert (VARCHAR, MailAdress) = '" + mailAdress + "'";

            //String'den komut oluştur.
            command = new SqlCommand(sql, sqlConnection);

            //Komutu çalıştır.
            dataReader = command.ExecuteReader();

            if(dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    output = output + dataReader.GetValue(0) + "\n";
                    command.Dispose();
                    return output;
                }
            }
            else
            {
                Console.WriteLine("Aranan değer bulunamadı.");
                return "-1";
            }
            return null;
        }

        private static string Listen()
        {
            // Bağlanılacak IP'yi al. (Local host IP'si) 
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try
            {
                // TCP'de kullanılacak soketi oluştur.    
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Soketi endpoint'e bağla.  
                listener.Bind(localEndPoint);
                // Aynı anda kaç istek alabileceğini tanımla.
                listener.Listen(10);

                //Dinlemeye başla.
                Console.WriteLine("Waiting for a connection...");
                handler = listener.Accept();

                // Client'tan gelen veri.    
                string data = null;
                byte[] bytes = null;

                //Byte'ları al ve string'e çevir.
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                listener.Close();
                return data;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private static void SendReply(string cevap)
        {
            //Geri gönder.
            byte[] byte_Cevap = Encoding.ASCII.GetBytes(cevap);
            handler.Send(byte_Cevap);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        private static void DB_Delete()
        {
            //Delete data
        }

        private static void DB_Connection_Close()
        {
            //Close connection.
        }
    }
}
