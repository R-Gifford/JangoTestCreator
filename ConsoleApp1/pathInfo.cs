using System.Text.RegularExpressions;

namespace OpenAPIDeserialization
{
    public class pathInfo
    {
        //Constructor Thing
        public pathInfo(string path, string httpMethod, string queryType) 
        {
            this.path = path;
            this.httpMethod = httpMethod;
            this.queryType = queryType;
            paramatersUsed = pathToParamatersUsed(path);
            positivePath = PositivePathVariables(path);
            negativePath = NegativePathVariables(path);
            negativePramaters = CreateNegativeParamaters(paramatersUsed);
            postivePramaters = CreatePositiveParamaters(paramatersUsed);
        }

        public string? path { get; set; }
        public string? httpMethod { get; set; }
        public string? queryType { get; set; }
        public MatchCollection? paramatersUsed { get; private set; }
        public string? positivePath { get; private set; }
        public string? negativePath { get; private set; }
        public List<string>? negativePramaters { get; private set; }
        public List<string>? postivePramaters { get; private set; }

        public string PositivePathVariables(string path)
        {   
          string Ppath =  path.Replace("{", "${");
            return Ppath;
        }
        public string NegativePathVariables(string path)
        {
          string Npath = path.Replace("{", "${NEG_");
            return Npath;
        }

        public List<string> CreateNegativeParamaters (MatchCollection parmatersUsed)
        {
            List<string> negativeParamaters = new List<string>();
            foreach (var parm  in parmatersUsed)
            {
              var used  = parm.ToString().Insert(1, "NEG_");
              negativeParamaters.Add(used);
            }

            return negativeParamaters;
        }
        public List<string> CreatePositiveParamaters (MatchCollection parmatersUsed)
        {
            List<string> positiveParamaters = new List<string>();
            foreach (var parm  in parmatersUsed)
            {
                var used  = parm.ToString();
                positiveParamaters .Add(used);
            }

            return positiveParamaters;
        }
    
        public MatchCollection pathToParamatersUsed(string path)
        {
            string pattern = @"\{([^}]+)\}"; // Regular expression pattern to match text between double quotes

            MatchCollection matches = Regex.Matches(path, pattern);
            return matches;
        }
    }
}

