using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Models;
using OpenAPIDeserialization;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;

class Program
{

    const string targetFile = @"C:\Users\rgifford\RiderProjects/JangoCreatedTests.jmx";
    const string headerFile = "./lib/jmeterHeader.txt";
    const string footerFile = "./lib/jmeterTemplateFooterDefault.txt";
    const string domainServerName = "billinghelperapi.conservice.com";
    static string jangoFileContent = "";
    static string userHeaderTemplate = File.ReadAllText("../lib/UserDefinedVariableHeader.txt");
    static string userDefinedVarBodyTemplate = File.ReadAllText("../lib/UserDefinedVariableBody.txt");
    static string userFooterTemplate = File.ReadAllText("../lib/UserDefinedVariablesFooter.txt");
    static string httpPathForControllerName = "";

    //static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;


    static void Main(string[] args)
    {
       

        List<pathInfo> allPaths = new List<pathInfo>();
        string jsonString = @"C:\Users\rgifford\Downloads\jsonString.txt";
        OpenApiStreamReader reader = new OpenApiStreamReader();

        using var stream = File.OpenRead(jsonString);
        OpenApiDocument document = reader.Read(stream, out var diagnostic);

        Dictionary<string, OpenApiPathItem> paths = new Dictionary<string, OpenApiPathItem>();
        // Populate List of AllPaths.
        foreach (var path in document.Paths)
        {
            paths.Add(path.Key, path.Value);
        }

        foreach (var item in paths)
        {
            Console.WriteLine(item.Key);
            Console.WriteLine(item.Value.Operations.Keys.First());
            foreach (var pram in item.Value.Operations.Values)
            {

                string pramIn = "";
                if (pram.Parameters.Count > 0 && pram.Parameters.First() != null)
                {
                    pramIn = pram.Parameters.FirstOrDefault().In.ToString();
                }

                pathInfo pathInfo = new pathInfo(
                     item.Key, item.Value.Operations.Keys.First().ToString(), pramIn);

                allPaths.Add(pathInfo);

            }

            parseHeader();
            createAllNeededElementsForPosOrNegController("Postive_Tests", false);
            createAllNeededElementsForPosOrNegController("Negative_Tests", true);
            createUserDefineVariablesBlock();
            parseFooter();


            File.WriteAllText(targetFile, jangoFileContent);

            //logicForCreatingFile();

        }

        //Start creating Document




        // This is where the file is being created

        void logicForCreatingFile()
        {
            if (File.Exists(targetFile)) File.Delete(targetFile);
            
            parseHeader();
            createAllNeededElementsForPosOrNegController("Postive_Tests", false);
            createAllNeededElementsForPosOrNegController("Negative_Tests", true);
            createUserDefineVariablesBlock();
            parseFooter();


            File.WriteAllText(targetFile, jangoFileContent);

                   
        }




        /* TODO
        Check if file exists 
            yes - delete
            no - create
        Create header for file
            create positive transction controller
                create postive tests for each endpoint
            Create Negative Transction controller
                create neagive tests for each endpoint
            Create user defined variables
        Creater footer for file

        FINISH 


        */


        /// Below are Functions to create the files.  --------------------------------- 
        /// ---------------------------------------------------------------------------
        /// 


        void parseHeader()
        {
            string headerFileContent = File.ReadAllText(headerFile);
            jangoFileContent += headerFileContent;
        }

        void parseFooter()
        {
            string footerFileContent = File.ReadAllText(footerFile);
            jangoFileContent += footerFileContent;
        }

        string transactionCreationLogicFlow(string currentHttpPathFromLoop, string previousHttpPath)

        {
            try
            {
                string httpPathForControllerName = currentHttpPathFromLoop.Split("/api/")[1].Split("/")[0];


                if (httpPathForControllerName != previousHttpPath && previousHttpPath == "")
                {
                    writeTransactionController(httpPathForControllerName, true);
                }
                else if (httpPathForControllerName != previousHttpPath && previousHttpPath != "")
                {
                    writeTransactionController(httpPathForControllerName, false);
                }
                else { }
                return httpPathForControllerName;

            }
            catch
            {
                return httpPathForControllerName;
            }

        }
            // Number of <HashTree> depends on if it is the first or following transaction controller
            void writeTransactionController(string httpPathForControllerName, bool isFirstHeader)
            {
                string newContent = "";

                if (isFirstHeader)
                {
                    newContent = File.ReadAllText("../lib/jmeterTemplate-TransactionControllerFIRST.txt");
                }
                else
                {
                    newContent = File.ReadAllText("../lib/jmeterTemplate-TransactionControllerAfterFirst.txt");
                }

                newContent = newContent.Replace("!!_TESTNAME_REPLACE_ME_!!", httpPathForControllerName);

                jangoFileContent += newContent;

            }



            void writeHttpSampler(string domainServerName, string httpPath, string httpMethod)
            {

                string requestName = httpMethod + httpPath;
                string newContent =
                    File.ReadAllText(Path.GetFullPath("./lib/HTTP_REQUEST_TEMPLATE200.txt"));
                newContent = newContent.Replace("!!_HTTPTESTNAME_!!", requestName);
                newContent = newContent.Replace("!!_DOMAINSERVERNAME_!", domainServerName);
                newContent = newContent.Replace("!!_HTTP_PATH__REPLACE_!!", httpPath);
                newContent = newContent.Replace("!!_HTTP_METHOD_REPLACE_!!", httpMethod);
                jangoFileContent += newContent;
            }



            void createAllNeededElementsForPosOrNegController(string typeOfTests, bool createPositiveTests)
            {
                string previousHttpPath = "";
                HashSet<string> paramaters = new HashSet<string>();
                writeTransactionController(typeOfTests, true);

                if (createPositiveTests)
                {
                    foreach (var endpoint in allPaths)
                    {
                        string path = endpoint.positivePath;
                        path = endpoint.negativePath;

                        previousHttpPath = transactionCreationLogicFlow(path, previousHttpPath);
                        writeHttpSampler(domainServerName, path, endpoint.httpMethod);


                    }
                }
                //Needed to close top level Controller
                jangoFileContent += "</hashTree></hashTree></hashTree>";
            }


            /// Function to Create User Defined Variables
            void createUserDefineVariablesBlock()
            {
                jangoFileContent += userHeaderTemplate; // Create header of the User Defined Variables Section
                createUserDefinedVariables(false); // Create positive Paramaters
                createUserDefinedVariables(true);
                jangoFileContent += userFooterTemplate;

            }

            void createUserDefinedVariables(bool useNegativeParamaters)
            {
                HashSet<string> userVariables = new HashSet<string>();


                foreach (var item in allPaths)
                {
                    var paramaterType = item.postivePramaters;
                    if (useNegativeParamaters) paramaterType = item.negativePramaters;

                    if (paramaterType != null)
                    {
                        foreach (var param in paramaterType)
                        {
                            userVariables.Add(param);
                        }
                    }
                }

                if (useNegativeParamaters)
                {
                    foreach (var variable in userVariables)
                    {
                        String updatedTemplateContent = userDefinedVarBodyTemplate;
                        updatedTemplateContent = updatedTemplateContent.Replace("!!_REPLACE_ME_WITH_PRAM_!!", variable).Replace("!!_VALUE_!!", "-1");
                        jangoFileContent += updatedTemplateContent;
                    }

                }
                else
                {
                    foreach (var variable in userVariables)
                    {
                        String updatedTemplateContent = userDefinedVarBodyTemplate;
                        updatedTemplateContent = updatedTemplateContent.Replace("!!_REPLACE_ME_WITH_PRAM_!!", variable).Replace("!!_VALUE_!!", "28080");
                        jangoFileContent += updatedTemplateContent;
                    }
                }

            }

            /// CURRENT -- Create User Defined Variables in a Generic way
            /// How to get the list out of the items .... 

        }

    }


