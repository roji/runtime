// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace System.Data.OleDb.Tests
{
    public static class Helpers
    {
        public const string IsDriverAvailable = nameof(Helpers) + "." + nameof(GetIsDriverAvailable);
        public static bool GetIsDriverAvailable() => IsAvailable;

        public static bool IsAvailable { get; private set; }
        public static string ConnectionString { get; private set; }
        public static string ProviderName { get; private set; }

        public static string GetTableName(string memberName) => memberName + ".csv";

        static Helpers()
        {
            // Detect which OLE providers are available in the system.
            // Note that providers may be installed for x86 but not for x64 or vice versa.
            DataTable table = (new OleDbEnumerator()).GetElements();
            DataColumn providersRegistered = table.Columns["SOURCES_NAME"];
            List<string> providerNames = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                providerNames.Add((string)row[providersRegistered]);
            }

            // skip if x86 or if the expected driver not available
            // For the following culture check: https://github.com/dotnet/runtime/issues/29969
            // if (CultureInfo.CurrentCulture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
            {
                if (TryUsingProviderName(providerNames, "MSOLEDBSQL19"))
                {
                    return;
                }

                if (TryUsingProviderName(providerNames, "MSOLEDBSQL"))
                {
                    return;
                }
            }

            static bool TryUsingProviderName(List<string> registeredProviders, string providerName)
            {
                if (!registeredProviders.Contains(providerName))
                {
                    return false;
                }

                string connectionString = $"""Provider={providerName};Server=(localdb)\\MSSQLLocalDB;Integrated Security=SSPI""";

                // Even when a provider is present in the installed providers list, it may not be available e.g. because of
                // a 32/64 bit mismatch.
                try
                {
                    using var connection = new OleDbConnection(ConnectionString);
                    connection.Open();

                    ConnectionString = connectionString;
                    ProviderName = providerName;
                    IsAvailable = true;

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
