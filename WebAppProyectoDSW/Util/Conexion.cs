using Microsoft.Data.SqlClient;

namespace WebAppProyectoDSW.Util
{
    public class conexion
    {
        SqlConnection cn = new SqlConnection(@"server = (local);database = Marketec2022;" +
                    "Trusted_Connection = True;" + "MultipleActiveResultSets = True;" +
                    "TrustServerCertificate = False;" + "Encrypt = False");

        public SqlConnection getcn

        {

            get { return cn; }

        }
    }
}
