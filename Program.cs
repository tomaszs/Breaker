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
        static string log = "";

        static void showError(string url, string erro)
        {
            var line = "********************************************************************\r\n";
            line += "********************************************************************\r\n";
            line += erro;
            line += "********************************************************************\r\n";
            Console.WriteLine(line);
            log += line;
        }

        static string GetSafeFileName(string name, char replace = '_')
        {
            char[] invalids = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalids.Contains(c) ? replace : c).ToArray());
        }

        static void Main(string[] args)
        {
            Console.WriteLine("");
            if (args.Length >= 1)
            {
                if (File.Exists(args[0]))
                {
                    string config = File.ReadAllText(args[0]);
                    string[] lines = config.Split(new char[] { '\n' });

                    string authorization = lines[0].Replace("\r", "").Trim();

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("authorization", authorization);
                    for (var i = 1; i < lines.Length; i++)
                    {
                        HttpResponseMessage response = null;
                        var line = lines[i].Replace("\r", "").Trim();
                        try
                        {
                            response = client.GetAsync(line).Result;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var schema = JsonSchema.FromSampleJson(response.Content.ReadAsStringAsync().Result);
                                var schemaJson = schema.ToJson();
                                var fileName = Path.GetDirectoryName(args[0]) + "\\" + GetSafeFileName(line) + ".txt";

                                if (File.Exists(fileName))
                                {
                                    string oldSchema = File.ReadAllText(fileName);
                                    if (oldSchema != schemaJson.ToString())
                                    {
                                        showError(line, "Different schema\r\n\r\nOld:\r\n\r\n" + oldSchema + "\r\n\r\nNew:\r\n\r\n" + schemaJson.ToString());
                                    }
                                }

                                if (args.Length > 1 && args[1] == "save")
                                {
                                    File.WriteAllText(fileName, schemaJson.ToString());
                                }
                            }
                            else
                            {
                                showError(line, response.StatusCode.ToString() + "\r\n" + response);
                            }
                        }
                        catch (Exception ex)
                        {
                            showError(line, ex.ToString());
                        }
                    }

                    var logFile = Path.GetDirectoryName(args[0]) + "\\log-" + GetSafeFileName(DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString()) + ".txt";
                    File.WriteAllText(logFile, log);
                    Console.WriteLine("Processing done. Log saved to " + logFile);
                }
                else
                {
                    Console.WriteLine("You need to provide existing configuration file url as a parameter.");
                }
            }
            else
            {
                Console.WriteLine("Application accepts a parameter. The parameter should be a url of the configuration file");
            }
        }
    }
}
