using System;
using System.Data;
using System.Text;
using StructureMap.DataAccess.Parameters;

namespace StructureMap.DataAccess.Parameterization
{
    public class ParameterizedCommandBuilder
    {
        private readonly string _commandTemplate;
        private readonly IDatabaseEngine _engine;
        private IDbCommand _command;
        private StringBuilder _commandTextBuilder;
        private bool _hasBuilt = false;
        private string[] _parameterNames;
        private ParameterCollection _parameters;

        public ParameterizedCommandBuilder(IDatabaseEngine engine, string commandTemplate)
        {
            _engine = engine;
            _commandTemplate = commandTemplate;
            _parameters = new ParameterCollection();

            TemplateParser parser = new TemplateParser(_commandTemplate);
            _parameterNames = parser.Parse();

            _commandTextBuilder = new StringBuilder(_commandTemplate);
            _command = _engine.GetCommand();
        }

        public IDbCommand Command
        {
            get { return _command; }
        }

        public ParameterCollection Parameters
        {
            get { return _parameters; }
        }

        public void Build()
        {
            if (_hasBuilt)
            {
                throw new ApplicationException("Build() can only be executed once");
            }

            _hasBuilt = true;

            foreach (string parameterName in _parameterNames)
            {
                string innerParameterName = _engine.GetParameterName(parameterName);

                substituteDatabaseParameterName(parameterName, innerParameterName);

                IDataParameter innerParameter = _command.CreateParameter();
                innerParameter.ParameterName = innerParameterName;

                _command.Parameters.Add(innerParameter);
                _parameters.AddParameter(new Parameter(innerParameter, parameterName));
            }

            _command.CommandText = _commandTextBuilder.ToString();
            _command.CommandType = CommandType.Text;
        }

        private void substituteDatabaseParameterName(string parameterName, string innerParameterName)
        {
            string template = "{" + parameterName + "}";
            _commandTextBuilder.Replace(template, innerParameterName);
        }
    }
}