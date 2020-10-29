using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Foundry.IO;
using Foundry.Reflection;

namespace Foundry.Serialization.Dbase.Level5
{
    internal sealed class LevelFiveParser : IDatabaseParser
    {
        public Encoding Encoding { get; }

        public CultureInfo Culture { get; }

        public LevelFiveParser(Encoding encoding, CultureInfo culture)
        {
            Encoding = encoding;
            Culture = culture;
        }

        public void Parse(ref BufferedBinaryReader reader, DataTable table, in CancellationToken cancellationToken)
        {
            var header = new DatabaseHeader(ref reader);
            var fieldDescriptors = ReadFieldDescriptors(ref reader, cancellationToken);

            CreateDataTableColumns(table, fieldDescriptors, cancellationToken);
            CreateDataTableRows(table, ref reader, ref header, fieldDescriptors, cancellationToken);
        }

        private static IReadOnlyCollection<DatabaseFieldDescriptor> ReadFieldDescriptors(ref BufferedBinaryReader br, in CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fields = new List<DatabaseFieldDescriptor>();
            int peekedChar = br.Peek();

            // Per the spec, 13 (0D) marks the end of the field descriptors, and Peek()
            // returns -1 if it can no longer read anything.
            while (peekedChar != 13 && peekedChar != -1)
            {
                fields.Add(new DatabaseFieldDescriptor(ref br));

                cancellationToken.ThrowIfCancellationRequested();
                peekedChar = br.Peek();
            }

            return fields;
        }

        private void CreateDataTableColumns(DataTable dataTable, IReadOnlyCollection<DatabaseFieldDescriptor> fieldDescriptors, in CancellationToken cancellationToken = default)
        {
            // Create the columns in our new DataTable
            foreach (var field in fieldDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var columnType = field.GetValueType();
                string fieldName = Encoding.GetString(field.FieldName);

                if (columnType != null)
                {
                    if (columnType.IsNullable())
                    {
                        throw new NotSupportedException($"DataSet does not support System.Nullable<> (FieldName = {fieldName}).");
                    }
                    dataTable.Columns.Add(new DataColumn(fieldName, columnType));
                }
                else
                {
                    throw new NotSupportedException($"The field type '{field.FieldType}' (FieldName = {fieldName}) is not supported.");
                }
            }
        }

        private void CreateDataTableRows(DataTable dataTable, ref BufferedBinaryReader br, ref DatabaseHeader header, IReadOnlyCollection<DatabaseFieldDescriptor> fields, in CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Skip past the end of the header. 
            br.Seek(header.HeaderLength, SeekOrigin.Begin);

            // Read in all the records.
            for (int counter = 0; counter < header.RecordCount; ++counter)
            {
                // '*' is a deleted record, ' ' is a valid record.
                if (br.Peek() == '*')
                {
                    br.SkipBytes(fields.Sum(f => f.FieldLength));
                    continue;
                }

                // Skip the "Deleted Row" marker.
                br.SkipBytes(1);

                var row = dataTable.NewRow();
                foreach (var field in fields)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string fieldName = Encoding.GetString(field.FieldName).Trim();
                    row[fieldName] = field.GetValue(ref br, Encoding, Culture);
                }

                dataTable.Rows.Add(row);
            }
        }
    }
}
