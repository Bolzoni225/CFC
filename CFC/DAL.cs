using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using System.ComponentModel;

namespace CFC
{
    public class DAL
    {
        SqlConnection con;
      ConnectionStringSettings ConSetting = ConfigurationManager.ConnectionStrings["defautconnection"];
        
       
        public string ConString()
        {
            return ConSetting.ConnectionString;
        }

        public void OuverTureConnexion()
        {
            con = new SqlConnection(ConString());
            con.Open();
        }

        public void Fermetureconnextion()
        {
            con.Close();
        }

        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();

            var properties = typeof(T).GetProperties();

            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                try
                {
                    foreach (var pro in properties)
                    {

                        if (columnNames.Contains(pro.Name))
                            ////pro.SetValue(objT, row[pro.Name]);
                            try
                            {
                                pro.SetValue(objT, row[pro.Name] ?? "0", null);
                            }
                            catch
                            {
                                pro.SetValue(objT, null);
                            }

                    }

                    return objT;
                }
                catch
                {
                    return Activator.CreateInstance<T>();
                }

            }).ToList();

        }




        public List<T> getStoredProcedureExecutionResult<T>(string procedureName, string pParamters, string activite = null, int etape=0 )
        {
            //var result = ExecutionStoredProcedure(procedureName, pParamters);

            //SqlDataReader reader;
            OuverTureConnexion();
            
            SqlCommand cmd_execution = new SqlCommand();
            cmd_execution.Connection = con;
            cmd_execution.CommandType = CommandType.StoredProcedure;
            cmd_execution.CommandText = procedureName;
            cmd_execution.CommandTimeout = 300;
            if (pParamters != null)
            {
                //foreach (var singleParam in pParamters)

                    cmd_execution.Parameters.AddWithValue("NATURECODE", pParamters);
                if(procedureName!= "GET_NATUREPOSITION")
                {
                    cmd_execution.Parameters.AddWithValue("ACTIVITECODE", activite);
                    //cmd_execution.Parameters.AddWithValue("ETAPE", etape);
                }
               
            }
            

            using (SqlDataReader reader = cmd_execution.ExecuteReader())
            {
                using (System.Data.DataTable dTable = new System.Data.DataTable())
                {
                    cmd_execution.Parameters.Clear();
                    dTable.Load(reader);
                    reader.Close();
                    reader.Dispose();
                    return ConvertToList<T>(dTable);

                };
              
            }


            Fermetureconnextion();

        }


        public List<T> getStoredProcedureExecution<T>(string procedureName, List<SqlParameter> pParamters)
        {
            //var result = ExecutionStoredProcedure(procedureName, pParamters);

            //SqlDataReader reader;
            OuverTureConnexion();

            SqlCommand cmd_execution = new SqlCommand();
            cmd_execution.Connection = con;
            cmd_execution.CommandType = CommandType.StoredProcedure;
            cmd_execution.CommandText = procedureName;
            cmd_execution.CommandTimeout = 300;
            if (pParamters != null)
            {
                foreach (var singleParam in pParamters)
                {
                    cmd_execution.Parameters.AddWithValue(singleParam.ParameterName, singleParam.Value);
                }
              
            }


            using (SqlDataReader reader = cmd_execution.ExecuteReader())
            {
                using (System.Data.DataTable dTable = new System.Data.DataTable())
                {
                    cmd_execution.Parameters.Clear();
                    dTable.Load(reader);
                    reader.Close();
                    reader.Dispose();
                    return ConvertToList<T>(dTable);

                };

            }


            Fermetureconnextion();

        }

        public void ExecSimpleQuery(string Query, List<SqlParameter> parametres)
        {
            OuverTureConnexion();
            SqlDataAdapter adapt = new SqlDataAdapter(Query, ConString());
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = Query;
            foreach (var parametre in parametres)
            {
                cmd.Parameters.AddWithValue(parametre.ParameterName, parametre.Value);
                
            }
            var reader = cmd.ExecuteReader();
           
            Fermetureconnextion();
        }


        public System.Data.DataTable SetData(String connectionString, String pCommandeText, SqlParameter pParameters, List<SqlParameter> param)
        {


            using (SqlConnection con = new SqlConnection(ConString()))
            {
                using (SqlCommand command = new SqlCommand(pCommandeText))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 300;
                    command.Connection = con;

                    command.Parameters.Clear();
                    command.Parameters.Add(pParameters);
                    foreach (var parametre in param)
                    {
                        command.Parameters.AddWithValue(parametre.ParameterName, parametre.Value);

                    }

                    con.Open();


                    //SqlDataReader rdr = command.ExecuteReader();
                    //DataTable dt = new DataTable();
                    //dt.Load(rdr);
                    //return dt;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        using (System.Data.DataTable dTable = new System.Data.DataTable())
                        {
                            command.Parameters.Clear();
                            dTable.Load(reader);
                            reader.Close();
                            reader.Dispose();
                            return dTable;
                        };
                    }

                }
            }



            return new DataTable();
        }


        /// <summary>
        /// converti une liste en datatable pour l'utiliser comme paramètre table pour une procédure stockée
        /// </summary>
        /// <typeparam name="T">type générique constituant la liste</typeparam>
        /// <param name="data">la variable contenant les valeurs de la liste</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                //table.Columns.Add(prop.Name, prop.PropertyType);
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);//add 29/05/2018 cso
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }


    }
}