﻿using System.Collections.Generic;
using System.Reflection;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Helpers;
using Traficante.TSQL.Schema.Managers;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Evaluator.Tables
{
    internal class TransitionSchema : BaseSchema
    {
        private readonly ITable _table;

        public TransitionSchema(string name, ITable table)
            : base(name, CreateLibrary())
        {
            _table = table;
        }

        public override ITable GetTableByName(string schema, string name)
        {
            return _table;
        }

        //public override RowSource GetRowSource(string schema, string name, RuntimeContext interCommunicator, params object[] parameters)
        //{
        //    return new TransientVariableSource(name);
        //}

        private static MethodsAggregator CreateLibrary()
        {
            var methodsManager = new MethodsManager();
            var propertiesManager = new PropertiesManager();

            var library = new TransitionLibrary();

            methodsManager.RegisterLibraries(library);
            propertiesManager.RegisterProperties(library);

            return new MethodsAggregator(methodsManager, propertiesManager);
        }

        //public override SchemaMethodInfo[] GetConstructors(string schema)
        //{
        //    var constructors = new List<SchemaMethodInfo>();

        //    constructors.AddRange(TypeHelper.GetSchemaMethodInfosForType<TransientVariableSource>("transient"));

        //    return constructors.ToArray();
        //}
    }
}