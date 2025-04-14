using System.Data;
using Microsoft.Data.SqlClient;
using System.Transactions;
namespace DotNetCoreMVCWith_WebApi.Services
{
    public class AdoDataAdapter
    {
        private readonly IConfiguration _configuration;
        public AdoDataAdapter(IConfiguration configuration)
        {
            _configuration= configuration;
        }
        public DataTable Getallemployees()
        {
            DataTable dataTable = new DataTable();
            using(TransactionScope tranScope = new TransactionScope())
                using(SqlConnection con=new SqlConnection(_configuration.GetConnectionString("Defaultconnection")))
            {
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("select * from Employees", con);
                sqlDataAdapter.Fill(dataTable);
            }
            return dataTable;
        }
    }
}
