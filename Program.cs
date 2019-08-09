using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJsonSchema;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net; 

namespace Breaker
{
    class Program
    {
        /// <summary>
        /// Path to a config file
        /// </summary>
        static string configFilePath = "";

        /// <summary>
        /// Log file content
        /// </summary>
        static string log = "";

        /// <summary>
        /// Path to log file
        /// </summary>
        static string logFile = "";

        /// <summary>
        /// Path to diff folder
        /// </summary>
        static string diffFolder = "";

        static void Main(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Application accepts a parameter. The parameter should be a url of the configuration file"); return; }
            if (!File.Exists(args[0])) { Console.WriteLine("You need to provide existing configuration file url as a parameter."); return; }

            configFilePath = args[0];
            PrepareFilesAndFolders();

            string config = File.ReadAllText(configFilePath);
            string[] lines = config.Split(new char[] { '\r' });

            string authorization = lines[0].Replace("\n", "").Trim();

            HttpClient client = prepareClient(authorization);

            string urlPartToRemove = lines[1].Replace("\n", "").Trim();

            for (var i = 2; i < lines.Length; i++)
            {
                HttpResponseMessage response = null;
                var endpointUrl = lines[i].Replace("\n", "").Trim();

                try
                {
                    response = client.GetAsync(endpointUrl).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var schema = JsonSchema.FromSampleJson(response.Content.ReadAsStringAsync().Result);
                        var currentSchema = schema.ToJson().ToString();
                        var oldSchemaFilePath = GetSchemaFilePath(endpointUrl);

                        if (File.Exists(oldSchemaFilePath))
                        {
                            string oldSchema = File.ReadAllText(oldSchemaFilePath);
                            if (oldSchema != currentSchema)
                            {
                                if (!Directory.Exists(GetSchemaFolderPath() + "schema-old\\")) Directory.CreateDirectory(GetSchemaFolderPath() + "schema-old\\");
                                if (!Directory.Exists(GetSchemaFolderPath() + "schema-current\\")) Directory.CreateDirectory(GetSchemaFolderPath() + "schema-current\\");

                                string oldSchemaDiffFilePath = GetSchemaFolderPath() + "schema-old\\" + GetSchemaFileName(endpointUrl.Replace(urlPartToRemove, "")) + ".txt";
                                string currentSchemaDiffFilePath = GetSchemaFolderPath() + "schema-current\\" + GetSchemaFileName(endpointUrl.Replace(urlPartToRemove, "")) + ".txt";

                                File.WriteAllText(oldSchemaDiffFilePath, endpointUrl + "\r\n\r\n" + oldSchema);
                                File.WriteAllText(currentSchemaDiffFilePath, endpointUrl + "\r\n\r\n" + currentSchema);

                                showError(endpointUrl, "Different schema\r\n\r\nOld:\r\n\r\n" + oldSchema + "\r\n\r\nNew:\r\n\r\n" + currentSchema);
                            }
                        }

                        if (args.Length > 1 && args[1] == "save")
                        {
                            
                            if (!System.IO.Directory.Exists(GetSchemaFolderPath())) System.IO.Directory.CreateDirectory(GetSchemaFolderPath());
                            File.WriteAllText(oldSchemaFilePath, currentSchema);
                        }
                    }
                    else
                    {
                        showError(endpointUrl, response.StatusCode.ToString() + "\r\n" + response);
                    }
                }
                catch (Exception ex)
                {
                    showError(endpointUrl, ex.ToString());
                }
            }


            File.WriteAllText(logFile, log);
            Console.WriteLine("Processing done. Log saved to " + logFile);
        }


        /// <summary>
        /// Adds information to a log file about changes in one endpoint
        /// </summary>
        /// <param name="url">Endpoint url</param>
        /// <param name="erro">Information about the endpoint</param>
        static void showError(string url, string erro)
        {
            var line = "********************************************************************\r\n";
            line += url + "\r\n";
            line += "********************************************************************\r\n";
            line += erro;
            line += "\r\n********************************************************************\r\n";
            Console.WriteLine(line);
            log += line;
        }

        /// <summary>
        /// Gets a safe file name from a text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        static string GetSafeFileName(string name, char replace = '_')
        {
            char[] invalids = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalids.Contains(c) ? replace : c).ToArray());
        }

        /// <summary>
        /// Prepares file of a log and diff folder
        /// </summary>
        static void PrepareFilesAndFolders()
        {
            var logFolder = Path.GetDirectoryName(configFilePath);
            if (logFolder != "") logFolder += "\\logs\\";
            logFolder += "logs\\";
            if (!System.IO.Directory.Exists(logFolder)) System.IO.Directory.CreateDirectory(logFolder);

            logFile = logFolder + "log-" + GetSafeFileName(DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString()) + ".txt";

            diffFolder = logFolder + "\\" + GetSafeFileName(DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString());
            System.IO.Directory.CreateDirectory(diffFolder);
        }

        /// <summary>
        /// Gets last schema of an endpoint from a snapshot file
        /// </summary>
        /// <returns></returns>
        static string GetLastSchema(string endpointUrl)
        {
            var fileName = GetSchemaFilePath(endpointUrl);

            if (File.Exists(fileName))
            {
                string oldSchema = File.ReadAllText(fileName);
                return oldSchema;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the path to the file with snapshot
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <returns></returns>
        static string GetSchemaFilePath(string endpointUrl)
        {
            return GetSchemaFolderPath() + GetSchemaFileName(endpointUrl) + ".txt";
        }

        /// <summary>
        /// Gets schema file name
        /// </summary>
        /// <param name="endpointUrl"></param>
        /// <returns></returns>
        static string GetSchemaFileName(string endpointUrl)
        {
            return GetSafeFileName(endpointUrl);
        }

        /// <summary>
        /// Gets the snapshot schema folder path
        /// </summary>
        /// <returns></returns>
        static string GetSchemaFolderPath()
        {
            var folderPath = Path.GetDirectoryName(configFilePath);
            if (folderPath != "") folderPath = folderPath + "\\";
            folderPath += "snapshot\\";
            return folderPath;
        }

        /// <summary>
        /// Prepares client to make requests to endpoints
        /// </summary>
        /// <param name="authorizationString">Adds authorization header with provided text</param>
        /// <returns></returns>
        private static HttpClient prepareClient(string authorizationString)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Add("Authorization", authorizationString);

            return client;
        }

    }
}
