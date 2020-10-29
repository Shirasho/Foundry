using System.Data;
using System.Text;
using System.Threading;
using Foundry.IO;

namespace Foundry.Serialization.Dbase
{
    internal interface IDatabaseParser
    {
        Encoding Encoding { get; }

        void Parse(ref BufferedBinaryReader reader, DataTable table, in CancellationToken cancellationToken);
    }
}
