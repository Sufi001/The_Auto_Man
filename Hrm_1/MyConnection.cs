using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CustomerRec
{
    class MyConnection
    {
        private SqlConnection myconnection;
        public MyConnection()
        {
            try
            {
                //System.Configuration.ConfigurationManager.ConnectionStrings["DBCon"].ConnectionString;
                string connectionString = ConfigurationManager.ConnectionStrings["sms1Entities1"].ConnectionString;
                //"Server=localhost\\SQLSERVER2012;Database=alsadiqpos;User Id=sa;Password = 123456; ";
                //"Data Source=DESKTOP-BUBKUD3\SQLSERVER2012;Initial Catalog=alsadiqpos;Integrated Security=True"
                //@"SERVER=.\SQLEXPRESS; Initial Catalog= AL-SADIQ DATABASE; Integrated Security = True;";
                myconnection = new SqlConnection(connectionString);
            }
            catch (SqlException ex)
            {
            }
        }
        public DataSet Select(string query)
        {
            try
            {
                DataSet dt = new DataSet();
                 myconnection.Open();
                SqlCommand cmd = new SqlCommand(query, myconnection);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                cmd.CommandType = CommandType.Text;
                sda.Fill(dt);
                myconnection.Close();
                return dt;
            }
            catch (Exception ex)
            {
                //  MessageBox.Show("You are not connected to Database.");
                return null;
            }
        }

        public List<string> ComboBoxFill(String QUERY)
        {
            //Create a list to store the result
            List<string> list = new List<string>();

            //Open connection
            myconnection.Open();


            SqlCommand cmd = new SqlCommand(QUERY, myconnection);
            //Create a data reader and Execute the command
            SqlDataReader dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                String s;
                s = dataReader[0].ToString();
                list.Add(s);

            }

            //close Data Reader
            dataReader.Close();

            //close Connection
            myconnection.Close();

            //return list to be displayed
            return list;


        }


        public void Query(string QUERY)
        {

            myconnection.Open();
            SqlCommand com = new SqlCommand();
            com.CommandText = QUERY;
            com.Connection = myconnection;
            com.ExecuteNonQuery();
            myconnection.Close();


        }



        public Int32 QueryWithId(string QUERY)
        {

            Int32 newProdID = 0;
            myconnection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = QUERY;
            cmd.Connection = myconnection;
            newProdID = (Int32)cmd.ExecuteScalar();
            myconnection.Close();
            return newProdID;


        }

        public int resultquery(string QUERY)
        {
            myconnection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = QUERY;
            cmd.Connection = myconnection;
            int i = ((int)cmd.ExecuteScalar());
            myconnection.Close();
            return i;

        }

        public int nonquery(string QUERY)
        {
            myconnection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = QUERY;
            cmd.Connection = myconnection;
            int i = ((int)cmd.ExecuteNonQuery());
            myconnection.Close();
            return i;

        }
    }
}
