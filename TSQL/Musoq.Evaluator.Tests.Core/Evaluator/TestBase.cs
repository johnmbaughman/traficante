﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Plugins;
using Traficante.TSQL.Schema;
using Environment = System.Environment;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    public class TestBase
    {
        protected CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

        protected CompiledQuery CreateAndRunVirtualMachine<T>(string script, IDictionary<string, IEnumerable<T>> sources)
        {
            var database = new TestSchema<T>(sources);
            return InstanceCreator.CompileForExecution(script, new SchemaProvider(new[] { database }));
        }

        protected void TestMethodTemplate<TResult>(string operation, TResult score)
        {
            var query = $"select {operation} from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(TResult), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(score, table[0][0]);
        }

        static TestBase()
        {
            new Plugins.Environment().SetValue(Constants.NetStandardDllEnvironmentName, EnvironmentUtils.GetOrCreateEnvironmentVariable());
        }
    }
}