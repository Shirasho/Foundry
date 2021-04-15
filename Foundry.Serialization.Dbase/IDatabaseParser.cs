using System.Data;
using System.Threading;
using Foundry.IO;

namespace Foundry.Serialization.Dbase
{
    internal interface IDatabaseParser
    {
        void Parse(ref BufferedBinaryReader reader, DataTable table, in CancellationToken cancellationToken);
    }
}
